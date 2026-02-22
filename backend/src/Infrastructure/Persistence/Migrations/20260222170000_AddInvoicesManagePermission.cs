using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaaS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddInvoicesManagePermission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add invoices.manage permission
            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "Name", "Description", "Resource", "Action" },
                values: new object[] { Guid.NewGuid(), "invoices.manage", "Manage invoice SRI workflow (XML generation, signing, submission)", "invoices", "manage" });

            // Owner: Gets the new permission
            migrationBuilder.Sql(@"
                INSERT INTO ""RolePermissions"" (""Id"", ""RoleId"", ""PermissionId"")
                SELECT gen_random_uuid(), r.""Id"", p.""Id""
                FROM ""Roles"" r
                CROSS JOIN ""Permissions"" p
                WHERE r.""Name"" = 'Owner'
                  AND p.""Name"" = 'invoices.manage';
            ");

            // Admin: Gets the new permission
            migrationBuilder.Sql(@"
                INSERT INTO ""RolePermissions"" (""Id"", ""RoleId"", ""PermissionId"")
                SELECT gen_random_uuid(), r.""Id"", p.""Id""
                FROM ""Roles"" r
                CROSS JOIN ""Permissions"" p
                WHERE r.""Name"" = 'Admin'
                  AND p.""Name"" = 'invoices.manage';
            ");

            // Manager: Gets the new permission
            migrationBuilder.Sql(@"
                INSERT INTO ""RolePermissions"" (""Id"", ""RoleId"", ""PermissionId"")
                SELECT gen_random_uuid(), r.""Id"", p.""Id""
                FROM ""Roles"" r
                CROSS JOIN ""Permissions"" p
                WHERE r.""Name"" = 'Manager'
                  AND p.""Name"" = 'invoices.manage';
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove invoices.manage permission from roles
            migrationBuilder.Sql(@"
                DELETE FROM ""RolePermissions""
                WHERE ""PermissionId"" IN (
                    SELECT ""Id"" FROM ""Permissions""
                    WHERE ""Name"" = 'invoices.manage'
                );
            ");

            // Delete the permission
            migrationBuilder.Sql(@"
                DELETE FROM ""Permissions""
                WHERE ""Name"" = 'invoices.manage';
            ");
        }
    }
}
