using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BackgroundJobs.Data.Migrations.Application
{
    public partial class Initial_Migration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Jobs",
                columns: table => new
                {
                    JobId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JobName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    JobType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    JobStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    JobQueued = table.Column<DateTime>(type: "datetime2", nullable: false),
                    JobStarted = table.Column<DateTime>(type: "datetime2", nullable: true),
                    JobEnded = table.Column<DateTime>(type: "datetime2", nullable: true),
                    JobProgress = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    JobResult = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StartedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Jobs", x => x.JobId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Jobs");
        }
    }
}
