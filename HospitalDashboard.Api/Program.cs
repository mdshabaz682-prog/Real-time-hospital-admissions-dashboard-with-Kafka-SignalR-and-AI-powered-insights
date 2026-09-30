using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using HospitalDashboard.Api;
using HospitalDashboard.Api.Endpoints;
using HospitalDashboard.Api.Seed;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<HospitalDashboardContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<HospitalDashboardContext>();
    DataSeeder.SeedData(db);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/", () => "Hospital Dashboard API is running.");
app.MapPatientEndpoints();
app.MapBedEndpoints();
app.MapAdmissionEndpoints();

app.Run();
