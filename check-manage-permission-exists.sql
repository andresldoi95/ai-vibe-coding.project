-- Check if invoices.manage permission exists
SELECT * FROM "Permissions" WHERE "Resource" = 'invoices' AND "Action" = 'manage';
