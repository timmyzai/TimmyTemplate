using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ByteAwesome.SecretAPI.Migrations
{
    /// <inheritdoc />
    public partial class KmsKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            _ = migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "TwoFactorAuth",
                type: "char(36)",
                nullable: false,
                collation: "ascii_general_ci",
                oldClrType: typeof(Guid),
                oldType: "char(36)")
                .Annotation("Relational:ColumnOrder", 0)
                .OldAnnotation("Relational:Collation", "ascii_general_ci");

            _ = migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Otp",
                type: "char(36)",
                nullable: false,
                collation: "ascii_general_ci",
                oldClrType: typeof(Guid),
                oldType: "char(36)")
                .Annotation("Relational:ColumnOrder", 0)
                .OldAnnotation("Relational:Collation", "ascii_general_ci");

            _ = migrationBuilder.CreateTable(
                name: "KmsKeys",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    UserId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    EncryptedPrivateKey = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PublicKey = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedBy = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LastModifiedTime = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    _ = table.PrimaryKey("PK_KmsKeys", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            _ = migrationBuilder.DropTable(
                name: "KmsKeys");

            _ = migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "TwoFactorAuth",
                type: "char(36)",
                nullable: false,
                collation: "ascii_general_ci",
                oldClrType: typeof(Guid),
                oldType: "char(36)")
                .OldAnnotation("Relational:Collation", "ascii_general_ci")
                .OldAnnotation("Relational:ColumnOrder", 0);

            _ = migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Otp",
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
