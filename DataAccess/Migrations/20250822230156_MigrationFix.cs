using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class MigrationFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UploadedFiles_LocalFolders_LocalFolderId1",
                table: "UploadedFiles");

            migrationBuilder.DropIndex(
                name: "IX_UploadedFiles_FileName",
                table: "UploadedFiles");

            migrationBuilder.DropIndex(
                name: "IX_UploadedFiles_LocalFolderId1",
                table: "UploadedFiles");

            migrationBuilder.DropColumn(
                name: "LocalFolderId1",
                table: "UploadedFiles");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LocalFolderId1",
                table: "UploadedFiles",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UploadedFiles_FileName",
                table: "UploadedFiles",
                column: "FileName");

            migrationBuilder.CreateIndex(
                name: "IX_UploadedFiles_LocalFolderId1",
                table: "UploadedFiles",
                column: "LocalFolderId1");

            migrationBuilder.AddForeignKey(
                name: "FK_UploadedFiles_LocalFolders_LocalFolderId1",
                table: "UploadedFiles",
                column: "LocalFolderId1",
                principalTable: "LocalFolders",
                principalColumn: "LocalFolderId");
        }
    }
}
