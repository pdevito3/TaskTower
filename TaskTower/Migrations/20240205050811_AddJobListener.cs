using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskTowerSandbox.Migrations
{
    /// <inheritdoc />
    public partial class AddJobListener : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //     WHEN (timezone('utc', NEW.run_after) <= timezone('utc', NEW.created_at))
            // or
            //    WHEN (NEW.run_after <= timezone('utc', NEW.created_at))
//             migrationBuilder.Sql($@"CREATE OR REPLACE FUNCTION notify_job_available()
// RETURNS trigger AS $$
// BEGIN
//     PERFORM pg_notify('job_available', 'Queue: ' || NEW.queue || ', ID: ' || NEW.id::text);
//     RETURN NEW;
// END;
// $$ LANGUAGE plpgsql;
//
// CREATE TRIGGER job_available_trigger
//     AFTER INSERT ON jobs FOR EACH ROW
//     WHEN (NEW.run_after <= NEW.created_at)
//     EXECUTE FUNCTION notify_job_available();");

            migrationBuilder.Sql($@"CREATE OR REPLACE FUNCTION notify_job_available()
RETURNS trigger AS $$
BEGIN
    PERFORM pg_notify('job_available', 'Queue: ' || NEW.queue || ', ID: ' || NEW.id::text);
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // migrationBuilder.Sql("DROP TRIGGER job_available_trigger ON jobs;");
            migrationBuilder.Sql("DROP FUNCTION notify_job_available();");
        }
    }
}
