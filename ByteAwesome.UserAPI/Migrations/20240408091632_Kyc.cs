using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ByteAwesome.UserAPI.Migrations
{
    /// <inheritdoc />
    public partial class Kyc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            _ = migrationBuilder.DropColumn(
                name: "IsKYCApproved",
                table: "Users");

            _ = migrationBuilder.DropColumn(
                name: "KYCApprovedDate",
                table: "Users");

            _ = migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Users",
                type: "char(36)",
                nullable: false,
                collation: "ascii_general_ci",
                oldClrType: typeof(Guid),
                oldType: "char(36)")
                .Annotation("Relational:ColumnOrder", 0)
                .OldAnnotation("Relational:Collation", "ascii_general_ci");

            _ = migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "UserRoles",
                type: "char(36)",
                nullable: false,
                collation: "ascii_general_ci",
                oldClrType: typeof(Guid),
                oldType: "char(36)")
                .Annotation("Relational:ColumnOrder", 0)
                .OldAnnotation("Relational:Collation", "ascii_general_ci");

            _ = migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Roles",
                type: "char(36)",
                nullable: false,
                collation: "ascii_general_ci",
                oldClrType: typeof(Guid),
                oldType: "char(36)")
                .Annotation("Relational:ColumnOrder", 0)
                .OldAnnotation("Relational:Collation", "ascii_general_ci");

            _ = migrationBuilder.CreateTable(
                name: "Kyc",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    UserId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ApprovedDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
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
                    _ = table.PrimaryKey("PK_Kyc", x => x.Id);
                    _ = table.ForeignKey(
                        name: "FK_Kyc_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            _ = migrationBuilder.CreateIndex(
                name: "IX_Kyc_UserId",
                table: "Kyc",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            _ = migrationBuilder.DropTable(
                name: "Kyc");

            _ = migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Users",
                type: "char(36)",
                nullable: false,
                collation: "ascii_general_ci",
                oldClrType: typeof(Guid),
                oldType: "char(36)")
                .OldAnnotation("Relational:Collation", "ascii_general_ci")
                .OldAnnotation("Relational:ColumnOrder", 0);

            _ = migrationBuilder.AddColumn<bool>(
                name: "IsKYCApproved",
                table: "Users",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            _ = migrationBuilder.AddColumn<DateTime>(
                name: "KYCApprovedDate",
                table: "Users",
                type: "datetime(6)",
                nullable: true);

            _ = migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "UserRoles",
                type: "char(36)",
                nullable: false,
                collation: "ascii_general_ci",
                oldClrType: typeof(Guid),
                oldType: "char(36)")
                .OldAnnotation("Relational:Collation", "ascii_general_ci")
                .OldAnnotation("Relational:ColumnOrder", 0);

            _ = migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Roles",
                type: "char(36)",
                nullable: false,
                collation: "ascii_general_ci",
                oldClrType: typeof(Guid),
                oldType: "char(36)")
                .OldAnnotation("Relational:Collation", "ascii_general_ci")
                .OldAnnotation("Relational:ColumnOrder", 0);
        }
    }
}
