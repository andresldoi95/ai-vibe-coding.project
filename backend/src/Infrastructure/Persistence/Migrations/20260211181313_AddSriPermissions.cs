using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaaS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSriPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            SeedSriPermissions(migrationBuilder);
            // Note: Role-permission assignment happens during demo seeding in SeedController
            // because roles don't exist yet during migration when database is freshly created
            AssignPermissionsToExistingRoles(migrationBuilder);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove permissions from roles
            migrationBuilder.Sql(@"
                DELETE FROM ""RolePermissions""
                WHERE ""PermissionId"" IN (
                    SELECT ""Id"" FROM ""Permissions""
                    WHERE ""Resource"" IN ('establishments', 'emission_points', 'sri_configuration')
                );
            ");

            // Delete the permissions
            migrationBuilder.Sql(@"
                DELETE FROM ""Permissions""
                WHERE ""Resource"" IN ('establishments', 'emission_points', 'sri_configuration');
            ");
        }

        private void SeedSriPermissions(MigrationBuilder migrationBuilder)
        {
            var permissions = new[]
            {
                // Establishments permissions
                ("establishments.read", "View establishments", "establishments", "read"),
                ("establishments.create", "Create establishments", "establishments", "create"),
                ("establishments.update", "Update establishments", "establishments", "update"),
                ("establishments.delete", "Delete establishments", "establishments", "delete"),

                // Emission Points permissions
                ("emission_points.read", "View emission points", "emission_points", "read"),
                ("emission_points.create", "Create emission points", "emission_points", "create"),
                ("emission_points.update", "Update emission points", "emission_points", "update"),
                ("emission_points.delete", "Delete emission points", "emission_points", "delete"),

                // SRI Configuration permissions
                ("sri_configuration.read", "View SRI configuration", "sri_configuration", "read"),
                ("sri_configuration.update", "Update SRI configuration", "sri_configuration", "update")
            };

            foreach (var (name, description, resource, action) in permissions)
            {
                migrationBuilder.InsertData(
                    table: "Permissions",
                    columns: new[] { "Id", "Name", "Description", "Resource", "Action" },
                    values: new object[] { Guid.NewGuid(), name, description, resource, action });
            }
        }

        private void AssignPermissionsToExistingRoles(MigrationBuilder migrationBuilder)
        {
            // Only assign permissions to roles if they exist (for existing databases with data)
            // For fresh databases, permissions will be assigned during seeding in SeedController.CreateRolesForTenant

            // Check if any roles exist before trying to assign permissions
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF EXISTS (SELECT 1 FROM ""Roles"" LIMIT 1) THEN
                        -- Owner: Gets ALL new SRI permissions
                        INSERT INTO ""RolePermissions"" (""Id"", ""RoleId"", ""PermissionId"")
                        SELECT gen_random_uuid(), r.""Id"", p.""Id""
                        FROM ""Roles"" r
                        CROSS JOIN ""Permissions"" p
                        WHERE r.""Name"" = 'Owner'
                          AND p.""Resource"" IN ('establishments', 'emission_points', 'sri_configuration')
                          AND NOT EXISTS (
                              SELECT 1 FROM ""RolePermissions"" rp
                              WHERE rp.""RoleId"" = r.""Id"" AND rp.""PermissionId"" = p.""Id""
                          );

                        -- Admin: Gets all SRI permissions
                        INSERT INTO ""RolePermissions"" (""Id"", ""RoleId"", ""PermissionId"")
                        SELECT gen_random_uuid(), r.""Id"", p.""Id""
                        FROM ""Roles"" r
                        CROSS JOIN ""Permissions"" p
                        WHERE r.""Name"" = 'Admin'
                          AND p.""Resource"" IN ('establishments', 'emission_points', 'sri_configuration')
                          AND NOT EXISTS (
                              SELECT 1 FROM ""RolePermissions"" rp
                              WHERE rp.""RoleId"" = r.""Id"" AND rp.""PermissionId"" = p.""Id""
                          );

                        -- Manager: Full access to establishments and emission_points, read/update for sri_configuration
                        INSERT INTO ""RolePermissions"" (""Id"", ""RoleId"", ""PermissionId"")
                        SELECT gen_random_uuid(), r.""Id"", p.""Id""
                        FROM ""Roles"" r
                        CROSS JOIN ""Permissions"" p
                        WHERE r.""Name"" = 'Manager'
                          AND (
                              p.""Resource"" IN ('establishments', 'emission_points')
                              OR (p.""Resource"" = 'sri_configuration' AND p.""Action"" IN ('read', 'update'))
                          )
                          AND NOT EXISTS (
                              SELECT 1 FROM ""RolePermissions"" rp
                              WHERE rp.""RoleId"" = r.""Id"" AND rp.""PermissionId"" = p.""Id""
                          );

                        -- User: Gets only read permissions for all SRI resources
                        INSERT INTO ""RolePermissions"" (""Id"", ""RoleId"", ""PermissionId"")
                        SELECT gen_random_uuid(), r.""Id"", p.""Id""
                        FROM ""Roles"" r
                        CROSS JOIN ""Permissions"" p
                        WHERE r.""Name"" = 'User'
                          AND p.""Resource"" IN ('establishments', 'emission_points', 'sri_configuration')
                          AND p.""Action"" = 'read'
                          AND NOT EXISTS (
                              SELECT 1 FROM ""RolePermissions"" rp
                              WHERE rp.""RoleId"" = r.""Id"" AND rp.""PermissionId"" = p.""Id""
                          );
                    END IF;
                END $$;
            ");
        }
    }
}
