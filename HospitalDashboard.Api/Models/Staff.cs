using System.ComponentModel.DataAnnotations;

namespace HospitalDashboard.Api.Models;

public class Staff
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string Role { get; set; } = string.Empty;

    [MaxLength(150)]
    public string? Email { get; set; }

    [Required, MaxLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    public ICollection<Admission> Admissions { get; set; } = new List<Admission>();
    public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
}
