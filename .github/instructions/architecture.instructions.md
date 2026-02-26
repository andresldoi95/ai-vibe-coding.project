---
applyTo: "**"
---

# Architecture Instructions

You are an expert in designing the architecture for the SaaS Billing + Inventory Management System.
Consolidates: **Project Architecture Agent**.

These are cross-cutting constraints that apply to all layers of the application.

---

## System Overview

| Layer | Technology |
|---|---|
| Backend | .NET 8 ASP.NET Core Web API — Clean Architecture |
| Database | PostgreSQL 16 — schema-per-tenant multi-tenancy |
| Frontend | Nuxt 3 + TypeScript + PrimeVue v4 (Teal) + Tailwind CSS |
| Auth | JWT Bearer + refresh token rotation |
| Background Jobs | Hangfire / Quartz.NET |
| Logging | Serilog structured logging |
| Deployment | Docker / Docker Compose (containerized) |

---

## Clean Architecture Layers

```
Domain       ← no dependencies on outer layers
Application  ← depends on Domain only (CQRS handlers, validators, interfaces)
Infrastructure ← implements Application interfaces (EF Core, SMTP, external services)
API          ← HTTP concern only (controllers, middleware, DI wiring)
```

**Rules:**
- Inner layers never reference outer layers
- Application defines interfaces; Infrastructure implements them
- Domain has zero framework dependencies
- All cross-cutting concerns (logging, validation) via pipeline behaviors — not scattered in handlers

---

## Multi-Tenant Architecture (Schema-per-Tenant)

- **Tenant identification**: HTTP header `X-Tenant-Id`, JWT claim, or subdomain — resolved early in middleware pipeline
- **Data isolation**: Every entity has `TenantId` (Guid) column; global EF query filters enforce isolation automatically
- **Schema-per-tenant**: Each tenant gets its own PostgreSQL schema — strongest isolation boundary
- **Onboarding**: Tenant provisioning creates schema + runs migrations + seeds default data
- **Security boundary**: Tenant context resolved from authenticated token only — never from user-supplied query params

---

## CQRS with MediatR

- **Commands** mutate state: `CreateXCommand`, `UpdateXCommand`, `DeleteXCommand`
- **Queries** read state: `GetAllXQuery`, `GetXByIdQuery`
- **Handlers** are the only classes that execute business logic — no business logic in controllers
- **Validators**: FluentValidation registered as pipeline behavior — auto-runs before every command
- **Result pattern**: All handlers return `Result<T>` (Success / Failure) — never throw exceptions for business errors

---

## Database Design Standards

- All entities have audit fields: `CreatedAt`, `UpdatedAt`, `CreatedBy`, `UpdatedBy`
- All entities support soft delete: `IsDeleted` (bool), `DeletedAt` (DateTime?)
- Every unique constraint is **scoped by TenantId** — never globally unique alone
- Indexes: All foreign keys indexed; composite indexes include `TenantId` first
- Migrations: EF Core only — never raw SQL schema changes outside migrations

---

## Security Architecture

### Authentication
- JWT Bearer tokens with short expiry (15 min)
- Refresh token rotation stored in DB with expiry + device fingerprint
- BCrypt for password hashing — never MD5/SHA1/plain text

### Authorization
- Policy-based RBAC: `Admin > Manager > User` hierarchy
- Policies enforced at controller level with `[Authorize(Policy = "...")]`
- All tenant data access additionally guarded by `TenantId` check in handlers

### API Security
- Rate limiting on auth endpoints
- CORS configured per environment
- HTTPS enforced in production
- No sensitive data in query strings or logs

---

## Frontend Architecture Standards

- **Nuxt 3 auto-routing**: File-based routing in `pages/`
- **Auth middleware**: `middleware/auth.ts` — guards all authenticated routes
- **Tenant middleware**: `middleware/tenant.ts` — ensures tenant context on every navigation
- **API plugin**: `plugins/api.ts` — injects `useApi()` with auth + tenant headers globally
- **Pinia stores**: `auth` (persisted), `tenant` (persisted), `ui` (session-level breadcrumbs/sidebar)
- **Composables**: One composable per domain entity following `useWarehouse` pattern

---

## Module Structure (Billing + Inventory)

### Inventory Module
- Warehouses ✅ (complete — use as reference)
- Products (catalog, SKU, barcode, pricing)
- Stock Movements (in/out/transfer, FIFO/LIFO/Average valuation)
- Suppliers + Purchase Orders

### Billing Module
- Customers
- Invoices (draft → sent → paid → overdue lifecycle)
- Payments (gateway integration — Stripe/PayPal)
- Subscriptions + Pricing Plans
- Tax Rates + Compliance

---

## Integration Architecture

- **Webhooks**: Outbound events for third-party integrations — async via background jobs
- **Email**: MailKit → background job queue → delivery log
- **Background jobs**: Hangfire — recurring billing, stock alerts, dunning logic, email queue
- **Reporting**: Query-side read models — never aggregate from OLTP tables at request time

---

## Deployment (Docker)

- `docker-compose.yml` — development (with Mailpit, hot reload)
- `docker-compose.prod.yml` — production (hardened, no dev tools)
- Rebuild after backend changes: `.\rebuild-backend.ps1`
- Rebuild after frontend changes: `.\rebuild-frontend.ps1`
- Reset demo data (runs inside container): `.\reset-demo-data.ps1`

---

## Reference Implementation

**Always start from the Warehouse module when building a new CRUD feature.**
`docs/WAREHOUSE_IMPLEMENTATION_REFERENCE.md` contains the full end-to-end pattern:
- Backend: entity → EF config → migration → CQRS → repository → controller → seed data
- Frontend: types → composable → pages (list/create/edit/view) → i18n translations
