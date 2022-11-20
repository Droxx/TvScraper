using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TvScraper.Database.Migrations
{
    public partial class TvMazeAdditionsForShows : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TvMazeId",
                table: "Shows",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "TvMazeUrl",
                table: "Shows",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TvMazeId",
                table: "Shows");

            migrationBuilder.DropColumn(
                name: "TvMazeUrl",
                table: "Shows");
        }
    }
}
