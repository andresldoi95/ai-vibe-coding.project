using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaaS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSriEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AccessKey",
                table: "Invoices",
                type: "character varying(49)",
                maxLength: 49,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DocumentType",
                table: "Invoices",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<Guid>(
                name: "EmissionPointId",
                table: "Invoices",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Environment",
                table: "Invoices",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "PaymentMethod",
                table: "Invoices",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<string>(
                name: "SignedXmlFilePath",
                table: "Invoices",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "XmlFilePath",
                table: "Invoices",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IdentificationType",
                table: "Customers",
                type: "integer",
                nullable: false,
                defaultValue: 5);

            migrationBuilder.CreateTable(
                name: "Establishments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EstablishmentCode = table.Column<string>(type: "character(3)", fixedLength: true, maxLength: 3, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Phone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Establishments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SriConfigurations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyRuc = table.Column<string>(type: "character(13)", fixedLength: true, maxLength: 13, nullable: false),
                    LegalName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    TradeName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    MainAddress = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    AccountingRequired = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    Environment = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    DigitalCertificate = table.Column<byte[]>(type: "bytea", nullable: true),
                    CertificatePassword = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CertificateExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SriConfigurations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmissionPoints",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmissionPointCode = table.Column<string>(type: "character(3)", fixedLength: true, maxLength: 3, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    InvoiceSequence = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    CreditNoteSequence = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    DebitNoteSequence = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    RetentionSequence = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    EstablishmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmissionPoints", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmissionPoints_Establishments_EstablishmentId",
                        column: x => x.EstablishmentId,
                        principalTable: "Establishments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_AccessKey",
                table: "Invoices",
                column: "AccessKey",
                unique: true,
                filter: "\"AccessKey\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_EmissionPointId",
                table: "Invoices",
                column: "EmissionPointId");

            migrationBuilder.CreateIndex(
                name: "IX_EmissionPoints_EstablishmentId",
                table: "EmissionPoints",
                column: "EstablishmentId");

            migrationBuilder.CreateIndex(
                name: "IX_EmissionPoints_EstablishmentId_Code",
                table: "EmissionPoints",
                columns: new[] { "EstablishmentId", "EmissionPointCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Establishments_TenantId",
                table: "Establishments",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Establishments_TenantId_Code",
                table: "Establishments",
                columns: new[] { "TenantId", "EstablishmentCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SriConfigurations_CompanyRuc",
                table: "SriConfigurations",
                column: "CompanyRuc");

            migrationBuilder.CreateIndex(
                name: "IX_SriConfigurations_TenantId",
                table: "SriConfigurations",
                column: "TenantId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Invoices_EmissionPoints_EmissionPointId",
                table: "Invoices",
                column: "EmissionPointId",
                principalTable: "EmissionPoints",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_EmissionPoints_EmissionPointId",
                table: "Invoices");

            migrationBuilder.DropTable(
                name: "EmissionPoints");

            migrationBuilder.DropTable(
                name: "SriConfigurations");

            migrationBuilder.DropTable(
                name: "Establishments");

            migrationBuilder.DropIndex(
                name: "IX_Invoices_AccessKey",
                table: "Invoices");

            migrationBuilder.DropIndex(
                name: "IX_Invoices_EmissionPointId",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "AccessKey",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "DocumentType",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "EmissionPointId",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "Environment",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "PaymentMethod",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "SignedXmlFilePath",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "XmlFilePath",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "IdentificationType",
                table: "Customers");
        }
    }
}
