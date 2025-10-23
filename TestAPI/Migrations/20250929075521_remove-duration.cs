using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestAPI.Migrations
{
    /// <inheritdoc />
    public partial class removeduration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Duration",
                table: "GameScores");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Duration",
                table: "GameScores",
                type: "double",
                nullable: false,
                defaultValue: 0.0);
        }
    }
}
