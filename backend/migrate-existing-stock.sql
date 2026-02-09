-- Migration script to populate WarehouseInventory from existing Product.CurrentStockLevel
-- This creates initial inventory records for products that have stock discrepancies

DO $$
DECLARE
    product_record RECORD;
    stock_difference INT;
BEGIN
    -- Find products where CurrentStockLevel doesn't match WarehouseInventory totals
    FOR product_record IN
        SELECT
            p."Id" as product_id,
            p."TenantId" as tenant_id,
            p."Code" as product_code,
            p."CurrentStockLevel" as old_stock,
            COALESCE(SUM(wi."Quantity"), 0) as current_total,
            (p."CurrentStockLevel" - COALESCE(SUM(wi."Quantity"), 0)) as difference,
            (
                SELECT w."Id"
                FROM "Warehouses" w
                WHERE w."TenantId" = p."TenantId"
                    AND w."IsDeleted" = false
                ORDER BY w."CreatedAt" ASC
                LIMIT 1
            ) as warehouse_id
        FROM "Products" p
        LEFT JOIN "WarehouseInventory" wi ON wi."ProductId" = p."Id" AND wi."IsDeleted" = false
        WHERE p."IsDeleted" = false
            AND p."CurrentStockLevel" > 0
        GROUP BY p."Id", p."TenantId", p."Code", p."CurrentStockLevel"
        HAVING p."CurrentStockLevel" > COALESCE(SUM(wi."Quantity"), 0)
    LOOP
        stock_difference := product_record.difference;

        IF product_record.warehouse_id IS NULL THEN
            RAISE NOTICE 'Skipping product % - no warehouse found for tenant', product_record.product_code;
            CONTINUE;
        END IF;

        -- Check if inventory record exists for this product/warehouse combo
        IF EXISTS (
            SELECT 1 FROM "WarehouseInventory"
            WHERE "ProductId" = product_record.product_id
                AND "WarehouseId" = product_record.warehouse_id
                AND "IsDeleted" = false
        ) THEN
            -- Update existing record
            UPDATE "WarehouseInventory"
            SET
                "Quantity" = "Quantity" + stock_difference,
                "UpdatedAt" = NOW()
            WHERE "ProductId" = product_record.product_id
                AND "WarehouseId" = product_record.warehouse_id
                AND "IsDeleted" = false;

            RAISE NOTICE 'Updated product % - added % units to warehouse',
                product_record.product_code, stock_difference;
        ELSE
            -- Insert new inventory record
            INSERT INTO "WarehouseInventory" (
                "Id",
                "TenantId",
                "ProductId",
                "WarehouseId",
                "Quantity",
                "ReservedQuantity",
                "LastMovementDate",
                "CreatedAt",
                "UpdatedAt",
                "IsDeleted"
            ) VALUES (
                gen_random_uuid(),
                product_record.tenant_id,
                product_record.product_id,
                product_record.warehouse_id,
                stock_difference,
                0,
                NOW(),
                NOW(),
                NOW(),
                false
            );

            RAISE NOTICE 'Created inventory for product % with % units',
                product_record.product_code, stock_difference;
        END IF;
    END LOOP;
END $$;

-- Verify migration results
SELECT
    p."Name",
    p."Code",
    p."CurrentStockLevel" as "OldField",
    COALESCE(SUM(wi."Quantity"), 0) as "ActualStock",
    CASE
        WHEN p."CurrentStockLevel" = COALESCE(SUM(wi."Quantity"), 0) THEN '✓ OK'
        ELSE '✗ MISMATCH'
    END as "Status"
FROM "Products" p
LEFT JOIN "WarehouseInventory" wi ON wi."ProductId" = p."Id" AND wi."IsDeleted" = false
WHERE p."IsDeleted" = false
GROUP BY p."Id", p."Name", p."Code", p."CurrentStockLevel"
ORDER BY p."CreatedAt" DESC
LIMIT 10;
