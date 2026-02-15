-- Check all permissions
SELECT "Resource", COUNT(*) as count FROM "Permissions" GROUP BY "Resource";
