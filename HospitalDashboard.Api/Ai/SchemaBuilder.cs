using System.Text;
using Microsoft.EntityFrameworkCore;

namespace HospitalDashboard.Api.Ai;

public static class SchemaBuilder
{
    static readonly HashSet<string> Hidden = new() { "Staff", "AuditLogs" };

    public static string Build(DbContext db)
    {
        var sb = new StringBuilder();
        foreach (var e in db.Model.GetEntityTypes())
        {
            var table = e.GetTableName();
            if (table is null || Hidden.Contains(table)) continue;

            var cols = e.GetProperties().Select(p =>
                $"{p.GetColumnName()} {(Nullable.GetUnderlyingType(p.ClrType) ?? p.ClrType).Name}");
            sb.AppendLine($"{table}({string.Join(", ", cols)})");

            foreach (var fk in e.GetForeignKeys())
            {
                var principal = fk.PrincipalEntityType.GetTableName();
                if (principal is null || Hidden.Contains(principal)) continue;
                var from = string.Join(",", fk.Properties.Select(p => p.GetColumnName()));
                sb.AppendLine($"  FK: {table}.{from} -> {principal}");
            }
        }
        return sb.ToString();
    }
}