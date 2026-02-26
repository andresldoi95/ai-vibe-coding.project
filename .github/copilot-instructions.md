# SaaS Billing + Inventory Management System — Copilot Instructions

## Pre-Generation Checklist

Before generating any code, **always**:

1. **Read `docs/COMMON_MISTAKES_AND_PATTERNS.md`** — catalogs recurring errors and established patterns
2. **Read the scoped instruction file** for the domain you are working on (see table below)
   - For any SRI feature: **always read `.github/instructions/sri.instructions.md`** first
3. **Read the relevant `docs/` file** for deep-dive reference
4. **Use `docs/WAREHOUSE_IMPLEMENTATION_REFERENCE.md`** as the canonical template for any new CRUD module
5. **Read `docs/i18n-standards.md`** before adding any translation keys
6. **Search `components/shared/`** before creating a new frontend component
7. **Update `backend/src/Api/Controllers/SeedController.cs`** after adding new entities or migrations
8. **Run `.\reset-demo-data.ps1`** after any database schema change to verify demo data compatibility

---

## Instruction Files (auto-applied by Copilot)

| File | Applies To | Covers |
|---|---|---|
| `.github/instructions/architecture.instructions.md` | `**` (always) | Clean Architecture layers, multi-tenancy, CQRS, security |
| `.github/instructions/sri.instructions.md` | `**` (always) | SRI XML schemas, access key, signing (XAdES-BES), SOAP, RIDE PDF, status machine |
| `.github/instructions/backend.instructions.md` | `backend/**` | .NET 8, EF Core, CQRS, Auth (JWT/RBAC/BCrypt), Email (MailKit) |
| `.github/instructions/frontend.instructions.md` | `frontend/**` | Nuxt 3, PrimeVue v4, Tailwind, `useApi()`, UX patterns |
| `.github/instructions/testing.instructions.md` | `backend/tests/**` | xUnit, FluentAssertions, Moq, AAA pattern |

---

## Key Reference Docs

| Doc | When to Read |
|---|---|
| `docs/WAREHOUSE_IMPLEMENTATION_REFERENCE.md` | Starting any new CRUD module (backend + frontend) |
| `docs/COMMON_MISTAKES_AND_PATTERNS.md` | Before any form, component, or CRUD implementation |
| `docs/i18n-standards.md` | Before adding any translation key |
| `docs/backend-agent.md` | Deep-dive backend patterns |
| `docs/frontend-agent.md` | Deep-dive frontend patterns |
| `docs/auth-agent.md` | Auth, JWT, RBAC, BCrypt patterns |
| `docs/email-agent.md` | MailKit, Mailpit, email templates |
| `docs/backend-unit-testing-agent.md` | xUnit test patterns, coverage checklists |
| `.github/instructions/sri.instructions.md` | Any SRI feature: XML generation, signing, SOAP, RIDE, new document type |

---

## Implementation Status

### ✅ Completed
- Authentication (JWT, refresh tokens, BCrypt)
- Multi-tenancy (schema-per-tenant, TenantId isolation)
- Inventory — Warehouses (full CRUD — use as reference)
- Backend Unit Tests — Warehouses (73 tests: 14 Domain + 59 Application)

### 📋 Pending
- Inventory — Products, Stock Movements, Suppliers + Purchase Orders
- Billing — Customers, Invoices, Payments, Subscriptions, Tax Rates

---

## Rebuild Commands

```powershell
.\rebuild-backend.ps1    # After any backend change
.\rebuild-frontend.ps1   # After any frontend change
.\reset-demo-data.ps1    # After any database schema change
```
