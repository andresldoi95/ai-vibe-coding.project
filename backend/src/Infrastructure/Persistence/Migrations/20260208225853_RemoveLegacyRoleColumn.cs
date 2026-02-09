using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaaS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveLegacyRoleColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LegacyRole",
                table: "UserTenants");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LegacyRole",
                table: "UserTenants",
                type: "integer",
                nullable: true);
        }
    }
}
