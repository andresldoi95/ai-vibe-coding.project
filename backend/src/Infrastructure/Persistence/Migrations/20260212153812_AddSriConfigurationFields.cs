using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaaS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSriConfigurationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "SriConfigurations",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsRiseRegime",
                table: "SriConfigurations",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "SriConfigurations",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SpecialTaxpayerNumber",
                table: "SriConfigurations",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Email",
                table: "SriConfigurations");

            migrationBuilder.DropColumn(
                name: "IsRiseRegime",
                table: "SriConfigurations");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "SriConfigurations");

            migrationBuilder.DropColumn(
                name: "SpecialTaxpayerNumber",
                table: "SriConfigurations");
        }
    }
}
