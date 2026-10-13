using Microsoft.EntityFrameworkCore;

namespace HospitalDashboard.Api.Ai;

public static class AiEndpoints
{
    public record QueryRequest(string Question);

    public static void MapAiEndpoints(this WebApplication app)
    {
        app.MapPost("/api/ai/query", async (QueryRequest req, HospitalDashboardContext db,
            ClaudeClient claude, CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(req.Question) || req.Question.Length > 500)
                return Results.BadRequest(new { error = "Question is required (max 500 characters)." });

            var system =
                "You translate questions about a hospital admissions database into ONE read-only " +
                "Microsoft SQL Server SELECT statement. Reply with the SQL only: no explanation, " +
                "no markdown, no semicolon, no comments. Use only these tables and columns:\n" +
                SchemaBuilder.Build(db) +
                "\nBeds.Status values: Available, Reserved, Occupied, Cleaning, OutOfService. " +
                "Admissions.Status values: Admitted, Discharged. Dates are UTC. " +
                "Always limit results with TOP 100 unless the question asks for a count.";

            string raw;
            try { raw = await claude.AskAsync(system, req.Question, ct); }
            catch (Exception ex) { return Results.Problem(ex.Message, statusCode: 502); }

            var sql = raw.Replace("```sql", "").Replace("```", "").Trim();
            var check = SqlGuard.Check(sql);
            if (!check.Ok)
                return Results.BadRequest(new { error = check.Error, sql });

            var conn = db.Database.GetDbConnection();
            await conn.OpenAsync(ct);
            try
            {
                await using var tx = await conn.BeginTransactionAsync(ct);
                await using var cmd = conn.CreateCommand();
                cmd.Transaction = tx;
                cmd.CommandText = check.Sql;
                cmd.CommandTimeout = 10;

                var rows = new List<Dictionary<string, object?>>();
                await using (var reader = await cmd.ExecuteReaderAsync(ct))
                {
                    while (await reader.ReadAsync(ct) && rows.Count < 200)
                    {
                        var row = new Dictionary<string, object?>();
                        for (var i = 0; i < reader.FieldCount; i++)
                            row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                        rows.Add(row);
                    }
                }
                await tx.RollbackAsync(ct);
                return Results.Ok(new { sql = check.Sql, rows });
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = "Query failed: " + ex.Message, sql = check.Sql });
            }
            finally
            {
                await conn.CloseAsync();
            }
        }).RequireAuthorization();
    }
}