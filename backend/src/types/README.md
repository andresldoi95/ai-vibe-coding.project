# Types

This directory contains TypeScript type definitions and interfaces.

## Structure

Type definitions for the application:
- `index.ts` - Exported types
- `auth.types.ts` - Authentication related types
- `invoice.types.ts` - Invoice entity types
- `product.types.ts` - Product entity types
- `customer.types.ts` - Customer entity types
- `sri.types.ts` - SRI specific types
- `api.types.ts` - API request/response types
- `database.types.ts` - Database model types

## Guidelines

- Prefix interfaces with 'I' (e.g., IInvoice, IUser)
- Use types for unions and intersections
- Export all types from index.ts
- Include JSDoc comments for complex types
- Align with database schema from docs/database-schema.md
