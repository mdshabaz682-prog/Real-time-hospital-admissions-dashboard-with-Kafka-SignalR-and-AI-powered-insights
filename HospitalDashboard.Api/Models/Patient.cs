using System.ComponentModel.DataAnnotations;

namespace HospitalDashboard.Api.Models;

public class Patient
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    public DateOnly Dob { get; set; }

    [MaxLength(20)]
    public string? Gender { get; set; }

    [MaxLength(20)]
    public string? ContactPhone { get; set; }

    public ICollection<Admission> Admissions { get; set; } = new List<Admission>();
}
