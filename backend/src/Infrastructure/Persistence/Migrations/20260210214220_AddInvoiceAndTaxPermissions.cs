using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaaS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddInvoiceAndTaxPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            SeedInvoiceAndTaxPermissions(migrationBuilder);
            AssignPermissionsToRoles(migrationBuilder);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove permissions from roles
            migrationBuilder.Sql(@"
                DELETE FROM ""RolePermissions""
                WHERE ""PermissionId"" IN (
                    SELECT ""Id"" FROM ""Permissions""
                    WHERE ""Resource"" IN ('invoices', 'tax-rates', 'invoice-config')
                );
            ");

            // Delete the permissions
            migrationBuilder.Sql(@"
                DELETE FROM ""Permissions""
                WHERE ""Resource"" IN ('invoices', 'tax-rates', 'invoice-config');
            ");
        }

        private void SeedInvoiceAndTaxPermissions(MigrationBuilder migrationBuilder)
        {
            var permissions = new[]
            {
                // Invoices permissions
                ("invoices.read", "View invoices", "invoices", "read"),
                ("invoices.create", "Create invoices", "invoices", "create"),
                ("invoices.update", "Update invoices", "invoices", "update"),
                ("invoices.delete", "Delete invoices", "invoices", "delete"),
                ("invoices.send", "Send invoices to customers", "invoices", "send"),
                ("invoices.void", "Void/cancel invoices", "invoices", "void"),
                ("invoices.export", "Export invoice data", "invoices", "export"),

                // Tax rates permissions
                ("tax-rates.read", "View tax rates", "tax-rates", "read"),
                ("tax-rates.create", "Create tax rates", "tax-rates", "create"),
                ("tax-rates.update", "Update tax rates", "tax-rates", "update"),
                ("tax-rates.delete", "Delete tax rates", "tax-rates", "delete"),

                // Invoice configuration permissions
                ("invoice-config.read", "View invoice configuration", "invoice-config", "read"),
                ("invoice-config.update", "Update invoice configuration", "invoice-config", "update")
            };

            foreach (var (name, description, resource, action) in permissions)
            {
                migrationBuilder.InsertData(
                    table: "Permissions",
                    columns: new[] { "Id", "Name", "Description", "Resource", "Action" },
                    values: new object[] { Guid.NewGuid(), name, description, resource, action });
            }
        }

        private void AssignPermissionsToRoles(MigrationBuilder migrationBuilder)
        {
            // Owner: Gets ALL new permissions
            migrationBuilder.Sql(@"
                INSERT INTO ""RolePermissions"" (""Id"", ""RoleId"", ""PermissionId"")
                SELECT gen_random_uuid(), r.""Id"", p.""Id""
                FROM ""Roles"" r
                CROSS JOIN ""Permissions"" p
                WHERE r.""Name"" = 'Owner'
                  AND p.""Resource"" IN ('invoices', 'tax-rates', 'invoice-config');
            ");

            // Admin: Gets all invoice and tax permissions
            migrationBuilder.Sql(@"
                INSERT INTO ""RolePermissions"" (""Id"", ""RoleId"", ""PermissionId"")
                SELECT gen_random_uuid(), r.""Id"", p.""Id""
                FROM ""Roles"" r
                CROSS JOIN ""Permissions"" p
                WHERE r.""Name"" = 'Admin'
                  AND p.""Resource"" IN ('invoices', 'tax-rates', 'invoice-config');
            ");

            // Manager: Gets read, create, update, send, export for invoices; read/update for tax-rates
            migrationBuilder.Sql(@"
                INSERT INTO ""RolePermissions"" (""Id"", ""RoleId"", ""PermissionId"")
                SELECT gen_random_uuid(), r.""Id"", p.""Id""
                FROM ""Roles"" r
                CROSS JOIN ""Permissions"" p
                WHERE r.""Name"" = 'Manager'
                  AND (
                      (p.""Resource"" = 'invoices' AND p.""Action"" IN ('read', 'create', 'update', 'send', 'export'))
                      OR (p.""Resource"" = 'tax-rates' AND p.""Action"" IN ('read', 'update'))
                      OR (p.""Resource"" = 'invoice-config' AND p.""Action"" = 'read')
                  );
            ");

            // User: Gets only read permissions for invoices, tax-rates, and invoice-config
            migrationBuilder.Sql(@"
                INSERT INTO ""RolePermissions"" (""Id"", ""RoleId"", ""PermissionId"")
                SELECT gen_random_uuid(), r.""Id"", p.""Id""
                FROM ""Roles"" r
                CROSS JOIN ""Permissions"" p
                WHERE r.""Name"" = 'User'
                  AND p.""Resource"" IN ('invoices', 'tax-rates', 'invoice-config')
                  AND p.""Action"" = 'read';
            ");
        }
    }
}
