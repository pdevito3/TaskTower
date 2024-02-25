using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskTower.Migrations
{
    /// <inheritdoc />
    public partial class AddContextParams : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "context_parameters",
                table: "jobs",
                type: "jsonb",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "context_parameters",
                table: "jobs");
        }
    }
}
