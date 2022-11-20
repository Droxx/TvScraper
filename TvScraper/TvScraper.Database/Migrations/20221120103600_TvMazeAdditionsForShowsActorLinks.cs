using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TvScraper.Database.Migrations
{
    public partial class TvMazeAdditionsForShowsActorLinks : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ActorsScraped",
                table: "Shows",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastScrapeDate",
                table: "Shows",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "TvMazeId",
                table: "Actors",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActorsScraped",
                table: "Shows");

            migrationBuilder.DropColumn(
                name: "LastScrapeDate",
                table: "Shows");

            migrationBuilder.DropColumn(
                name: "TvMazeId",
                table: "Actors");
        }
    }
}
