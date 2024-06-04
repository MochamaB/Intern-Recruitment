using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Workflows.Migrations
{
    /// <inheritdoc />
    public partial class ChangeDocumentField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DocumentType",
                table: "Document",
                newName: "DocumentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Document_DocumentTypeId",
                table: "Document",
                column: "DocumentTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Document_DocumentType_DocumentTypeId",
                table: "Document",
                column: "DocumentTypeId",
                principalTable: "DocumentType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Document_DocumentType_DocumentTypeId",
                table: "Document");

            migrationBuilder.DropIndex(
                name: "IX_Document_DocumentTypeId",
                table: "Document");

            migrationBuilder.RenameColumn(
                name: "DocumentTypeId",
                table: "Document",
                newName: "DocumentType");
        }
    }
}
