using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using HospitalDashboard.Api.Models;

namespace HospitalDashboard.Api.Endpoints;

public static class PatientEndpoints
{
    public static void MapPatientEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/patients").WithTags("Patients").RequireAuthorization();

        group.MapGet("/", async (HospitalDashboardContext db) =>
            await db.Patients.ToListAsync());

        group.MapGet("/{id}", async (Guid id, HospitalDashboardContext db) =>
            await db.Patients.FindAsync(id) is Patient p ? Results.Ok(p) : Results.NotFound());

        group.MapPost("/", async (Patient patient, HospitalDashboardContext db) =>
        {
            db.Patients.Add(patient);
            await db.SaveChangesAsync();
            return Results.Created($"/api/patients/{patient.Id}", patient);
        });

        group.MapPut("/{id}", async (Guid id, Patient input, HospitalDashboardContext db) =>
        {
            var patient = await db.Patients.FindAsync(id);
            if (patient is null) return Results.NotFound();

            patient.FirstName = input.FirstName;
            patient.LastName = input.LastName;
            patient.Dob = input.Dob;
            patient.Gender = input.Gender;
            patient.ContactPhone = input.ContactPhone;

            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        group.MapDelete("/{id}", async (Guid id, HospitalDashboardContext db) =>
        {
            var patient = await db.Patients.FindAsync(id);
            if (patient is null) return Results.NotFound();

            db.Patients.Remove(patient);
            await db.SaveChangesAsync();
            return Results.NoContent();
        }).RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" });
    }
}
