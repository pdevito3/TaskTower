﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using TaskTowerSandbox.Database;

#nullable disable

namespace TaskTowerSandbox.Migrations
{
    [DbContext(typeof(TaskTowerDbContext))]
    [Migration("20240205050811_AddJobListener")]
    partial class AddJobListener
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("TaskTowerSandbox.Domain.TaskTowerJob.TaskTowerJob", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_at");

                    b.Property<DateTimeOffset?>("Deadline")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("deadline");

                    b.Property<string>("Error")
                        .HasColumnType("text")
                        .HasColumnName("error");

                    b.Property<string>("Fingerprint")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("fingerprint");

                    b.Property<int?>("MaxRetries")
                        .HasColumnType("integer")
                        .HasColumnName("max_retries");

                    b.Property<string>("Payload")
                        .IsRequired()
                        .HasColumnType("jsonb")
                        .HasColumnName("payload");

                    b.Property<string>("Queue")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("queue");

                    b.Property<DateTimeOffset?>("RanAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("ran_at");

                    b.Property<int>("Retries")
                        .HasColumnType("integer")
                        .HasColumnName("retries");

                    b.Property<DateTimeOffset>("RunAfter")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("run_after");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("status");

                    b.HasKey("Id")
                        .HasName("pk_jobs");

                    b.ToTable("jobs", (string)null);
                });
#pragma warning restore 612, 618
        }
    }
}
