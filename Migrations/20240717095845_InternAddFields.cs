using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Workflows.Migrations
{
    /// <inheritdoc />
    public partial class InternAddFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Certification",
                table: "Intern",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Course",
                table: "Intern",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "School",
                table: "Intern",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Certification",
                table: "Intern");

            migrationBuilder.DropColumn(
                name: "Course",
                table: "Intern");

            migrationBuilder.DropColumn(
                name: "School",
                table: "Intern");
        }
    }
}
