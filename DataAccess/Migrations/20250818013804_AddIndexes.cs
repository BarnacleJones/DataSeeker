using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_UploadedFiles_FileName",
                table: "UploadedFiles",
                column: "FileName");

            migrationBuilder.CreateIndex(
                name: "IX_UploadedFiles_FolderId",
                table: "UploadedFiles",
                column: "FolderId");

            migrationBuilder.CreateIndex(
                name: "IX_LogLines_FileType",
                table: "LogLines",
                column: "FileType");

            migrationBuilder.CreateIndex(
                name: "IX_LogFiles_TransferDirection",
                table: "LogFiles",
                column: "TransferDirection");

            migrationBuilder.CreateIndex(
                name: "IX_LocalFolders_Name",
                table: "LocalFolders",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_LocalFolders_ParentFolderId_Name",
                table: "LocalFolders",
                columns: new[] { "ParentFolderId", "Name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UploadedFiles_FileName",
                table: "UploadedFiles");

            migrationBuilder.DropIndex(
                name: "IX_UploadedFiles_FolderId",
                table: "UploadedFiles");

            migrationBuilder.DropIndex(
                name: "IX_LogLines_FileType",
                table: "LogLines");

            migrationBuilder.DropIndex(
                name: "IX_LogFiles_TransferDirection",
                table: "LogFiles");

            migrationBuilder.DropIndex(
                name: "IX_LocalFolders_Name",
                table: "LocalFolders");

            migrationBuilder.DropIndex(
                name: "IX_LocalFolders_ParentFolderId_Name",
                table: "LocalFolders");
        }
    }
}
