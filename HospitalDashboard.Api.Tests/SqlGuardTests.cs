using HospitalDashboard.Api.Ai;
using Xunit;

namespace HospitalDashboard.Api.Tests;

public class SqlGuardTests
{
    [Theory]
    [InlineData("SELECT * FROM Patients")]
    [InlineData("select count(*) from Admissions")]
    [InlineData("  SELECT * FROM Beds WHERE Status = 'Available'  ")]
    [InlineData("SELECT * FROM Beds;")]
    [InlineData("WITH a AS (SELECT 1 AS n) SELECT n FROM a")]
    public void Allows_read_only_queries(string sql)
    {
        Assert.True(SqlGuard.Check(sql).Ok);
    }

    [Fact]
    public void Strips_a_single_trailing_semicolon()
    {
        Assert.Equal("SELECT * FROM Beds", SqlGuard.Check("SELECT * FROM Beds;").Sql);
    }

    [Theory]
    [InlineData("DROP TABLE Patients", "Only SELECT")]
    [InlineData("DELETE FROM Patients", "Only SELECT")]
    [InlineData("UPDATE Beds SET Status = 'x'", "Only SELECT")]
    [InlineData("EXEC sp_who", "Only SELECT")]
    [InlineData("SELECT * FROM Patients; DROP TABLE Patients", "single statement")]
    [InlineData("SELECT * FROM Patients -- x", "Comments")]
    [InlineData("SELECT * FROM Patients /* x */", "Comments")]
    [InlineData("SELECT * INTO Copy FROM Patients", "forbidden keyword")]
    [InlineData("SELECT * FROM sys.tables", "forbidden keyword")]
    [InlineData("SELECT * FROM Staff", "restricted")]
    [InlineData("sElEcT * FrOm sTaFf", "restricted")]
    [InlineData("SELECT PasswordHash FROM Patients", "restricted")]
    [InlineData("SELECT * FROM AuditLogs", "restricted")]
    [InlineData("", "Empty")]
    [InlineData("   ", "Empty")]
    public void Rejects_unsafe_queries(string sql, string expectedError)
    {
        var result = SqlGuard.Check(sql);
        Assert.False(result.Ok);
        Assert.Contains(expectedError, result.Error);
    }

    [Fact]
    public void Rejects_null_input()
    {
        var result = SqlGuard.Check(null!);
        Assert.False(result.Ok);
    }
}
