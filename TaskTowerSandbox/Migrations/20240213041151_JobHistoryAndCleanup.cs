using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskTowerSandbox.Migrations
{
    /// <inheritdoc />
    public partial class JobHistoryAndCleanup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "error",
                table: "jobs");

            migrationBuilder.AlterColumn<string>(
                name: "queue",
                table: "jobs",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateTable(
                name: "run_histories",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    job_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    comment = table.Column<string>(type: "text", nullable: true),
                    details = table.Column<string>(type: "text", nullable: true),
                    occurred_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_run_histories", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_jobs_run_after",
                table: "jobs",
                column: "run_after");

            migrationBuilder.CreateIndex(
                name: "ix_jobs_status",
                table: "jobs",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_run_histories_job_id",
                table: "run_histories",
                column: "job_id");

            migrationBuilder.CreateIndex(
                name: "ix_run_histories_status",
                table: "run_histories",
                column: "status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "run_histories");

            migrationBuilder.DropIndex(
                name: "ix_jobs_run_after",
                table: "jobs");

            migrationBuilder.DropIndex(
                name: "ix_jobs_status",
                table: "jobs");

            migrationBuilder.AlterColumn<string>(
                name: "queue",
                table: "jobs",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "error",
                table: "jobs",
                type: "text",
                nullable: true);
        }
    }
}
