using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContentManagement.Storage.Sqlite.Migrations
{
    /// <inheritdoc />
    public partial class AddColumn_NotesTagsProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Properties",
                table: "FileDuplicates",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Tags",
                table: "FileDuplicates",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Properties",
                table: "Drives",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Directories",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Properties",
                table: "Directories",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Tags",
                table: "Directories",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Properties",
                table: "FileDuplicates");

            migrationBuilder.DropColumn(
                name: "Tags",
                table: "FileDuplicates");

            migrationBuilder.DropColumn(
                name: "Properties",
                table: "Drives");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Directories");

            migrationBuilder.DropColumn(
                name: "Properties",
                table: "Directories");

            migrationBuilder.DropColumn(
                name: "Tags",
                table: "Directories");
        }
    }
}
