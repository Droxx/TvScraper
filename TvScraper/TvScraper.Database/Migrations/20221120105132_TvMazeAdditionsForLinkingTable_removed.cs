using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TvScraper.Database.Migrations
{
    public partial class TvMazeAdditionsForLinkingTable_removed : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TvMazeId",
                table: "CastMembers");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TvMazeId",
                table: "CastMembers",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
