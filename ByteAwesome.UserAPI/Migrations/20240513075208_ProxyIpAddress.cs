using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ByteAwesome.UserAPI.Migrations
{
    /// <inheritdoc />
    public partial class ProxyIpAddress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            _ = migrationBuilder.AddColumn<string>(
                name: "ProxyIpAddress",
                table: "UserLoginSessionInfo",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            _ = migrationBuilder.DropColumn(
                name: "ProxyIpAddress",
                table: "UserLoginSessionInfo");
        }
    }
}
