using Microsoft.EntityFrameworkCore;
using HospitalDashboard.Api.Models;

namespace HospitalDashboard.Api;

public class HospitalDashboardContext : DbContext
{
    public HospitalDashboardContext(DbContextOptions<HospitalDashboardContext> options)
        : base(options) { }

    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<Bed> Beds => Set<Bed>();
    public DbSet<Staff> Staff => Set<Staff>();
    public DbSet<Admission> Admissions => Set<Admission>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
}
