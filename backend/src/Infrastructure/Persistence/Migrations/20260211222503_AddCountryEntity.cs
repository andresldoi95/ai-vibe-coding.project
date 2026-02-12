using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaaS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCountryEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TaxRates_Country",
                table: "TaxRates");

            migrationBuilder.DropIndex(
                name: "IX_Customers_BillingCountry",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "Country",
                table: "Warehouses");

            migrationBuilder.DropColumn(
                name: "Country",
                table: "TaxRates");

            migrationBuilder.DropColumn(
                name: "BillingCountry",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "ShippingCountry",
                table: "Customers");

            migrationBuilder.AddColumn<Guid>(
                name: "CountryId",
                table: "Warehouses",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "CountryId",
                table: "TaxRates",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "BillingCountryId",
                table: "Customers",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "BillingCountryId1",
                table: "Customers",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ShippingCountryId",
                table: "Customers",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ShippingCountryId1",
                table: "Customers",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Countries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Alpha3Code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    NumericCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Countries", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Warehouses_CountryId",
                table: "Warehouses",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_TaxRates_CountryId",
                table: "TaxRates",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_BillingCountryId",
                table: "Customers",
                column: "BillingCountryId");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_BillingCountryId1",
                table: "Customers",
                column: "BillingCountryId1");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_ShippingCountryId",
                table: "Customers",
                column: "ShippingCountryId");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_ShippingCountryId1",
                table: "Customers",
                column: "ShippingCountryId1");

            migrationBuilder.CreateIndex(
                name: "IX_Countries_Code",
                table: "Countries",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Countries_IsActive",
                table: "Countries",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Countries_Name",
                table: "Countries",
                column: "Name");

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_Countries_BillingCountryId",
                table: "Customers",
                column: "BillingCountryId",
                principalTable: "Countries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_Countries_BillingCountryId1",
                table: "Customers",
                column: "BillingCountryId1",
                principalTable: "Countries",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_Countries_ShippingCountryId",
                table: "Customers",
                column: "ShippingCountryId",
                principalTable: "Countries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_Countries_ShippingCountryId1",
                table: "Customers",
                column: "ShippingCountryId1",
                principalTable: "Countries",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TaxRates_Countries_CountryId",
                table: "TaxRates",
                column: "CountryId",
                principalTable: "Countries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Warehouses_Countries_CountryId",
                table: "Warehouses",
                column: "CountryId",
                principalTable: "Countries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Customers_Countries_BillingCountryId",
                table: "Customers");

            migrationBuilder.DropForeignKey(
                name: "FK_Customers_Countries_BillingCountryId1",
                table: "Customers");

            migrationBuilder.DropForeignKey(
                name: "FK_Customers_Countries_ShippingCountryId",
                table: "Customers");

            migrationBuilder.DropForeignKey(
                name: "FK_Customers_Countries_ShippingCountryId1",
                table: "Customers");

            migrationBuilder.DropForeignKey(
                name: "FK_TaxRates_Countries_CountryId",
                table: "TaxRates");

            migrationBuilder.DropForeignKey(
                name: "FK_Warehouses_Countries_CountryId",
                table: "Warehouses");

            migrationBuilder.DropTable(
                name: "Countries");

            migrationBuilder.DropIndex(
                name: "IX_Warehouses_CountryId",
                table: "Warehouses");

            migrationBuilder.DropIndex(
                name: "IX_TaxRates_CountryId",
                table: "TaxRates");

            migrationBuilder.DropIndex(
                name: "IX_Customers_BillingCountryId",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Customers_BillingCountryId1",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Customers_ShippingCountryId",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Customers_ShippingCountryId1",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "CountryId",
                table: "Warehouses");

            migrationBuilder.DropColumn(
                name: "CountryId",
                table: "TaxRates");

            migrationBuilder.DropColumn(
                name: "BillingCountryId",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "BillingCountryId1",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "ShippingCountryId",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "ShippingCountryId1",
                table: "Customers");

            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "Warehouses",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "TaxRates",
                type: "character varying(2)",
                maxLength: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BillingCountry",
                table: "Customers",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShippingCountry",
                table: "Customers",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TaxRates_Country",
                table: "TaxRates",
                column: "Country");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_BillingCountry",
                table: "Customers",
                column: "BillingCountry");
        }
    }
}
