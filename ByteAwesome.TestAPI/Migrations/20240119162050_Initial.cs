using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ByteAwesome.TestAPI.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            _ = migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            _ = migrationBuilder.CreateTable(
                name: "CampaignUserSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    UserId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    CreatedBy = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LastModifiedTime = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    _ = table.PrimaryKey("PK_CampaignUserSessions", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            _ = migrationBuilder.CreateTable(
                name: "Rewards",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Code = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MoneyPool = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    PoolBalance = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    Symbol = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Min = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    Max = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    DecimalPlaces = table.Column<int>(type: "int", nullable: false),
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
                    _ = table.PrimaryKey("PK_Rewards", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            _ = migrationBuilder.CreateTable(
                name: "RewardCampaigns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Code = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StartAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    EndAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    IsReclaimable = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ReclaimCoolDownDays = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RewardsId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
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
                    _ = table.PrimaryKey("PK_RewardCampaigns", x => x.Id);
                    _ = table.ForeignKey(
                        name: "FK_RewardCampaigns_Rewards_RewardsId",
                        column: x => x.RewardsId,
                        principalTable: "Rewards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            _ = migrationBuilder.CreateTable(
                name: "Claims",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    CampaignsId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    RewardCampaignsId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    UserId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ClaimDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    Symbol = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    _ = table.PrimaryKey("PK_Claims", x => x.Id);
                    _ = table.ForeignKey(
                        name: "FK_Claims_RewardCampaigns_RewardCampaignsId",
                        column: x => x.RewardCampaignsId,
                        principalTable: "RewardCampaigns",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            _ = migrationBuilder.CreateIndex(
                name: "IX_Claims_RewardCampaignsId",
                table: "Claims",
                column: "RewardCampaignsId");

            _ = migrationBuilder.CreateIndex(
                name: "IX_RewardCampaigns_RewardsId",
                table: "RewardCampaigns",
                column: "RewardsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            _ = migrationBuilder.DropTable(
                name: "CampaignUserSessions");

            _ = migrationBuilder.DropTable(
                name: "Claims");

            _ = migrationBuilder.DropTable(
                name: "RewardCampaigns");

            _ = migrationBuilder.DropTable(
                name: "Rewards");
        }
    }
}
