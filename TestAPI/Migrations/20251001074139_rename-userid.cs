using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestAPI.Migrations
{
    /// <inheritdoc />
    public partial class renameuserid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UsersId",
                table: "Games",
                newName: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Games",
                newName: "UsersId");
        }
    }
}
