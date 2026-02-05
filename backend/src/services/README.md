# Services

This directory contains business logic and application services.

## Structure

Services handle complex business operations:
- `authService.ts` - Authentication logic (JWT, password hashing)
- `companyService.ts` - Company operations
- `invoiceService.ts` - Invoice creation, calculation, validation
- `productService.ts` - Product management
- `inventoryService.ts` - Stock management, transactions
- `sriService.ts` - SRI integration (XML generation, authorization)
- `pdfService.ts` - RIDE PDF generation
- `digitalSignatureService.ts` - Firma electrónica operations
- `sequenceService.ts` - Document sequential numbering

## Guidelines

- Keep services focused on single responsibility
- Services can call other services
- Don't access req/res objects directly
- Return data or throw errors
- Implement transaction management for database operations
- Log important operations for audit trail
