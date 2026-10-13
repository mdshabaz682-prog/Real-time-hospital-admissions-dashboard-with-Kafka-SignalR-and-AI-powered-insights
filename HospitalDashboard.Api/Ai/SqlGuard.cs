using System.Text.RegularExpressions;

namespace HospitalDashboard.Api.Ai;

public static class SqlGuard
{
    static readonly Regex Forbidden = new(
        @"\b(insert|update|delete|drop|alter|create|truncate|merge|exec|execute|grant|revoke|deny|backup|restore|into|openrowset|opendatasource|waitfor|shutdown|dbcc|use|sys|information_schema)\b|\bxp_\w+|\bsp_\w+",
        RegexOptions.IgnoreCase);

    static readonly Regex Blocked = new(
        @"\b(staff|auditlogs|passwordhash|__efmigrationshistory)\b",
        RegexOptions.IgnoreCase);

    public static (bool Ok, string? Error, string Sql) Check(string sql)
    {
        sql = (sql ?? "").Trim();
        if (sql.EndsWith(';')) sql = sql[..^1].TrimEnd();

        if (sql.Length == 0) return (false, "Empty query.", sql);
        if (sql.Contains(';')) return (false, "Only a single statement is allowed.", sql);
        if (sql.Contains("--") || sql.Contains("/*")) return (false, "Comments are not allowed.", sql);

        var lower = sql.TrimStart().ToLowerInvariant();
        if (!(lower.StartsWith("select") || lower.StartsWith("with")))
            return (false, "Only SELECT queries are allowed.", sql);

        if (Forbidden.IsMatch(sql)) return (false, "Query contains a forbidden keyword.", sql);
        if (Blocked.IsMatch(sql)) return (false, "Query touches a restricted table or column.", sql);

        return (true, null, sql);
    }
}