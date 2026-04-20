using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace OnlineSurvey.Infrastructure.Data.Migrations;

public partial class AddSurveyAccessControl : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "AccessMode",
            table: "Surveys",
            type: "integer",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AddColumn<int>(
            name: "CollectedFields",
            table: "Surveys",
            type: "integer",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AddColumn<bool>(
            name: "IsPublic",
            table: "Surveys",
            type: "boolean",
            nullable: false,
            defaultValue: true);

        migrationBuilder.CreateTable(
            name: "SurveyAccessCodes",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                SurveyId = table.Column<Guid>(type: "uuid", nullable: false),
                Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                CodeHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                UsedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_SurveyAccessCodes", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_SurveyAccessCodes_SurveyId_Email",
            table: "SurveyAccessCodes",
            columns: new[] { "SurveyId", "Email" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "SurveyAccessCodes");
        migrationBuilder.DropColumn(name: "AccessMode", table: "Surveys");
        migrationBuilder.DropColumn(name: "CollectedFields", table: "Surveys");
        migrationBuilder.DropColumn(name: "IsPublic", table: "Surveys");
    }
}
