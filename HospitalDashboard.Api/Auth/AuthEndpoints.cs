using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using HospitalDashboard.Api.Models;

namespace HospitalDashboard.Api.Auth;

public record LoginRequest(string Username, string Password);
public record LoginResponse(string Token, string Name, string Role);

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth");

        group.MapPost("/login", async (LoginRequest request, HospitalDashboardContext db, IConfiguration config) =>
        {
            var staff = await db.Staff.FirstOrDefaultAsync(s => s.Username == request.Username);
            if (staff is null || !BCrypt.Net.BCrypt.Verify(request.Password, staff.PasswordHash))
                return Results.Unauthorized();

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, staff.Id.ToString()),
                new Claim(ClaimTypes.Name, staff.Name),
                new Claim(ClaimTypes.Role, staff.Role),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: config["Jwt:Issuer"],
                audience: config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: creds
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            return Results.Ok(new LoginResponse(tokenString, staff.Name, staff.Role));
        });
    }
}
