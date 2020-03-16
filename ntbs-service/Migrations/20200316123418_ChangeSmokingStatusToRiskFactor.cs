﻿using Microsoft.EntityFrameworkCore.Migrations;

namespace ntbs_service.Migrations
{
    public partial class ChangeSmokingStatusToRiskFactor : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SmokingStatus",
                table: "SocialRiskFactors");
            
            migrationBuilder.CreateTable(
                name: "RiskFactorSmoking",
                columns: table => new
                {
                    SocialRiskFactorsNotificationId = table.Column<int>(nullable: false),
                    Type = table.Column<string>(maxLength: 30, nullable: false, defaultValue: "Smoking"),
                    Status = table.Column<string>(maxLength: 30, nullable: true),
                    IsCurrent = table.Column<bool>(nullable: true),
                    InPastFiveYears = table.Column<bool>(nullable: true),
                    MoreThanFiveYearsAgo = table.Column<bool>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RiskFactorSmoking", x => x.SocialRiskFactorsNotificationId);
                    table.ForeignKey(
                        name: "FK_RiskFactorSmoking_SocialRiskFactors_SocialRiskFactorsNotificationId",
                        column: x => x.SocialRiskFactorsNotificationId,
                        principalTable: "SocialRiskFactors",
                        principalColumn: "NotificationId",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RiskFactorSmoking");

            migrationBuilder.AddColumn<string>(
                name: "SmokingStatus",
                table: "SocialRiskFactors",
                maxLength: 30,
                nullable: true);
        }
    }
}
