-- Add invoices.manage permission and assign to Admin, Manager, and Owner roles

-- 1. Insert the permission if it doesn't exist
INSERT INTO "Permissions" ("Id", "Name", "Description", "Resource", "Action")
SELECT
    gen_random_uuid(),
    'invoices.manage',
    'Manage invoice SRI workflow (generate XML, sign, submit)',
    'invoices',
    'manage'
WHERE NOT EXISTS (
    SELECT 1 FROM "Permissions" WHERE "Name" = 'invoices.manage'
);

-- 2. Assign permission to all Admin roles across tenants
INSERT INTO "RolePermissions" ("Id", "RoleId", "PermissionId")
SELECT
    gen_random_uuid(),
    r."Id",
    p."Id"
FROM "Roles" r
CROSS JOIN "Permissions" p
WHERE r."Name" = 'Admin'
  AND p."Name" = 'invoices.manage'
  AND NOT EXISTS (
      SELECT 1
      FROM "RolePermissions" rp
      WHERE rp."RoleId" = r."Id"
        AND rp."PermissionId" = p."Id"
  );

-- 3. Assign permission to all Manager roles across tenants
INSERT INTO "RolePermissions" ("Id", "RoleId", "PermissionId")
SELECT
    gen_random_uuid(),
    r."Id",
    p."Id"
FROM "Roles" r
CROSS JOIN "Permissions" p
WHERE r."Name" = 'Manager'
  AND p."Name" = 'invoices.manage'
  AND NOT EXISTS (
      SELECT 1
      FROM "RolePermissions" rp
      WHERE rp."RoleId" = r."Id"
        AND rp."PermissionId" = p."Id"
  );

-- 4. Assign permission to all Owner roles across tenants
INSERT INTO "RolePermissions" ("Id", "RoleId", "PermissionId")
SELECT
    gen_random_uuid(),
    r."Id",
    p."Id"
FROM "Roles" r
CROSS JOIN "Permissions" p
WHERE r."Name" = 'Owner'
  AND p."Name" = 'invoices.manage'
  AND NOT EXISTS (
      SELECT 1
      FROM "RolePermissions" rp
      WHERE rp."RoleId" = r."Id"
        AND rp."PermissionId" = p."Id"
  );

-- Verify the permission was added
SELECT
    r."Name" as role_name,
    COUNT(*) as tenants_with_permission
FROM "Roles" r
JOIN "RolePermissions" rp ON r."Id" = rp."RoleId"
JOIN "Permissions" p ON rp."PermissionId" = p."Id"
WHERE p."Name" = 'invoices.manage'
GROUP BY r."Name"
ORDER BY r."Name";
