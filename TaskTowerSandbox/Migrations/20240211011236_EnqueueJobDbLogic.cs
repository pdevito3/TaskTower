﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskTowerSandbox.Migrations
{
    /// <inheritdoc />
    public partial class EnqueueJobDbLogic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
//             migrationBuilder.Sql($@"CREATE OR REPLACE FUNCTION enqueue_job()
// RETURNS TRIGGER AS $$
// BEGIN
//     INSERT INTO enqueued_jobs(id, job_id, queue)
//     VALUES (gen_random_uuid(), NEW.id, NEW.queue);
//     RETURN NEW;
// END;
// $$ LANGUAGE plpgsql;
//
//
// CREATE TRIGGER trigger_enqueue_job
//     AFTER INSERT ON jobs
//     FOR EACH ROW
//     WHEN (timezone('utc', NEW.run_after) <= timezone('utc', NEW.created_at)) 
//     EXECUTE FUNCTION enqueue_job();
// ");


            migrationBuilder.Sql($@"CREATE OR REPLACE FUNCTION enqueue_job()
RETURNS TRIGGER AS $$
BEGIN
    -- Insert into enqueued_jobs
    INSERT INTO enqueued_jobs(id, job_id, queue)
    VALUES (gen_random_uuid(), NEW.id, NEW.queue);
    
    -- Update status in jobs table
    UPDATE jobs
    SET status = 'Enqueued'
    WHERE id = NEW.id;
    
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
            migrationBuilder.Sql("DROP TRIGGER trigger_enqueue_job ON jobs;");
            migrationBuilder.Sql("DROP FUNCTION enqueue_job();");

        }
    }
}
