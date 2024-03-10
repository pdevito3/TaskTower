namespace TaskTower.Database.Migrations;

using System.Data;
using FluentMigrator;

[Migration(20240227)]
public class Initial : Migration
{
    public override void Up()
    {
        var schemaName = MigrationConfig.SchemaName;
        Execute.Sql($"CREATE SCHEMA IF NOT EXISTS {schemaName}");
        
        Create.Table("jobs")
            .InSchema(schemaName)
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("queue").AsString().Nullable()
            .WithColumn("status").AsString().NotNullable()
            .WithColumn("type").AsString().NotNullable()
            .WithColumn("method").AsString().NotNullable()
            .WithColumn("job_name").AsString().Nullable()
            .WithColumn("parameter_types").AsCustom("text[]").NotNullable()
            .WithColumn("payload").AsCustom("jsonb").NotNullable()
            .WithColumn("retries").AsInt32().NotNullable()
            .WithColumn("max_retries").AsInt32().Nullable()
            .WithColumn("run_after").AsDateTimeOffset().NotNullable()
            .WithColumn("ran_at").AsDateTimeOffset().Nullable()
            .WithColumn("created_at").AsDateTimeOffset().NotNullable()
            .WithColumn("deadline").AsDateTimeOffset().Nullable()
            .WithColumn("context_parameters").AsCustom("jsonb").Nullable();  
        
        Create.Table("run_histories")
            .InSchema(schemaName)
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("job_id").AsGuid().ForeignKey("FK_run_histories_job_id", schemaName, "jobs", "id").OnDelete(Rule.None)
            .WithColumn("status").AsString().NotNullable()
            .WithColumn("comment").AsString().Nullable()
            .WithColumn("details").AsString().Nullable()
            .WithColumn("occurred_at").AsDateTimeOffset().NotNullable();

        Create.Table("tags")
            .InSchema(schemaName)
            .WithColumn("job_id").AsGuid().PrimaryKey().ForeignKey("FK_tags_job_id", schemaName, "jobs", "id").OnDelete(Rule.None)
            .WithColumn("name").AsString().PrimaryKey();

        Create.Index("ix_jobs_run_after").OnTable("jobs").InSchema(schemaName).OnColumn("run_after");
        Create.Index("ix_jobs_status").OnTable("jobs").InSchema(schemaName).OnColumn("status");
        Create.Index("ix_run_histories_job_id").OnTable("run_histories").InSchema(schemaName).OnColumn("job_id");
        Create.Index("ix_run_histories_status").OnTable("run_histories").InSchema(schemaName).OnColumn("status");
        Create.Index("ix_tags_name").OnTable("tags").InSchema(schemaName).OnColumn("name");

        Execute.Sql($@"CREATE OR REPLACE FUNCTION {schemaName}.notify_job_available()
RETURNS trigger AS $$
BEGIN
    PERFORM pg_notify('job_available', 'Queue: ' || NEW.queue || ', ID: ' || NEW.id::text);
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;");
        
        Execute.Sql($@"CREATE OR REPLACE FUNCTION {schemaName}.notify_job_set_for_processing()
RETURNS trigger AS $$
BEGIN
    PERFORM pg_notify('job_set_for_processing', NEW.id::text);
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;");

        Execute.Sql($@"CREATE OR REPLACE FUNCTION {schemaName}.enqueue_job()
RETURNS TRIGGER AS $$
BEGIN    
    -- Update status in jobs table
    UPDATE {schemaName}.jobs
    SET status = 'Enqueued'
    WHERE id = NEW.id;

    -- Add a job history records
    INSERT INTO {schemaName}.run_histories(id, job_id, status, occurred_at)
    VALUES 
        (gen_random_uuid(), NEW.id, 'Pending', NOW()),
        (gen_random_uuid(), NEW.id, 'Enqueued', NOW());
    
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trigger_enqueue_job
    AFTER INSERT ON {schemaName}.jobs
    FOR EACH ROW
    WHEN (timezone('utc', NEW.run_after) <= timezone('utc', NEW.created_at)) 
    EXECUTE FUNCTION {schemaName}.enqueue_job();");

        Execute.Sql($@"CREATE OR REPLACE FUNCTION {schemaName}.add_scheduled_job_pending_history()
RETURNS TRIGGER AS $$
BEGIN
    -- Add a job history record for pending
    INSERT INTO {schemaName}.run_histories(id, job_id, status, occurred_at)
    VALUES (gen_random_uuid(), NEW.id, 'Pending', NOW());
    
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trigger_add_scheduled_job_pending_history
    AFTER INSERT ON {schemaName}.jobs
    FOR EACH ROW
    WHEN (timezone('utc', NEW.run_after) > timezone('utc', NEW.created_at)) 
    EXECUTE FUNCTION {schemaName}.add_scheduled_job_pending_history();");
    }

    public override void Down()
    {
        var schemaName = MigrationConfig.SchemaName;
        Delete.Table("jobs").InSchema(schemaName);
        Delete.Table("run_histories").InSchema(schemaName);
        Delete.Table("tags").InSchema(schemaName);

        Execute.Sql($@"DROP FUNCTION IF EXISTS {schemaName}.notify_job_available();");
        Execute.Sql($@"DROP FUNCTION IF EXISTS {schemaName}.enqueue_job();");
        Execute.Sql($@"DROP FUNCTION IF EXISTS {schemaName}.add_scheduled_job_pending_history();");
        
        Execute.Sql($@"DROP TRIGGER IF EXISTS trigger_enqueue_job ON {schemaName}.jobs;");
        Execute.Sql($@"DROP TRIGGER IF EXISTS trigger_add_scheduled_job_pending_history ON {schemaName}.jobs;");
        
        Delete.Schema(schemaName);
    }
}