using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HospitalDashboard.Api.Models;

public class Admission
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid PatientId { get; set; }
    [ForeignKey(nameof(PatientId))]
    public Patient? Patient { get; set; }

    [Required]
    public Guid BedId { get; set; }
    [ForeignKey(nameof(BedId))]
    public Bed? Bed { get; set; }

    public Guid? StaffId { get; set; }
    [ForeignKey(nameof(StaffId))]
    public Staff? Staff { get; set; }

    public DateTime AdmittedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DischargedAt { get; set; }

    [MaxLength(20)]
    public string Status { get; set; } = "Admitted";
}
