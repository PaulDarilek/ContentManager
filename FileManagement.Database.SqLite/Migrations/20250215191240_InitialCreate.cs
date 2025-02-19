using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FileManagement.Database.SqLite.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Drives",
                columns: table => new
                {
                    DriveId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DriveType = table.Column<string>(type: "TEXT", nullable: false),
                    DriveLetter = table.Column<string>(type: "TEXT", nullable: false),
                    MachineNames = table.Column<string>(type: "TEXT", nullable: true),
                    TotalSize = table.Column<long>(type: "INTEGER", nullable: false),
                    TotalFreeSpace = table.Column<long>(type: "INTEGER", nullable: true),
                    DriveFormat = table.Column<string>(type: "TEXT", nullable: true),
                    VolumeSerialNumber = table.Column<string>(type: "TEXT", nullable: true),
                    VolumeLabel = table.Column<string>(type: "TEXT", nullable: true),
                    DateCreatedUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DateUpdatedUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    HardwareSerialNumber = table.Column<string>(type: "TEXT", nullable: true),
                    Model = table.Column<string>(type: "TEXT", nullable: true),
                    Tags = table.Column<string>(type: "TEXT", nullable: true),
                    Properties = table.Column<string>(type: "TEXT", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Drives", x => x.DriveId);
                });

            migrationBuilder.CreateTable(
                name: "Directories",
                columns: table => new
                {
                    DirectoryId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DriveId = table.Column<int>(type: "INTEGER", nullable: false),
                    DriveLetter = table.Column<string>(type: "TEXT", nullable: true),
                    DirectoryPath = table.Column<string>(type: "TEXT", nullable: true),
                    Exists = table.Column<bool>(type: "INTEGER", nullable: true),
                    ShouldMonitorFiles = table.Column<bool>(type: "INTEGER", nullable: true),
                    IsBackupLocation = table.Column<bool>(type: "INTEGER", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    Tags = table.Column<string>(type: "TEXT", nullable: true),
                    Properties = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Directories", x => x.DirectoryId);
                    table.ForeignKey(
                        name: "FK_Directories_Drives_DriveId",
                        column: x => x.DriveId,
                        principalTable: "Drives",
                        principalColumn: "DriveId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Files",
                columns: table => new
                {
                    FileId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DriveId = table.Column<int>(type: "INTEGER", nullable: true),
                    DriveLetter = table.Column<string>(type: "TEXT", nullable: true),
                    DirectoryPath = table.Column<string>(type: "TEXT", nullable: true),
                    FileName = table.Column<string>(type: "TEXT", nullable: true),
                    Attributes = table.Column<int>(type: "INTEGER", nullable: false),
                    Exists = table.Column<bool>(type: "INTEGER", nullable: true),
                    Length = table.Column<long>(type: "INTEGER", nullable: true),
                    DateCreatedUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DateUpdatedUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DateDeletedUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsReadOnly = table.Column<bool>(type: "INTEGER", nullable: false),
                    DateRetention = table.Column<DateTime>(type: "TEXT", nullable: true),
                    HashCode = table.Column<string>(type: "TEXT", nullable: true),
                    Crc32 = table.Column<uint>(type: "INTEGER", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    Tags = table.Column<string>(type: "TEXT", nullable: true),
                    Properties = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Files", x => x.FileId);
                    table.ForeignKey(
                        name: "FK_Files_Drives_DriveId",
                        column: x => x.DriveId,
                        principalTable: "Drives",
                        principalColumn: "DriveId");
                });

            migrationBuilder.CreateTable(
                name: "FileDuplicates",
                columns: table => new
                {
                    FirstId = table.Column<int>(type: "INTEGER", nullable: false),
                    SecondId = table.Column<int>(type: "INTEGER", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    Tags = table.Column<string>(type: "TEXT", nullable: true),
                    Properties = table.Column<string>(type: "TEXT", nullable: true),
                    AreDuplicates = table.Column<bool>(type: "INTEGER", nullable: true),
                    FirstIsABackup = table.Column<bool>(type: "INTEGER", nullable: true),
                    SecondIsABackup = table.Column<bool>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileDuplicates", x => new { x.FirstId, x.SecondId });
                    table.CheckConstraint("Ck_FileDuplicates_IdOrder", "FirstId < SecondId");
                    table.ForeignKey(
                        name: "FK_FileDuplicates_Files_FirstId",
                        column: x => x.FirstId,
                        principalTable: "Files",
                        principalColumn: "FileId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FileDuplicates_Files_SecondId",
                        column: x => x.SecondId,
                        principalTable: "Files",
                        principalColumn: "FileId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Directories_DriveId",
                table: "Directories",
                column: "DriveId");

            migrationBuilder.CreateIndex(
                name: "IX_Drives_VolumeSerialNumber",
                table: "Drives",
                column: "VolumeSerialNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FileDuplicates_SecondId",
                table: "FileDuplicates",
                column: "SecondId");

            migrationBuilder.CreateIndex(
                name: "IX_Files_Crc32",
                table: "Files",
                column: "Crc32");

            migrationBuilder.CreateIndex(
                name: "IX_Files_DirectoryPath_FileName_DriveId",
                table: "Files",
                columns: new[] { "DirectoryPath", "FileName", "DriveId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Files_DriveId",
                table: "Files",
                column: "DriveId");

            migrationBuilder.CreateIndex(
                name: "IX_Files_FileName",
                table: "Files",
                column: "FileName");

            migrationBuilder.CreateIndex(
                name: "IX_Files_HashCode",
                table: "Files",
                column: "HashCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Directories");

            migrationBuilder.DropTable(
                name: "FileDuplicates");

            migrationBuilder.DropTable(
                name: "Files");

            migrationBuilder.DropTable(
                name: "Drives");
        }
    }
}
