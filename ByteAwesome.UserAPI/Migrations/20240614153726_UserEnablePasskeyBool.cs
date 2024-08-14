using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ByteAwesome.UserAPI.Migrations
{
    /// <inheritdoc />
    public partial class UserEnablePasskeyBool : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            _ = migrationBuilder.AddColumn<bool>(
                name: "IsPassKeyEnabled",
                table: "Users",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            _ = migrationBuilder.DropColumn(
                name: "IsPassKeyEnabled",
                table: "Users");
        }
    }
}
