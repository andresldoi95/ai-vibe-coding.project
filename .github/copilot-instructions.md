# Copilot Instructions - Billing & Inventory System (Ecuador)

## Project Overview
This is a billing and inventory management system designed for Ecuador, with full SRI (Servicio de Rentas Internas) electronic invoicing compliance.

## Tech Stack
- **Backend**: Node.js + Express
- **Database**: PostgreSQL
- **Cache/Queue**: Redis
- **Frontend**: Vue.js
- **Desktop**: Electron

## Ecuador-Specific Requirements

### SRI Electronic Invoicing Compliance
- All invoices must be electronically signed with "firma electrónica"
- Generate XML documents in SRI format (factura electrónica)
- Create RIDE (Representación Impresa del Documento Electrónico) as PDF
- Integrate with SRI web services for authorization
- Support offline mode - store invoices locally when internet is unavailable
- Implement retry logic for SRI authorization

### Document Types to Support
- Facturas (Invoices)
- Notas de crédito (Credit notes)
- Notas de débito (Debit notes)
- Guías de remisión (Delivery notes)
- Comprobantes de retención (Withholding receipts)

### Required Data Fields
- RUC (tax ID)
- Ambiente (1=pruebas, 2=producción)
- Tipo de emisión (1=normal, 2=contingencia)
- Clave de acceso (48-digit access key)
- Número de autorización from SRI

## Coding Conventions

### General
- Use TypeScript for type safety
- Follow clean architecture principles
- Implement comprehensive error handling
- Add logging for all SRI interactions
- Include unit tests for business logic

### Naming
- Use camelCase for variables and functions
- Use PascalCase for classes and components
- Prefix interfaces with `I` (e.g., `IInvoice`)
- Use descriptive names for SRI-related functions

### Styling
- **Use Tailwind CSS exclusively** - NO scoped CSS in components
- Define custom utilities in `tailwind.config.js` when needed
- Use Tailwind's built-in classes for all styling
- Only use CSS in `main.css` for global styles and PrimeVue overrides
- Prefer utility classes over custom CSS classes
- Use Tailwind's responsive modifiers (sm:, md:, lg:, xl:)
- Leverage Tailwind's state variants (hover:, focus:, active:)

### Security
- Never log sensitive data (RUC, firma electrónica credentials)
- Encrypt database backups
- Implement role-based access control (RBAC)
- Validate all user inputs
- Secure storage for digital certificates

### Database
- Use migrations for schema changes
- Create indexes for frequently queried fields
- Implement soft deletes for audit trails
- Store all monetary values as DECIMAL(10,2)
- Keep audit logs for all invoice operations

### API Design
- RESTful endpoints
- Consistent error responses
- Paginate list endpoints
- Include request validation
- Document with OpenAPI/Swagger

## Important Business Rules

### Inventory
- Track stock in real-time
- Support multiple warehouses
- Implement low-stock alerts
- Use FIFO or weighted average cost methods
- Support serial numbers and batch tracking

### Billing
- Sequential invoice numbering (001-001-000000001 format)
- Support multiple payment methods
- Handle partial payments
- Calculate taxes automatically (IVA 12%, IVA 0%, No IVA)
- Support discounts at item and invoice level

### Reports Required
- Sales by period
- Inventory valuation
- Tax reports (IVA, retenciones)
- Best-selling products
- Customer statements
- Daily cash register closing

## Performance
- Cache frequently accessed data (products, prices)
- Optimize queries with proper indexing
- Implement pagination for large datasets
- Use connection pooling for database
- Async processing for invoice generation

## Offline Support
- Local SQLite for offline POS operations
- Sync mechanism when connection restored
- Queue invoices for SRI authorization
- Store RIDE PDFs locally

## Testing Requirements
- Unit tests for business logic
- Integration tests for SRI communication
- E2E tests for critical flows (invoice creation)
- Mock SRI responses for testing
- Test both ambiente pruebas and producción

## Deployment
- Use Docker for containerization
- Environment-specific configurations
- Automated database backups
- Zero-downtime deployments
- Health check endpoints

## When Helping Me
- Prioritize SRI compliance in all billing features
- Consider offline scenarios
- Suggest error handling for network issues
- Remind me about audit trail requirements
- Flag potential tax compliance issues
- Recommend scalable solutions
- Keep Ecuador-specific regulations in mind
