using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Workflows.Migrations
{
    /// <inheritdoc />
    public partial class AddDeptDocumentModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "departmentCode",
                table: "Document",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "departmentCode",
                table: "Document");
        }
    }
}
