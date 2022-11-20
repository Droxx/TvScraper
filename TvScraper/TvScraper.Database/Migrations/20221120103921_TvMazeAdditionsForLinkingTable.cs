using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TvScraper.Database.Migrations
{
    public partial class TvMazeAdditionsForLinkingTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TvMazeId",
                table: "CastMembers",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TvMazeId",
                table: "CastMembers");
        }
    }
}
