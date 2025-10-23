using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestAPI.Migrations
{
    /// <inheritdoc />
    public partial class changescoretoint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Score",
                table: "GameScores",
                type: "int",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "double");

            migrationBuilder.AlterColumn<int>(
                name: "FinalScore",
                table: "Games",
                type: "int",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "double");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "Score",
                table: "GameScores",
                type: "double",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<double>(
                name: "FinalScore",
                table: "Games",
                type: "double",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}
