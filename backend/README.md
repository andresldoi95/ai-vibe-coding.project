# SaaS Backend API

## Overview

Multi-tenant SaaS backend built with .NET 8, featuring:
- **Clean Architecture** with CQRS pattern using MediatR
- **Multi-tenant** support with schema-per-tenant isolation
- **JWT Authentication** with refresh tokens
- **PostgreSQL** database with Entity Framework Core
- **FluentValidation** for request validation
- **Serilog** for structured logging
- **Swagger/OpenAPI** documentation

## Architecture

```
backend/
├── src/
│   ├── Api/                 # ASP.NET Core Web API
│   │   ├── Controllers/     # API endpoints
│   │   ├── Middleware/      # Custom middleware
│   │   └── Program.cs       # Application entry point
│   ├── Application/         # Business logic & CQRS
│   │   ├── Features/        # Commands & Queries
│   │   ├── DTOs/            # Data Transfer Objects
│   │   ├── Behaviors/       # MediatR pipeline behaviors
│   │   └── Interfaces/      # Abstractions
│   ├── Domain/              # Domain entities & enums
│   │   ├── Entities/        # Database entities
│   │   ├── Enums/           # Enumerations
│   │   └── Common/          # Base classes
│   └── Infrastructure/      # Data access & external services
│       ├── Persistence/     # EF Core, DbContext, Repositories
│       └── Services/        # Authentication, external APIs
```

## Getting Started

### Prerequisites

- Docker & Docker Compose (no .NET SDK needed locally)
- PostgreSQL client (optional, for database inspection)

### Quick Start

1. **Build and run with Docker Compose:**
```bash
docker-compose up --build
```

2. **Access the API:**
- Swagger UI: http://localhost:5000/swagger
- API Base URL: http://localhost:5000/api/v1

3. **Test the endpoints:**
   - Register a new company: `POST /api/v1/auth/register`
   - Login: `POST /api/v1/auth/login`
   - Get user tenants: `GET /api/v1/tenants`

### Database Migrations

The application automatically runs migrations on startup. To create new migrations:

```bash
# Access the backend container
docker exec -it saas-backend /bin/bash

# Navigate to Infrastructure project
cd /src/src/Infrastructure

# Create a new migration
dotnet ef migrations add MigrationName --startup-project ../Api --context ApplicationDbContext

# Apply migration
dotnet ef database update --startup-project ../Api --context ApplicationDbContext
```

### Database Change Workflow

**⚠️ CRITICAL**: When making schema changes, follow this checklist to keep demo data synchronized:

**1. Apply Schema Changes**
   - Create/modify entities in `src/Domain/Entities/`
   - Update EF configurations in `src/Infrastructure/Persistence/Configurations/`
   - Generate and apply migration (see above)

**2. Update Seed Data**
   - **Always update `src/Api/Controllers/SeedController.cs`**
   - Add methods to create sample data for new entities
   - Follow existing patterns (3-5 samples per tenant)
   - Update `SeedDemoData()` method to include new entities

**3. Reset Demo Database**
   - Run from project root: `./reset-demo-data.ps1 -Force`
   - This drops/recreates DB, applies migrations, seeds all data
   - Verify demo users can login and see sample data

**Why this matters**: Forgetting step 2 causes broken demo data, test failures, and inconsistent development environments. The reset script calls `/api/seed/demo` which relies on SeedController being up-to-date.

**Note**: The reset script must run from the host machine (or inside a container with docker-compose access) because it uses `docker-compose exec` to connect to the database.

## API Endpoints

### Authentication

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| POST | `/api/v1/auth/register` | Register company + admin user | No |
| POST | `/api/v1/auth/login` | Login with email/password | No |
| POST | `/api/v1/auth/refresh` | Refresh access token | No |
| GET | `/api/v1/auth/me` | Get current user | Yes |

### Tenants

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| GET | `/api/v1/tenants` | Get user's tenants | Yes |
| GET | `/api/v1/tenants/{id}` | Get specific tenant | Yes |

## Registration Flow

**Multi-step registration creates:**
1. **Tenant (Company)** - New database schema
2. **User** - Admin account with hashed password
3. **UserTenant** - Relationship with Owner role
4. **Tokens** - JWT access token + refresh token

**Example Request:**
```json
{
  "companyName": "Acme Corp",
  "slug": "acme-corp",
  "email": "admin@acme.com",
  "password": "SecurePass123",
  "name": "John Doe"
}
```

**Response:**
```json
{
  "data": {
    "accessToken": "eyJhbGci...",
    "refreshToken": "base64...",
    "user": {
      "id": "guid",
      "email": "admin@acme.com",
      "name": "John Doe",
      "isActive": true,
      "emailConfirmed": false
    },
    "tenants": [{
      "id": "guid",
      "name": "Acme Corp",
      "slug": "acme-corp",
      "status": "Active"
    }]
  },
  "message": "Registration successful",
  "success": true
}
```

## Multi-Tenancy

### How It Works

1. **Schema Isolation**: Each tenant gets a separate PostgreSQL schema
2. **Tenant Resolution**: `X-Tenant-Id` header identifies the tenant
3. **Access Control**: User-tenant relationships validated via `UserTenant` table
4. **Context Injection**: `ITenantContext` provides tenant info to all requests

### Tenant Header

All authenticated requests (except auth endpoints) require:
```
X-Tenant-Id: <tenant-guid>
```

If user lacks access to the tenant:
- **403 Forbidden** response

## Environment Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `ConnectionStrings__DefaultConnection` | PostgreSQL connection | `Host=db;Port=5432;Database=saas;Username=postgres;Password=postgres` |
| `JWT__Secret` | JWT signing key (32+ chars) | *See appsettings.json* |
| `JWT__Issuer` | JWT issuer claim | `SaaS.Backend` |
| `JWT__Audience` | JWT audience claim | `SaaS.Frontend` |
| `JWT__ExpirationMinutes` | Access token lifetime | `15` |

## Development

### Project Structure Decisions

- **Clean Architecture**: Separation of concerns, testable, maintainable
- **CQRS with MediatR**: Clear separation of reads/writes
- **Repository + UnitOfWork**: Abstraction over data access
- **Schema-per-tenant**: Strong isolation, easier backup/restore per customer
- **BCrypt password hashing**: Industry standard with automatic salting

### Adding New Features

1. **Create Command/Query** in `Application/Features/`
2. **Add Validator** using FluentValidation
3. **Implement Handler** with business logic
4. **Create Controller endpoint** in `Api/Controllers/`
5. **Update DTOs** if needed

### Testing with Swagger

1. Start the application
2. Navigate to http://localhost:5000/swagger
3. Register a new company
4. Copy the `accessToken` from response
5. Click "Authorize" button
6. Enter: `Bearer <your-token>`
7. Test protected endpoints

## Database Schema

### Core Tables

**Users**
- Id (PK)
- Email (unique)
- PasswordHash
- Name
- IsActive
- EmailConfirmed
- Audit fields (CreatedAt, UpdatedAt, etc.)

**Tenants**
- Id (PK)
- Name
- Slug (unique)
- Status (Active/Suspended/Cancelled/Trial)
- SchemaName

**UserTenants** (Join Table)
- Id (PK)
- UserId (FK)
- TenantId (FK)
- Role (Owner/Admin/Manager/User)
- IsActive

**RefreshTokens**
- Id (PK)
- UserId (FK)
- Token (unique)
- ExpiresAt
- RevokedAt (nullable)

## Security

- **Password Hashing**: BCrypt with cost factor 12
- **JWT Tokens**: 15-minute expiration
- **Refresh Tokens**: 7-day expiration
- **Token Rotation**: Old refresh token revoked on renewal
- **CORS**: Configured for frontend (http://localhost:3000)
- **Validation**: All inputs validated before processing

## Troubleshooting

### Database Connection Issues

```bash
# Check if database is running
docker ps | grep postgres

# View database logs
docker logs saas-postgres

# Connect to database
docker exec -it saas-postgres psql -U postgres -d saas
```

### View Backend Logs

```bash
# Real-time logs
docker logs -f saas-backend

# Check log files
docker exec -it saas-backend ls /app/Logs
```

### Reset Database

```bash
# Stop containers
docker-compose down

# Remove volumes
docker volume rm ai-vibe-coding.project_postgres_data

# Rebuild
docker-compose up --build
```

## Production Deployment

Use `docker-compose.prod.yml` for production:

```bash
docker-compose -f docker-compose.prod.yml up -d
```

**Production Checklist:**
- [ ] Change JWT secret to strong random value
- [ ] Update CORS origins to production domain
- [ ] Configure proper PostgreSQL credentials
- [ ] Enable HTTPS
- [ ] Set up database backups
- [ ] Configure monitoring and alerting
- [ ] Review and adjust log levels

## License

MIT
