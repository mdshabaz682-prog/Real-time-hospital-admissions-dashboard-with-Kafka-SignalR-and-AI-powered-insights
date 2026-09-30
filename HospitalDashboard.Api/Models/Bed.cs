using System.ComponentModel.DataAnnotations;

namespace HospitalDashboard.Api.Models;

public class Bed
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(50)]
    public string Ward { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string BedNumber { get; set; } = string.Empty;

    [MaxLength(20)]
    public string Status { get; set; } = "Available";

    public ICollection<Admission> Admissions { get; set; } = new List<Admission>();
}
