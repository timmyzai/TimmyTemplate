using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ByteAwesome.SecretAPI.Migrations
{
    /// <inheritdoc />
    public partial class PassKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            _ = migrationBuilder.CreateTable(
                name: "Passkey",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    CredentialId = table.Column<byte[]>(type: "longblob", nullable: true),
                    PublicKey = table.Column<byte[]>(type: "longblob", nullable: true),
                    UserHandle = table.Column<byte[]>(type: "longblob", nullable: true),
                    SignatureCounter = table.Column<uint>(type: "int unsigned", nullable: false),
                    UserName = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    _ = table.PrimaryKey("PK_Passkey", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            _ = migrationBuilder.DropTable(
                name: "Passkey");
        }
    }
}
