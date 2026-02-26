---
applyTo: "backend/**"
---

# Backend Development Instructions

You are an expert in .NET 8 backend development for the SaaS Billing + Inventory Management System.
Consolidates: **Backend Agent**, **Auth Agent**, **Email Agent**.

---

## Tech Stack

- **Framework**: .NET 8 ASP.NET Core Web API — Clean Architecture (Domain / Application / Infrastructure / API)
- **ORM**: Entity Framework Core 8+ / PostgreSQL 16
- **Pattern**: CQRS with MediatR + Repository + Unit of Work
- **Validation**: FluentValidation (pipeline behaviors)
- **Auth**: JWT Bearer tokens + refresh token rotation, BCrypt.NET password hashing
- **Authorization**: Policy-based RBAC (Admin > Manager > User)
- **Email**: MailKit (SMTP) — never `System.Net.Mail`; Mailpit on ports 1025/8025 for dev
- **Logging**: Serilog structured logging
- **Docs**: Swagger/OpenAPI with JWT support

---

## Architecture Rules

### Clean Architecture Layers
- **Domain**: Entities, value objects, domain events — no external dependencies
- **Application**: CQRS handlers, validators, interfaces, DTOs — depends only on Domain
- **Infrastructure**: EF Core, repositories, external services — implements Application interfaces
- **API**: Controllers, middleware, DI wiring — depends on Application + Infrastructure

### Always follow the Warehouse reference implementation
`docs/WAREHOUSE_IMPLEMENTATION_REFERENCE.md` is the canonical template for all CRUD modules.

---

## Entity Standards

All entities must have:
```csharp
public Guid Id { get; set; }
public Guid TenantId { get; set; }

// Audit fields
public DateTime CreatedAt { get; set; }
public DateTime UpdatedAt { get; set; }
public string CreatedBy { get; set; } = string.Empty;
public string UpdatedBy { get; set; } = string.Empty;

// Soft delete
public bool IsDeleted { get; set; }
public DateTime? DeletedAt { get; set; }
```

Unique constraints must be **scoped by TenantId** (e.g., `TenantId + Code` — never globally unique).

---

## CQRS Pattern

### Commands (Create / Update / Delete)
```
Application/Features/{Module}/Commands/Create{Entity}/
  Create{Entity}Command.cs
  Create{Entity}CommandHandler.cs
  Create{Entity}CommandValidator.cs
```

### Queries (GetAll / GetById)
```
Application/Features/{Module}/Queries/GetAll{Entity}/
  GetAll{Entity}Query.cs
  GetAll{Entity}QueryHandler.cs
```

- Always use `Result<T>` pattern for error handling — never throw exceptions from handlers
- Validate with FluentValidation pipeline behavior registered in DI
- Repository calls must pass `TenantId` from current user context

---

## EF Core Rules

- Global query filters for **soft delete** and **tenant isolation** on every entity DbSet
- Unique index example:
  ```csharp
  builder.HasIndex(e => new { e.TenantId, e.Code }).IsUnique();
  ```
- Every schema change requires a migration **and** a SeedController update (see Database Workflow)

---

## Database Change Workflow

**⚠️ Must be followed for every schema change:**

1. **Apply migration** (inside Docker container):
   ```bash
   docker exec -it saas-backend /bin/bash
   cd /src/src/Infrastructure
   dotnet ef migrations add MigrationName --startup-project ../Api --context ApplicationDbContext
   dotnet ef database update --startup-project ../Api --context ApplicationDbContext
   ```

2. **Update SeedController** (`backend/src/Api/Controllers/SeedController.cs`):
   - Add sample data for new entities (3-5 per tenant)
   - Cover all 3 demo tenants: Demo Company, Tech Startup, Manufacturing Corp

3. **Reset demo data** (from project root):
   ```powershell
   .\reset-demo-data.ps1
   ```

After code changes, always rebuild:
```powershell
.\rebuild-backend.ps1
```

---

## Auth Standards

- **JWT**: Bearer tokens with refresh token rotation — store refresh tokens in DB with expiry
- **Passwords**: BCrypt.NET only — never MD5, SHA1, or plain text
- **Tenant resolution**: Resolve `TenantId` from JWT claims or subdomain early in middleware
- **Data isolation**: Every repository query must filter by `TenantId` — enforce via global query filter
- **RBAC roles**: `Admin`, `Manager`, `User` — `Admin > Manager > User` hierarchy
- **Audit logging**: Log all security events (login attempts, auth failures, token refresh, role changes)
- **Policy-based authorization**: Register policies in DI; apply `[Authorize(Policy = "...")]` on controllers

---

## Email Standards (MailKit only)

- Implement `IEmailService` interface in Infrastructure layer
- Use Mailpit for dev/testing (ports 1025 SMTP / 8025 web UI)
- All sends must be **async** via background jobs — never block request threads
- Log every send attempt and failure
- HTML templates must be responsive; include unsubscribe link where legally required

**Email types**: Welcome, password reset, invoice sent, payment confirmation, stock alert, system notification

---

## Controller Standards

- RESTful endpoints with Swagger `[ProducesResponseType]` annotations
- Authorize with `[Authorize]` + policy guards for tenant isolation
- Return `IActionResult` with appropriate HTTP status codes
- Route prefix: `api/v1/{resource}`

---

## Code Quality

- Follow SOLID principles — single responsibility per class
- Use `Result<T>` to return Success/Failure — callers check, never catch exceptions from business logic
- No business logic in controllers — only orchestration
- Inject via constructor — no service locator
- Never use `var` for ambiguous types; prefer explicit types for readability
