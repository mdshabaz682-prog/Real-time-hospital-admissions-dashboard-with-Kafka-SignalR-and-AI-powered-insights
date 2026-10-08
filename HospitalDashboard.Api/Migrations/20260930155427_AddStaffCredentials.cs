using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HospitalDashboard.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddStaffCredentials : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PasswordHash",
                table: "Staff",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Username",
                table: "Staff",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PasswordHash",
                table: "Staff");

            migrationBuilder.DropColumn(
                name: "Username",
                table: "Staff");
        }
    }
}
