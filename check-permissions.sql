-- Check payment permissions by role
SELECT r."Name" as role, p."Name" as permission
FROM "RolePermissions" rp
JOIN "Roles" r ON rp."RoleId" = r."Id"
JOIN "Permissions" p ON rp."PermissionId" = p."Id"
WHERE p."Resource" = 'payments'
ORDER BY r."Name", p."Action";
