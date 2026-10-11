using System.Security.Claims;
using HospitalDashboard.Api.Messaging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using HospitalDashboard.Api.Hubs;
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

        
                group.MapPost("/", async (Admission admission, ClaimsPrincipal user, HospitalDashboardContext db, EventPublisher events) =>
        {
            if (!await db.Patients.AnyAsync(p => p.Id == admission.PatientId))
                return Results.BadRequest($"Patient {admission.PatientId} does not exist.");

            var bed = await db.Beds.FindAsync(admission.BedId);
            if (bed is null) return Results.BadRequest($"Bed {admission.BedId} does not exist.");
            if (bed.Status != "Available") return Results.BadRequest($"Bed is {bed.Status}, not Available.");

            var staffId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var admissionId = Guid.NewGuid();
            var now = DateTime.UtcNow;

            await events.PublishAsync("bed-events", admissionId.ToString(), new AdmissionCreated(
                Guid.NewGuid(), "AdmissionCreated", admissionId, admission.PatientId,
                admission.BedId, staffId, admission.AdmittedAt == default ? now : admission.AdmittedAt, now));

            return Results.Accepted($"/api/admissions/{admissionId}", new { admissionId });
        });

        group.MapPut("/{id}", async (Guid id, Admission input, HospitalDashboardContext db, IHubContext<AdmissionsHub> hub) =>
        {
            var admission = await db.Admissions.FindAsync(id);
            if (admission is null) return Results.NotFound();

            admission.Status = input.Status;
            admission.DischargedAt = input.DischargedAt;
            admission.StaffId = input.StaffId;

            if (input.Status == "Discharged")
            {
                var bed = await db.Beds.FindAsync(admission.BedId);
                if (bed is not null) bed.Status = "Available";
            }

            await db.SaveChangesAsync();
            await hub.Clients.All.SendAsync("DataChanged", "admissions");
            return Results.NoContent();
        });

        group.MapDelete("/{id}", async (Guid id, HospitalDashboardContext db, IHubContext<AdmissionsHub> hub) =>
        {
            var admission = await db.Admissions.FindAsync(id);
            if (admission is null) return Results.NotFound();

            db.Admissions.Remove(admission);
            await db.SaveChangesAsync();
            await hub.Clients.All.SendAsync("DataChanged", "admissions");
            return Results.NoContent();
        }).RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" });
    }
}