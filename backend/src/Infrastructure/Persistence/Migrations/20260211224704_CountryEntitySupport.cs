using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaaS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CountryEntitySupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Customers_Countries_BillingCountryId1",
                table: "Customers");

            migrationBuilder.DropForeignKey(
                name: "FK_Customers_Countries_ShippingCountryId1",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Customers_BillingCountryId1",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Customers_ShippingCountryId1",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "BillingCountryId1",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "ShippingCountryId1",
                table: "Customers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "BillingCountryId1",
                table: "Customers",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ShippingCountryId1",
                table: "Customers",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Customers_BillingCountryId1",
                table: "Customers",
                column: "BillingCountryId1");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_ShippingCountryId1",
                table: "Customers",
                column: "ShippingCountryId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_Countries_BillingCountryId1",
                table: "Customers",
                column: "BillingCountryId1",
                principalTable: "Countries",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_Countries_ShippingCountryId1",
                table: "Customers",
                column: "ShippingCountryId1",
                principalTable: "Countries",
                principalColumn: "Id");
        }
    }
}
