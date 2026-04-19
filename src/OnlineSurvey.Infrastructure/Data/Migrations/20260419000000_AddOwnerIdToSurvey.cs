using Microsoft.EntityFrameworkCore.Migrations;

namespace OnlineSurvey.Infrastructure.Data.Migrations;

public partial class AddOwnerIdToSurvey : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "OwnerId",
            table: "Surveys",
            type: "character varying(128)",
            maxLength: 128,
            nullable: false,
            defaultValue: "anonymous");

        migrationBuilder.CreateIndex(
            name: "IX_Surveys_OwnerId",
            table: "Surveys",
            column: "OwnerId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(name: "IX_Surveys_OwnerId", table: "Surveys");
        migrationBuilder.DropColumn(name: "OwnerId", table: "Surveys");
    }
}
