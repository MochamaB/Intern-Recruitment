using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Workflows.Migrations
{
    /// <inheritdoc />
    public partial class AddFilePathToDocument : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                table: "Document",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FilePath",
                table: "Document");
        }
    }
}
