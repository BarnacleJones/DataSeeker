using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LocalFolders",
                columns: table => new
                {
                    LocalFolderId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    ParentFolderId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocalFolders", x => x.LocalFolderId);
                    table.ForeignKey(
                        name: "FK_LocalFolders_LocalFolders_ParentFolderId",
                        column: x => x.ParentFolderId,
                        principalTable: "LocalFolders",
                        principalColumn: "LocalFolderId");
                });

            migrationBuilder.CreateTable(
                name: "LogFiles",
                columns: table => new
                {
                    LogFileId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LogFileName = table.Column<string>(type: "text", nullable: true),
                    TransferDirection = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogFiles", x => x.LogFileId);
                });

            migrationBuilder.CreateTable(
                name: "UploadedFiles",
                columns: table => new
                {
                    UploadedFileId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FileName = table.Column<string>(type: "text", nullable: false),
                    ContainingFolderId = table.Column<int>(type: "integer", nullable: true),
                    LocalFolderId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UploadedFiles", x => x.UploadedFileId);
                    table.ForeignKey(
                        name: "FK_UploadedFiles_LocalFolders_ContainingFolderId",
                        column: x => x.ContainingFolderId,
                        principalTable: "LocalFolders",
                        principalColumn: "LocalFolderId");
                    table.ForeignKey(
                        name: "FK_UploadedFiles_LocalFolders_LocalFolderId",
                        column: x => x.LocalFolderId,
                        principalTable: "LocalFolders",
                        principalColumn: "LocalFolderId");
                });

            migrationBuilder.CreateTable(
                name: "LogLines",
                columns: table => new
                {
                    LogLineId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    User = table.Column<string>(type: "text", nullable: true),
                    FullFilePath = table.Column<string>(type: "text", nullable: false),
                    FileType = table.Column<string>(type: "text", nullable: false),
                    LogFileId = table.Column<int>(type: "integer", nullable: false),
                    UploadedFileId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogLines", x => x.LogLineId);
                    table.ForeignKey(
                        name: "FK_LogLines_LogFiles_LogFileId",
                        column: x => x.LogFileId,
                        principalTable: "LogFiles",
                        principalColumn: "LogFileId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LogLines_UploadedFiles_UploadedFileId",
                        column: x => x.UploadedFileId,
                        principalTable: "UploadedFiles",
                        principalColumn: "UploadedFileId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_LocalFolders_Name",
                table: "LocalFolders",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_LocalFolders_ParentFolderId",
                table: "LocalFolders",
                column: "ParentFolderId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalFolders_ParentFolderId_Name",
                table: "LocalFolders",
                columns: new[] { "ParentFolderId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LogFiles_TransferDirection",
                table: "LogFiles",
                column: "TransferDirection");

            migrationBuilder.CreateIndex(
                name: "IX_LogLines_FileType",
                table: "LogLines",
                column: "FileType");

            migrationBuilder.CreateIndex(
                name: "IX_LogLines_LogFileId",
                table: "LogLines",
                column: "LogFileId");

            migrationBuilder.CreateIndex(
                name: "IX_LogLines_UploadedFileId",
                table: "LogLines",
                column: "UploadedFileId");

            migrationBuilder.CreateIndex(
                name: "IX_UploadedFiles_ContainingFolderId",
                table: "UploadedFiles",
                column: "ContainingFolderId");

            migrationBuilder.CreateIndex(
                name: "IX_UploadedFiles_FileName",
                table: "UploadedFiles",
                column: "FileName");

            migrationBuilder.CreateIndex(
                name: "IX_UploadedFiles_LocalFolderId",
                table: "UploadedFiles",
                column: "LocalFolderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LogLines");

            migrationBuilder.DropTable(
                name: "LogFiles");

            migrationBuilder.DropTable(
                name: "UploadedFiles");

            migrationBuilder.DropTable(
                name: "LocalFolders");
        }
    }
}
