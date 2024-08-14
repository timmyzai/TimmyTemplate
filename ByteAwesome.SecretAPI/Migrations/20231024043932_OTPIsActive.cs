using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ByteAwesome.SecretAPI.Migrations
{
    /// <inheritdoc />
    public partial class OTPIsActive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            _ = migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Otp",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            _ = migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Otp");
        }
    }
}
