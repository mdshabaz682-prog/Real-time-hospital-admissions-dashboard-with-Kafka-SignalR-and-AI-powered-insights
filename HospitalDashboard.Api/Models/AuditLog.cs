using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HospitalDashboard.Api.Models;

public class AuditLog
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid StaffId { get; set; }
    [ForeignKey(nameof(StaffId))]
    public Staff? Staff { get; set; }

    [Required, MaxLength(50)]
    public string Action { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string Entity { get; set; } = string.Empty;

    public Guid EntityId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
