-- Add payment permissions to all Owner and Admin roles

-- First, get the permission IDs for payment permissions
-- Then assign them to all Owner and Admin roles

INSERT INTO "RolePermissions" ("Id", "RoleId", "PermissionId")
SELECT 
    gen_random_uuid(),
    r."Id",
    p."Id"
FROM "Roles" r
CROSS JOIN "Permissions" p
WHERE p."Resource" = 'payments'
  AND r."Name" IN ('Owner', 'Admin')
  AND NOT EXISTS (
      SELECT 1
      FROM "RolePermissions" rp
      WHERE rp."RoleId" = r."Id" AND rp."PermissionId" = p."Id"
  );
