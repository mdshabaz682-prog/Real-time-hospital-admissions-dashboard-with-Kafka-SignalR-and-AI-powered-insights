using System.Security.Claims;
using HospitalDashboard.Api.Messaging;using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using HospitalDashboard.Api.Models;

namespace HospitalDashboard.Api.Endpoints;

public static class BedEndpoints
{
    public static void MapBedEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/beds").WithTags("Beds").RequireAuthorization();

        group.MapGet("/", async (HospitalDashboardContext db) =>
            await db.Beds.ToListAsync());

        group.MapGet("/{id}", async (Guid id, HospitalDashboardContext db) =>
            await db.Beds.FindAsync(id) is Bed b ? Results.Ok(b) : Results.NotFound());

        group.MapPost("/", async (Bed bed, HospitalDashboardContext db) =>
        {
            db.Beds.Add(bed);
            await db.SaveChangesAsync();
            return Results.Created($"/api/beds/{bed.Id}", bed);
        });
        group.MapPost("/{id}/clean", async (Guid id, ClaimsPrincipal user, HospitalDashboardContext db, EventPublisher events) =>
        {
            var bed = await db.Beds.FindAsync(id);
            if (bed is null) return Results.NotFound();
            if (bed.Status != "Cleaning") return Results.BadRequest($"Bed is {bed.Status}, not Cleaning.");

            var staffId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await events.PublishAsync("bed-events", id.ToString(),
                new BedCleaned(Guid.NewGuid(), "BedCleaned", id, staffId, DateTime.UtcNow));

            return Results.Accepted($"/api/beds/{id}", new { bedId = id });
        });
        group.MapPut("/{id}", async (Guid id, Bed input, HospitalDashboardContext db) =>
        {
            var bed = await db.Beds.FindAsync(id);
            if (bed is null) return Results.NotFound();

            bed.Ward = input.Ward;
            bed.BedNumber = input.BedNumber;
            bed.Status = input.Status;

            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        group.MapDelete("/{id}", async (Guid id, HospitalDashboardContext db) =>
        {
            var bed = await db.Beds.FindAsync(id);
            if (bed is null) return Results.NotFound();

            db.Beds.Remove(bed);
            await db.SaveChangesAsync();
            return Results.NoContent();
        }).RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" });
    }
}
