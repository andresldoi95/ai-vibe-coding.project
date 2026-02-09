-- Verify stock levels after migration
SELECT
    p."Name",
    p."Code",
    p."CurrentStockLevel" as "OldField",
    COALESCE(SUM(wi."Quantity"), 0) as "ActualStock"
FROM "Products" p
LEFT JOIN "WarehouseInventory" wi
    ON wi."ProductId" = p."Id" AND wi."IsDeleted" = false
WHERE p."IsDeleted" = false
GROUP BY p."Id", p."Name", p."Code", p."CurrentStockLevel"
ORDER BY p."CreatedAt" DESC
LIMIT 5;
