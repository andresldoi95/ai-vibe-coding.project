-- Add Payment permissions manually
INSERT INTO "Permissions" ("Id", "Name", "Description", "Resource", "Action")
VALUES
    (gen_random_uuid(), 'payments.read', 'View payments', 'payments', 'read'),
    (gen_random_uuid(), 'payments.create', 'Create payments', 'payments', 'create'),
    (gen_random_uuid(), 'payments.update', 'Update payments', 'payments', 'update'),
    (gen_random_uuid(), 'payments.void', 'Void payments', 'payments', 'void'),
    (gen_random_uuid(), 'payments.delete', 'Delete payments', 'payments', 'delete')
ON CONFLICT DO NOTHING;

-- Assign payment permissions to all roles
INSERT INTO "RolePermissions" ("Id", "RoleId", "PermissionId")
SELECT
    gen_random_uuid(),
    r."Id",
    p."Id"
FROM "Roles" r
CROSS JOIN "Permissions" p
WHERE p."Resource" = 'payments'
  AND NOT EXISTS (
      SELECT 1
      FROM "RolePermissions" rp
      WHERE rp."RoleId" = r."Id" AND rp."PermissionId" = p."Id"
  );

-- Mark the migration as applied
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260213212022_AddPaymentPermissions', '8.0.0')
ON CONFLICT DO NOTHING;

-- Verify permissions were addedSELECT r."Name" as role, p."Name" as permission
FROM "RolePermissions" rp
JOIN "Roles" r ON rp."RoleId" = r."Id"
JOIN "Permissions" p ON rp."PermissionId" = p."Id"
WHERE p."Resource" = 'payments'
ORDER BY r."Name", p."Action";
