using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskTowerSandbox.Migrations
{
    /// <inheritdoc />
    public partial class AddEnqueuedJobs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "enqueued_jobs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    queue = table.Column<string>(type: "text", nullable: false),
                    job_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_enqueued_jobs", x => x.id);
                    table.ForeignKey(
                        name: "fk_enqueued_jobs_jobs_job_id",
                        column: x => x.job_id,
                        principalTable: "jobs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_enqueued_jobs_job_id",
                table: "enqueued_jobs",
                column: "job_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "enqueued_jobs");
        }
    }
}
