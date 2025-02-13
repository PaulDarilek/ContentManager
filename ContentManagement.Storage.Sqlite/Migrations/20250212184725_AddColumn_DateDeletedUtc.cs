using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContentManagement.Storage.Sqlite.Migrations
{
    /// <inheritdoc />
    public partial class AddColumn_DateDeletedUtc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Files",
                newName: "FileName");

            migrationBuilder.RenameIndex(
                name: "IX_Files_Name",
                table: "Files",
                newName: "IX_Files_FileName");

            migrationBuilder.RenameIndex(
                name: "IX_Files_DirectoryPath_Name_DriveId",
                table: "Files",
                newName: "IX_Files_DirectoryPath_FileName_DriveId");

            migrationBuilder.AddColumn<DateTime>(
                name: "DateDeletedUtc",
                table: "Files",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Files_DriveId",
                table: "Files",
                column: "DriveId");

            migrationBuilder.AddForeignKey(
                name: "FK_Files_Drives_DriveId",
                table: "Files",
                column: "DriveId",
                principalTable: "Drives",
                principalColumn: "DriveId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Files_Drives_DriveId",
                table: "Files");

            migrationBuilder.DropIndex(
                name: "IX_Files_DriveId",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "DateDeletedUtc",
                table: "Files");

            migrationBuilder.RenameColumn(
                name: "FileName",
                table: "Files",
                newName: "Name");

            migrationBuilder.RenameIndex(
                name: "IX_Files_FileName",
                table: "Files",
                newName: "IX_Files_Name");

            migrationBuilder.RenameIndex(
                name: "IX_Files_DirectoryPath_FileName_DriveId",
                table: "Files",
                newName: "IX_Files_DirectoryPath_Name_DriveId");
        }
    }
}
