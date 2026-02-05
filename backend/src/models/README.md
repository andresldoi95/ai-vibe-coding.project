# Models

This directory contains data access layer (repositories/models).

## Structure

Models handle database operations:
- `companyModel.ts` - Company CRUD operations
- `userModel.ts` - User CRUD operations
- `invoiceModel.ts` - Invoice database operations
- `productModel.ts` - Product database operations
- `inventoryModel.ts` - Inventory operations
- `customerModel.ts` - Customer CRUD operations
- `taxTypeModel.ts` - Tax configuration
- `auditLogModel.ts` - Audit trail operations

## Guidelines

- Use prepared statements to prevent SQL injection
- Return plain objects or arrays
- Don't include business logic
- Use transactions for multi-table operations
- Include proper error handling
- Create reusable query builders
- Consider using Query Builder or ORM (TypeORM, Prisma) if needed
