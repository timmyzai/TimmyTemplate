using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestAPI.Migrations
{
    /// <inheritdoc />
    public partial class updateGameGameScorerelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_GameScores_GamesId",
                table: "GameScores",
                column: "GamesId");

            migrationBuilder.AddForeignKey(
                name: "FK_GameScores_Games_GamesId",
                table: "GameScores",
                column: "GamesId",
                principalTable: "Games",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GameScores_Games_GamesId",
                table: "GameScores");

            migrationBuilder.DropIndex(
                name: "IX_GameScores_GamesId",
                table: "GameScores");
        }
    }
}
