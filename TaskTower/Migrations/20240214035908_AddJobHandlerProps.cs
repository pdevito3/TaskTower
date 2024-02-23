using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskTowerSandbox.Migrations
{
    /// <inheritdoc />
    public partial class AddJobHandlerProps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "method",
                table: "jobs",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string[]>(
                name: "parameter_types",
                table: "jobs",
                type: "text[]",
                nullable: false,
                defaultValue: new string[0]);

            migrationBuilder.AddColumn<string>(
                name: "type",
                table: "jobs",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "method",
                table: "jobs");

            migrationBuilder.DropColumn(
                name: "parameter_types",
                table: "jobs");

            migrationBuilder.DropColumn(
                name: "type",
                table: "jobs");
        }
    }
}
