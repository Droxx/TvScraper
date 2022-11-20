using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TvScraper.Database.Migrations
{
    public partial class FixedMistakeInDbContext : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CastMembers_Shows_ActorId",
                table: "CastMembers");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateOfBirth",
                table: "Actors",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.CreateIndex(
                name: "IX_CastMembers_ShowId",
                table: "CastMembers",
                column: "ShowId");

            migrationBuilder.AddForeignKey(
                name: "FK_CastMembers_Shows_ShowId",
                table: "CastMembers",
                column: "ShowId",
                principalTable: "Shows",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CastMembers_Shows_ShowId",
                table: "CastMembers");

            migrationBuilder.DropIndex(
                name: "IX_CastMembers_ShowId",
                table: "CastMembers");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateOfBirth",
                table: "Actors",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CastMembers_Shows_ActorId",
                table: "CastMembers",
                column: "ActorId",
                principalTable: "Shows",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
