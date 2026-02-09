using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaaS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRolesAndPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Step 1: Preserve existing role data in LegacyRole before dropping Role column
            migrationBuilder.RenameColumn(
                name: "Role",
                table: "UserTenants",
                newName: "LegacyRole");

            // Make LegacyRole nullable since it's deprecated
            migrationBuilder.AlterColumn<int>(
                name: "LegacyRole",
                table: "UserTenants",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: false);

            migrationBuilder.AddColumn<Guid>(
                name: "RoleId",
                table: "UserTenants",
                type: "uuid",
                nullable: true);

            // Step 2: Create Permissions table
            migrationBuilder.CreateTable(
                name: "Permissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Resource = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Action = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => x.Id);
                });

            // Step 3: Create Roles table
            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    IsSystemRole = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            // Step 4: Create RolePermissions junction table
            migrationBuilder.CreateTable(
                name: "RolePermissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    PermissionId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RolePermissions_Permissions_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "Permissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RolePermissions_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Step 5: Create indexes
            migrationBuilder.CreateIndex(
                name: "IX_UserTenants_RoleId",
                table: "UserTenants",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_Name",
                table: "Permissions",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_Resource_Action",
                table: "Permissions",
                columns: new[] { "Resource", "Action" });

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_PermissionId",
                table: "RolePermissions",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_RoleId_PermissionId",
                table: "RolePermissions",
                columns: new[] { "RoleId", "PermissionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Roles_Priority",
                table: "Roles",
                column: "Priority");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_TenantId_Name",
                table: "Roles",
                columns: new[] { "TenantId", "Name" },
                unique: true);

            // Step 6: Seed Permissions (24 permissions for warehouses, tenants, users)
            SeedPermissions(migrationBuilder);

            // Step 7: Seed Roles for each existing tenant (4 roles per tenant)
            SeedRolesForExistingTenants(migrationBuilder);

            // Step 8: Seed RolePermissions (map permissions to roles)
            SeedRolePermissions(migrationBuilder);

            // Step 9: Migrate existing UserTenant records from enum to RoleId
            MigrateUserTenantRoles(migrationBuilder);

            // Step 10: Add foreign key constraint
            migrationBuilder.AddForeignKey(
                name: "FK_UserTenants_Roles_RoleId",
                table: "UserTenants",
                column: "RoleId",
                principalTable: "Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        private void SeedPermissions(MigrationBuilder migrationBuilder)
        {
            var permissions = new[]
            {
                // Warehouses permissions
                ("warehouses.read", "View warehouses", "warehouses", "read"),
                ("warehouses.create", "Create warehouses", "warehouses", "create"),
                ("warehouses.update", "Update warehouses", "warehouses", "update"),
                ("warehouses.delete", "Delete warehouses", "warehouses", "delete"),

                // Products permissions
                ("products.read", "View products", "products", "read"),
                ("products.create", "Create products", "products", "create"),
                ("products.update", "Update products", "products", "update"),
                ("products.delete", "Delete products", "products", "delete"),

                // Customers permissions
                ("customers.read", "View customers", "customers", "read"),
                ("customers.create", "Create customers", "customers", "create"),
                ("customers.update", "Update customers", "customers", "update"),
                ("customers.delete", "Delete customers", "customers", "delete"),

                // Stock movements permissions
                ("stock.read", "View stock movements", "stock", "read"),
                ("stock.create", "Create stock movements", "stock", "create"),
                ("stock.update", "Update stock movements", "stock", "update"),
                ("stock.delete", "Delete stock movements", "stock", "delete"),

                // Tenants permissions
                ("tenants.read", "View tenants", "tenants", "read"),
                ("tenants.create", "Create tenants", "tenants", "create"),
                ("tenants.update", "Update tenants", "tenants", "update"),
                ("tenants.delete", "Delete tenants", "tenants", "delete"),

                // Users permissions
                ("users.read", "View users", "users", "read"),
                ("users.create", "Create users", "users", "create"),
                ("users.update", "Update users", "users", "update"),
                ("users.delete", "Delete users", "users", "delete"),
                ("users.invite", "Invite users to tenant", "users", "invite"),
                ("users.remove", "Remove users from tenant", "users", "remove"),

                // Roles permissions
                ("roles.read", "View roles", "roles", "read"),
                ("roles.manage", "Manage roles and permissions", "roles", "manage")
            };

            foreach (var (name, description, resource, action) in permissions)
            {
                migrationBuilder.InsertData(
                    table: "Permissions",
                    columns: new[] { "Id", "Name", "Description", "Resource", "Action" },
                    values: new object[] { Guid.NewGuid(), name, description, resource, action });
            }
        }

        private void SeedRolesForExistingTenants(MigrationBuilder migrationBuilder)
        {
            // This will be executed as raw SQL to create roles for each tenant
            migrationBuilder.Sql(@"
                INSERT INTO ""Roles"" (""Id"", ""Name"", ""Description"", ""Priority"", ""IsSystemRole"", ""IsActive"", ""TenantId"", ""CreatedAt"", ""UpdatedAt"", ""IsDeleted"")
                SELECT
                    gen_random_uuid(),
                    'Owner',
                    'Full system access with all permissions',
                    100,
                    true,
                    true,
                    ""Id"",
                    NOW(),
                    NOW(),
                    false
                FROM ""Tenants""
                WHERE ""IsDeleted"" = false;

                INSERT INTO ""Roles"" (""Id"", ""Name"", ""Description"", ""Priority"", ""IsSystemRole"", ""IsActive"", ""TenantId"", ""CreatedAt"", ""UpdatedAt"", ""IsDeleted"")
                SELECT
                    gen_random_uuid(),
                    'Admin',
                    'Administrative access with most permissions',
                    50,
                    true,
                    true,
                    ""Id"",
                    NOW(),
                    NOW(),
                    false
                FROM ""Tenants""
                WHERE ""IsDeleted"" = false;

                INSERT INTO ""Roles"" (""Id"", ""Name"", ""Description"", ""Priority"", ""IsSystemRole"", ""IsActive"", ""TenantId"", ""CreatedAt"", ""UpdatedAt"", ""IsDeleted"")
                SELECT
                    gen_random_uuid(),
                    'Manager',
                    'Manager access with read and update permissions',
                    25,
                    true,
                    true,
                    ""Id"",
                    NOW(),
                    NOW(),
                    false
                FROM ""Tenants""
                WHERE ""IsDeleted"" = false;

                INSERT INTO ""Roles"" (""Id"", ""Name"", ""Description"", ""Priority"", ""IsSystemRole"", ""IsActive"", ""TenantId"", ""CreatedAt"", ""UpdatedAt"", ""IsDeleted"")
                SELECT
                    gen_random_uuid(),
                    'User',
                    'Basic user with read-only access',
                    10,
                    true,
                    true,
                    ""Id"",
                    NOW(),
                    NOW(),
                    false
                FROM ""Tenants""
                WHERE ""IsDeleted"" = false;
            ");
        }

        private void SeedRolePermissions(MigrationBuilder migrationBuilder)
        {
            // Owner gets ALL permissions
            migrationBuilder.Sql(@"
                INSERT INTO ""RolePermissions"" (""Id"", ""RoleId"", ""PermissionId"")
                SELECT gen_random_uuid(), r.""Id"", p.""Id""
                FROM ""Roles"" r
                CROSS JOIN ""Permissions"" p
                WHERE r.""Name"" = 'Owner';
            ");

            // Admin gets all except tenants.delete and users.remove
            migrationBuilder.Sql(@"
                INSERT INTO ""RolePermissions"" (""Id"", ""RoleId"", ""PermissionId"")
                SELECT gen_random_uuid(), r.""Id"", p.""Id""
                FROM ""Roles"" r
                CROSS JOIN ""Permissions"" p
                WHERE r.""Name"" = 'Admin'
                  AND p.""Name"" NOT IN ('tenants.delete', 'users.remove');
            ");

            // Manager gets read + update for warehouses, products, customers, stock; read for users, tenants
            migrationBuilder.Sql(@"
                INSERT INTO ""RolePermissions"" (""Id"", ""RoleId"", ""PermissionId"")
                SELECT gen_random_uuid(), r.""Id"", p.""Id""
                FROM ""Roles"" r
                CROSS JOIN ""Permissions"" p
                WHERE r.""Name"" = 'Manager'
                  AND p.""Name"" IN (
                      'warehouses.read', 'warehouses.update',
                      'products.read', 'products.update',
                      'customers.read', 'customers.update',
                      'stock.read', 'stock.update',
                      'users.read',
                      'tenants.read'
                  );
            ");

            // User gets only read permissions
            migrationBuilder.Sql(@"
                INSERT INTO ""RolePermissions"" (""Id"", ""RoleId"", ""PermissionId"")
                SELECT gen_random_uuid(), r.""Id"", p.""Id""
                FROM ""Roles"" r
                CROSS JOIN ""Permissions"" p
                WHERE r.""Name"" = 'User'
                  AND p.""Action"" = 'read';
            ");
        }

        private void MigrateUserTenantRoles(MigrationBuilder migrationBuilder)
        {
            // Map LegacyRole enum (0=Owner, 1=Admin, 2=Manager, 3=User) to RoleId
            migrationBuilder.Sql(@"
                UPDATE ""UserTenants"" ut
                SET ""RoleId"" = (
                    SELECT r.""Id""
                    FROM ""Roles"" r
                    WHERE r.""TenantId"" = ut.""TenantId""
                      AND r.""Name"" = CASE ut.""LegacyRole""
                          WHEN 0 THEN 'Owner'
                          WHEN 1 THEN 'Admin'
                          WHEN 2 THEN 'Manager'
                          WHEN 3 THEN 'User'
                          ELSE 'User'
                      END
                    LIMIT 1
                )
                WHERE ut.""LegacyRole"" IS NOT NULL;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserTenants_Roles_RoleId",
                table: "UserTenants");

            migrationBuilder.DropTable(
                name: "RolePermissions");

            migrationBuilder.DropTable(
                name: "Permissions");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropIndex(
                name: "IX_UserTenants_RoleId",
                table: "UserTenants");

            migrationBuilder.DropColumn(
                name: "LegacyRole",
                table: "UserTenants");

            migrationBuilder.DropColumn(
                name: "RoleId",
                table: "UserTenants");

            migrationBuilder.AddColumn<int>(
                name: "Role",
                table: "UserTenants",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
