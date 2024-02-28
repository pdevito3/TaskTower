using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskTower.Migrations
{
    /// <inheritdoc />
    public partial class InitialConsolidation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "jobs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    queue = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false),
                    type = table.Column<string>(type: "text", nullable: false),
                    method = table.Column<string>(type: "text", nullable: false),
                    parameter_types = table.Column<string[]>(type: "text[]", nullable: false),
                    payload = table.Column<string>(type: "jsonb", nullable: false),
                    retries = table.Column<int>(type: "integer", nullable: false),
                    max_retries = table.Column<int>(type: "integer", nullable: true),
                    run_after = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ran_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    deadline = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    context_parameters = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_jobs", x => x.id);
                });

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
                    table.ForeignKey(
                        name: "fk_run_histories_jobs_job_id",
                        column: x => x.job_id,
                        principalTable: "jobs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tags",
                columns: table => new
                {
                    job_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tags", x => new { x.job_id, x.name });
                    table.ForeignKey(
                        name: "fk_tags_jobs_job_id",
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

            migrationBuilder.CreateIndex(
                name: "ix_tags_name",
                table: "tags",
                column: "name");
            
            migrationBuilder.Sql($@"CREATE OR REPLACE FUNCTION notify_job_available()
RETURNS trigger AS $$
BEGIN
    PERFORM pg_notify('job_available', 'Queue: ' || NEW.queue || ', ID: ' || NEW.id::text);
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;");
            
            migrationBuilder.Sql($@"CREATE OR REPLACE FUNCTION enqueue_job()
RETURNS TRIGGER AS $$
BEGIN
    -- Insert into enqueued_jobs
    INSERT INTO enqueued_jobs(id, job_id, queue)
    VALUES (gen_random_uuid(), NEW.id, NEW.queue);
    
    -- Update status in jobs table
    UPDATE jobs
    SET status = 'Processing'
    WHERE id = NEW.id;

    -- add a job history record for enqueuing
    INSERT INTO run_histories(id, job_id, status, occurred_at)
    VALUES (gen_random_uuid(), NEW.id, 'Enqueued', NOW());
    
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trigger_enqueue_job
    AFTER INSERT ON jobs
    FOR EACH ROW
    WHEN (timezone('utc', NEW.run_after) <= timezone('utc', NEW.created_at)) 
    EXECUTE FUNCTION enqueue_job();
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "enqueued_jobs");

            migrationBuilder.DropTable(
                name: "run_histories");

            migrationBuilder.DropTable(
                name: "tags");

            migrationBuilder.DropTable(
                name: "jobs");
            
            migrationBuilder.Sql("DROP FUNCTION notify_job_available();");
            migrationBuilder.Sql("DROP TRIGGER trigger_enqueue_job ON jobs;");
            migrationBuilder.Sql("DROP FUNCTION enqueue_job();");

        }
    }
}
