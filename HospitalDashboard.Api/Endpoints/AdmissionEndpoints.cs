using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using HospitalDashboard.Api.Models;

namespace HospitalDashboard.Api.Endpoints;

public static class AdmissionEndpoints
{
    public static void MapAdmissionEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/admissions").WithTags("Admissions").RequireAuthorization();

        group.MapGet("/", async (HospitalDashboardContext db) =>
            await db.Admissions.Include(a => a.Patient).Include(a => a.Bed).Include(a => a.Staff).ToListAsync());

        group.MapGet("/{id}", async (Guid id, HospitalDashboardContext db) =>
            await db.Admissions.Include(a => a.Patient).Include(a => a.Bed).Include(a => a.Staff)
                .FirstOrDefaultAsync(a => a.Id == id) is Admission a ? Results.Ok(a) : Results.NotFound());

        group.MapPost("/", async (Admission admission, HospitalDashboardContext db) =>
        {
            var patientExists = await db.Patients.AnyAsync(p => p.Id == admission.PatientId);
            if (!patientExists) return Results.BadRequest($"Patient {admission.PatientId} does not exist.");

            var bedExists = await db.Beds.AnyAsync(b => b.Id == admission.BedId);
            if (!bedExists) return Results.BadRequest($"Bed {admission.BedId} does not exist.");

            db.Admissions.Add(admission);
            await db.SaveChangesAsync();
            return Results.Created($"/api/admissions/{admission.Id}", admission);
        });

        group.MapPut("/{id}", async (Guid id, Admission input, HospitalDashboardContext db) =>
        {
            var admission = await db.Admissions.FindAsync(id);
            if (admission is null) return Results.NotFound();

            admission.Status = input.Status;
            admission.DischargedAt = input.DischargedAt;
            admission.StaffId = input.StaffId;

            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        group.MapDelete("/{id}", async (Guid id, HospitalDashboardContext db) =>
        {
            var admission = await db.Admissions.FindAsync(id);
            if (admission is null) return Results.NotFound();

            db.Admissions.Remove(admission);
            await db.SaveChangesAsync();
            return Results.NoContent();
        }).RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" });
    }
}
