using HospitalDashboard.Api.Models;

namespace HospitalDashboard.Api.Seed;

public static class DataSeeder
{
    public static void SeedData(HospitalDashboardContext db)
    {
        if (db.Patients.Any()) return;

        var staff = new List<Staff>
        {
            new() {
                Name = "Shabaaz Mohammad", Role = "Admin", Email = "shabaaz@hospital.example",
                Username = "shabaaz.md", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!")
            },
            new() {
                Name = "Jilani Shaik", Role = "Nurse", Email = "j.shaik@hospital.example",
                Username = "jilani.shaik", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!")
            },
        };

        var beds = new List<Bed>
        {
            new() { Ward = "ICU", BedNumber = "101", Status = "Occupied" },
            new() { Ward = "ICU", BedNumber = "102", Status = "Available" },
            new() { Ward = "General", BedNumber = "201", Status = "Available" },
            new() { Ward = "General", BedNumber = "202", Status = "Occupied" },
            new() { Ward = "Pediatrics", BedNumber = "301", Status = "Available" },
        };

        var patients = new List<Patient>
        {
            new() { FirstName = "Maria", LastName = "Gonzalez", Dob = new DateOnly(1985, 3, 12), Gender = "Female", ContactPhone = "555-0101" },
            new() { FirstName = "David", LastName = "Kim", Dob = new DateOnly(1972, 11, 4), Gender = "Male", ContactPhone = "555-0102" },
            new() { FirstName = "Aisha", LastName = "Patel", Dob = new DateOnly(1998, 7, 22), Gender = "Female", ContactPhone = "555-0103" },
        };

        db.Staff.AddRange(staff);
        db.Beds.AddRange(beds);
        db.Patients.AddRange(patients);
        db.SaveChanges();

        var admissions = new List<Admission>
        {
            new() { PatientId = patients[0].Id, BedId = beds[0].Id, StaffId = staff[0].Id, Status = "Admitted" },
            new() { PatientId = patients[1].Id, BedId = beds[3].Id, StaffId = staff[1].Id, Status = "Admitted" },
        };

        db.Admissions.AddRange(admissions);
        db.SaveChanges();
    }
}
