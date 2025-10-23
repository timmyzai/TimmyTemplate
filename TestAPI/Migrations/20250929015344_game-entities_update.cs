using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestAPI.Migrations
{
    /// <inheritdoc />
    public partial class gameentities_update : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropColumn(
                name: "ActualRounds",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "EndTime",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "StartTime",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "UsersId",
                table: "Users",
                newName: "GamesId");

            migrationBuilder.RenameColumn(
                name: "MaxRounds",
                table: "Users",
                newName: "RoundNumber");

            migrationBuilder.RenameColumn(
                name: "FinalScore",
                table: "Users",
                newName: "Score");

            migrationBuilder.AddColumn<string>(
                name: "Answer",
                table: "Users",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<double>(
                name: "Duration",
                table: "Users",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<bool>(
                name: "IsCorrect",
                table: "Users",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "WordPrompt",
                table: "Users",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Games",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    UsersId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    FinalScore = table.Column<double>(type: "double", nullable: false),
                    MaxRounds = table.Column<int>(type: "int", nullable: false),
                    ActualRounds = table.Column<int>(type: "int", nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedBy = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LastModifiedTime = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DeletedBy = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DeletedTime = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Games", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Games");

            migrationBuilder.DropColumn(
                name: "Answer",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Duration",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsCorrect",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "WordPrompt",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "Score",
                table: "Users",
                newName: "FinalScore");

            migrationBuilder.RenameColumn(
                name: "RoundNumber",
                table: "Users",
                newName: "MaxRounds");

            migrationBuilder.RenameColumn(
                name: "GamesId",
                table: "Users",
                newName: "UsersId");

            migrationBuilder.AddColumn<int>(
                name: "ActualRounds",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "EndTime",
                table: "Users",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "StartTime",
                table: "Users",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Answer = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedBy = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DeletedBy = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DeletedTime = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Duration = table.Column<double>(type: "double", nullable: false),
                    GamesId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    IsCorrect = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LastModifiedTime = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    RoundNumber = table.Column<int>(type: "int", nullable: false),
                    Score = table.Column<double>(type: "double", nullable: false),
                    WordPrompt = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
