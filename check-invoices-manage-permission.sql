-- Check if invoices.manage permission exists and is assigned to roles
SELECT
    r."Name" as role_name,
    p."Resource",
    p."Action"
FROM "RolePermissions" rp
JOIN "Roles" r ON rp."RoleId" = r."Id"
JOIN "Permissions" p ON rp."PermissionId" = p."Id"
WHERE p."Resource" = 'invoices'
  AND r."Name" IN ('Owner', 'Admin', 'Manager')
ORDER BY r."Name", p."Action";
