# Backend Agent

## Specialization
Expert in .NET 8 backend development with Entity Framework Core, CQRS patterns, API design with Swagger, authorization guards, and SOLID principles for the SaaS Billing + Inventory Management System.

## Tech Stack
- **Framework**: .NET 8 (ASP.NET Core Web API)
- **ORM**: Entity Framework Core 8+
- **Database**: PostgreSQL 16
- **Patterns**: CQRS with MediatR
- **Documentation**: Swagger/OpenAPI (Swashbuckle)
- **Validation**: FluentValidation
- **Authentication**: JWT Bearer Tokens
- **Password Hashing**: BCrypt.NET
- **Logging**: Serilog
- **Authorization**: Policy-based authorization (planned)

## Implementation Status

### ✅ Completed Features
- Clean Architecture (4-layer structure: Domain, Application, Infrastructure, API)
- Multi-tenant architecture with schema-per-tenant support
- JWT authentication with refresh tokens
- CQRS pattern with MediatR
- Repository + Unit of Work pattern
- FluentValidation with pipeline behaviors
- Global exception handling middleware
- Tenant resolution middleware
- Serilog structured logging
- Swagger/OpenAPI documentation with JWT support
- BCrypt password hashing
- Entity Framework Core with PostgreSQL
- Result pattern for error handling
- Audit fields (CreatedAt, UpdatedAt, CreatedBy, UpdatedBy)
- Soft delete support (IsDeleted, DeletedAt)

### 📋 Planned Features
- Advanced authorization policies
- Billing module entities and features
- Inventory module entities and features
- Advanced query filters and specifications
- Caching (Redis)
- Background jobs
- Email notifications
- File upload support

## Core Responsibilities

### 1. Project Structure & Organization

#### Current Implementation
```
backend/
├── src/
│   ├── Domain/                          # Core business entities (no dependencies)
│   │   ├── Common/
│   │   │   ├── BaseEntity.cs           # Base class with Id and audit fields
│   │   │   ├── AuditableEntity.cs      # Adds CreatedAt, UpdatedAt, soft delete
│   │   │   └── TenantEntity.cs         # Base for tenant-scoped entities
│   │   ├── Entities/
│   │   │   ├── User.cs                 # User account
│   │   │   ├── Tenant.cs               # Company/organization
│   │   │   ├── UserTenant.cs           # Many-to-many with role
│   │   │   └── RefreshToken.cs         # JWT refresh tokens
│   │   └── Enums/
│   │       ├── UserRole.cs             # Owner, Admin, User
│   │       └── TenantStatus.cs         # Active, Suspended, Trial
│   │
│   ├── Application/                     # Business logic & orchestration
│   │   ├── Common/
│   │   │   ├── Behaviors/
│   │   │   │   ├── ValidationBehavior.cs           # FluentValidation pipeline
│   │   │   │   ├── LoggingBehavior.cs              # Request/response logging
│   │   │   │   └── UnhandledExceptionBehavior.cs   # Global exception handling
│   │   │   ├── Interfaces/
│   │   │   │   ├── IAuthService.cs                 # Auth & token generation
│   │   │   │   ├── ITenantContext.cs               # Current tenant info
│   │   │   │   ├── IApplicationDbContext.cs        # EF DbContext abstraction
│   │   │   │   ├── IRepository.cs                  # Generic repository
│   │   │   │   ├── IUserRepository.cs
│   │   │   │   ├── ITenantRepository.cs
│   │   │   │   ├── IUserTenantRepository.cs
│   │   │   │   ├── IRefreshTokenRepository.cs
│   │   │   │   └── IUnitOfWork.cs                  # Transaction coordination
│   │   │   └── Models/
│   │   │       ├── Result.cs                       # Success/failure wrapper
│   │   │       ├── ApiResponse.cs                  # Standardized response
│   │   │       └── PaginatedResponse.cs            # Pagination support
│   │   ├── DTOs/
│   │   │   ├── UserDto.cs
│   │   │   ├── TenantDto.cs
│   │   │   └── LoginResponseDto.cs
│   │   └── Features/                    # CQRS - organized by feature
│   │       ├── Auth/
│   │       │   ├── Commands/
│   │       │   │   ├── Register/
│   │       │   │   │   ├── RegisterCommand.cs
│   │       │   │   │   ├── RegisterCommandValidator.cs
│   │       │   │   │   └── RegisterCommandHandler.cs
│   │       │   │   ├── Login/
│   │       │   │   │   ├── LoginCommand.cs
│   │       │   │   │   ├── LoginCommandValidator.cs
│   │       │   │   │   └── LoginCommandHandler.cs
│   │       │   │   └── RefreshToken/
│   │       │   │       ├── RefreshTokenCommand.cs
│   │       │   │       └── RefreshTokenCommandHandler.cs
│   │       │   └── Queries/
│   │       │       └── GetCurrentUser/
│   │       │           ├── GetCurrentUserQuery.cs
│   │       │           └── GetCurrentUserQueryHandler.cs
│   │       └── Tenants/
│   │           └── Queries/
│   │               └── GetUserTenants/
│   │                   ├── GetUserTenantsQuery.cs
│   │                   └── GetUserTenantsQueryHandler.cs
│   │
│   ├── Infrastructure/                  # External services & data access
│   │   ├── Persistence/
│   │   │   ├── ApplicationDbContext.cs             # EF Core DbContext
│   │   │   ├── TenantContext.cs                    # Tenant resolution
│   │   │   ├── Configurations/
│   │   │   │   ├── UserConfiguration.cs            # Fluent API for User
│   │   │   │   ├── TenantConfiguration.cs          # Fluent API for Tenant
│   │   │   │   ├── UserTenantConfiguration.cs      # Fluent API for UserTenant
│   │   │   │   └── RefreshTokenConfiguration.cs    # Fluent API for RefreshToken
│   │   │   ├── Repositories/
│   │   │   │   ├── Repository.cs                   # Generic base
│   │   │   │   ├── UserRepository.cs
│   │   │   │   ├── TenantRepository.cs
│   │   │   │   ├── UserTenantRepository.cs
│   │   │   │   ├── RefreshTokenRepository.cs
│   │   │   │   └── UnitOfWork.cs
│   │   │   └── Migrations/
│   │   │       └── 20260206000000_InitialCreate.cs
│   │   └── Services/
│   │       └── AuthService.cs                      # JWT & password hashing
│   │
│   └── Api/                             # HTTP endpoints & middleware
│       ├── Controllers/
│       │   ├── BaseController.cs                   # Shared controller logic
│       │   ├── AuthController.cs                   # /api/v1/auth
│       │   └── TenantsController.cs                # /api/v1/tenants
│       ├── Middleware/
│       │   ├── ExceptionHandlingMiddleware.cs      # Global error handling
│       │   └── TenantResolutionMiddleware.cs       # X-Tenant-Id validation
│       ├── Program.cs                              # App configuration
│       ├── appsettings.json                        # Configuration
│       └── appsettings.Development.json
```

#### Architecture Principles
- **Dependency Flow**: API → Application → Domain ← Infrastructure
- **Domain** has no dependencies
- **Application** references Domain only
- **Infrastructure** implements Application interfaces
- **API** orchestrates via dependency injection


### 2. Entity Framework Core & Database Models

#### Current DbContext Implementation
```csharp
public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    // DbSets
    public DbSet<User> Users => Set<User>();
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<UserTenant> UserTenants => Set<UserTenant>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    // Features:
    // - Automatic audit field updates (CreatedAt, UpdatedAt)
    // - Global query filters for soft deletes
    // - Fluent API configurations via IEntityTypeConfiguration
    // - Multi-tenant context awareness
}
```

#### Implemented Entities

**BaseEntity** (Domain/Common/BaseEntity.cs)
```csharp
public abstract class BaseEntity
{
    public Guid Id { get; set; }
}
```

**AuditableEntity** (Domain/Common/AuditableEntity.cs)
```csharp
public abstract class AuditableEntity : BaseEntity
{
    public DateTime CreatedAt { get; set; }       // Auto-set on insert
    public string? CreatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }       // Auto-set on update
    public string? UpdatedBy { get; set; }
    public DateTime? DeletedAt { get; set; }
    public bool IsDeleted { get; set; } = false;  // Soft delete flag
}
```

**User Entity**
- Email (unique, required)
- PasswordHash (BCrypt with cost 12)
- Name
- IsActive
- EmailConfirmed
- Navigation: UserTenants, RefreshTokens

**Tenant Entity**
- Name (company name)
- Slug (unique, URL-friendly identifier)
- Status (Active, Suspended, Trial)
- ConnectionString (for schema-per-tenant)
- SchemaName
- Navigation: UserTenants

**UserTenant Entity** (Many-to-Many + Role)
- UserId (FK to Users)
- TenantId (FK to Tenants)
- Role (Owner, Admin, User)
- Composite PK: (UserId, TenantId)

**RefreshToken Entity**
- UserId (FK to Users)
- Token (unique)
- ExpiresAt
- CreatedAt
- CreatedByIp
- RevokedAt
- RevokedByIp
- ReplacedByToken

#### Entity Configurations (Fluent API)
All entity configurations use `IEntityTypeConfiguration<T>`:
- Table names: PascalCase, plural (Users, Tenants)
- Column types: `decimal(18,2)` for money, `varchar(500)` for strings
- Indexes: Unique on Email, Slug; composite on (UserId, TenantId)
- Foreign keys: Cascade delete on UserTenants
- Required properties marked with `.IsRequired()`

#### Migrations
- Initial migration: `20260206000000_InitialCreate.cs`
- PostgreSQL provider with `UseNpgsql()`
- Ready for deployment with `dotnet ef database update`


### 3. CQRS Implementation with MediatR

#### Current Implementation Structure
```
Features/
├── Auth/
│   ├── Commands/
│   │   ├── Register/
│   │   │   ├── RegisterCommand.cs              # Request DTO
│   │   │   ├── RegisterCommandValidator.cs     # FluentValidation rules
│   │   │   └── RegisterCommandHandler.cs       # Business logic
│   │   ├── Login/
│   │   │   ├── LoginCommand.cs
│   │   │   ├── LoginCommandValidator.cs
│   │   │   └── LoginCommandHandler.cs
│   │   └── RefreshToken/
│   │       ├── RefreshTokenCommand.cs
│   │       └── RefreshTokenCommandHandler.cs
│   └── Queries/
│       └── GetCurrentUser/
│           ├── GetCurrentUserQuery.cs
│           └── GetCurrentUserQueryHandler.cs
└── Tenants/
    └── Queries/
        └── GetUserTenants/
            ├── GetUserTenantsQuery.cs
            └── GetUserTenantsQueryHandler.cs
```

#### Command Pattern Implementation

**Example: RegisterCommand**
```csharp
public class RegisterCommand : IRequest<Result<LoginResponseDto>>
{
    public string CompanyName { get; set; }
    public string Slug { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public string Name { get; set; }
}

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).MinimumLength(8);
        RuleFor(x => x.CompanyName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Slug).NotEmpty().Matches("^[a-z0-9-]+$");
    }
}

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<LoginResponseDto>>
{
    // Constructor injection: IUnitOfWork, IAuthService, ILogger

    // Handle method:
    // 1. Check if email/slug exists
    // 2. Create Tenant
    // 3. Create User with hashed password
    // 4. Create UserTenant relationship (Owner role)
    // 5. Generate JWT tokens
    // 6. SaveChangesAsync (atomic transaction)
    // 7. Return Result.Success or Result.Failure
}
```

#### Query Pattern Implementation

**Example: GetUserTenantsQuery**
```csharp
public class GetUserTenantsQuery : IRequest<Result<List<TenantDto>>>
{
    public Guid UserId { get; set; }
}

public class GetUserTenantsQueryHandler : IRequestHandler<GetUserTenantsQuery, Result<List<TenantDto>>>
{
    // Uses AsNoTracking() for read-only queries
    // Projects directly to DTOs
    // Filters by UserId
    // Returns tenant list with role information
}
```

#### Pipeline Behaviors (Registered in Program.cs)

1. **UnhandledExceptionBehavior** - Catches all exceptions
2. **LoggingBehavior** - Logs request/response with timing
3. **ValidationBehavior** - Auto-validates using FluentValidation

**Order of execution**:
Request → UnhandledExceptionBehavior → LoggingBehavior → ValidationBehavior → Handler

#### Result Pattern
```csharp
public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? Error { get; }
    public List<string> Errors { get; }

    public static Result<T> Success(T value);
    public static Result<T> Failure(string error);
    public static Result<T> Failure(List<string> errors);
}
```

**Usage in handlers**:
```csharp
if (existingUser != null)
    return Result<LoginResponseDto>.Failure("Email already registered");

return Result<LoginResponseDto>.Success(response);
```

### 4. SOLID Principles Implementation

#### Single Responsibility Principle (SRP)
- Each class has one reason to change
- Separate command handlers from query handlers
- Split large controllers into feature-based controllers
- Create specific validators per command/query

#### Open/Closed Principle (OCP)
- Use strategy pattern for payment gateways
- Implement plugin architecture for tenant-specific features
- Create extensible pipeline behaviors
- Use specification pattern for flexible queries

#### Liskov Substitution Principle (LSP)
- Properly implement base entity inheritance
- Ensure derived classes don't break base class contracts
- Use abstract classes and interfaces correctly

#### Interface Segregation Principle (ISP)
- Create focused interfaces (IRepository<T>, IUnitOfWork)
- Avoid fat interfaces with many methods
- Define role-specific interfaces (IReadRepository, IWriteRepository)

#### Dependency Inversion Principle (DIP)
- Depend on abstractions (interfaces), not concretions
- Use dependency injection throughout
- Define interfaces in Application layer, implement in Infrastructure
- Inject services via constructor injection


### 4. API Design & Controllers

#### Implemented Controllers

**BaseController** (Shared Logic)
```csharp
[ApiController]
public class BaseController : ControllerBase
{
    protected readonly IMediator _mediator;

    public BaseController(IMediator mediator)
    {
        _mediator = mediator;
    }

    protected string GetIpAddress()
    {
        return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }

    protected Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim!);
    }
}
```

**AuthController** (`/api/v1/auth`)
```
POST   /register     # Create tenant + user (returns tokens)
POST   /login        # Authenticate user (returns tokens)
POST   /refresh      # Get new access token using refresh token
GET    /me           # Get current authenticated user [Authorized]
```

**TenantsController** (`/api/v1/tenants`)
```
GET    /             # Get user's accessible tenants [Authorized]
```

#### Response Format Standards

**Success Response**:
```json
{
  "data": { ...actualData },
  "message": "Operation successful",
  "success": true
}
```

**Error Response**:
```json
{
  "message": "Error description",
  "errors": ["Detailed error 1", "Detailed error 2"],
  "success": false
}
```

#### HTTP Status Code Usage
- **200 OK**: Successful GET, PUT, POST with response data
- **201 Created**: Successful resource creation
- **204 No Content**: Successful DELETE or action with no response
- **400 Bad Request**: Validation errors, invalid input
- **401 Unauthorized**: Missing or invalid JWT token
- **403 Forbidden**: Valid auth but insufficient permissions
- **404 Not Found**: Resource doesn't exist
- **500 Internal Server Error**: Unhandled exceptions

#### Controller Design Principles
- Keep controllers thin - delegate to MediatR
- Use [Authorize] attribute for protected endpoints
- Use [AllowAnonymous] for public endpoints (register, login)
- Extract cross-cutting logic to base controller
- Return consistent `IActionResult` types


### 5. Swagger/OpenAPI Configuration

#### Current Implementation (Program.cs)
```csharp
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "SaaS Multi-Tenant API",
        Version = "v1",
        Description = "Multi-tenant SaaS API with JWT authentication"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});
```

#### Features
- **Interactive UI**: Available at `/swagger` in Development
- **JWT Testing**: "Authorize" button to add Bearer token
- **Auto Documentation**: Reads XML comments from controllers
- **Response Examples**: Shows DTOs and status codes
- **Try It Out**: Test endpoints directly from browser

#### Access
- Development: `http://localhost:5000/swagger`
- Production: Disabled for security

### 6. Authentication & Authorization

#### JWT Configuration (appsettings.json)
```json
{
  "JWT": {
    "Secret": "your-secret-key-min-32-characters",
    "Issuer": "SaaS.Api",
    "Audience": "SaaS.Client",
    "AccessTokenExpirationMinutes": 15,
    "RefreshTokenExpirationDays": 7
  }
}
```

#### Token Generation (AuthService)
```csharp
public string GenerateJwtToken(User user, List<Guid> tenantIds)
{
    var claims = new List<Claim>
    {
        new(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new(ClaimTypes.Email, user.Email),
        new(ClaimTypes.Name, user.Name)
    };

    // Add tenant IDs as claims
    claims.AddRange(tenantIds.Select(id =>
        new Claim("TenantId", id.ToString())));

    // Generate JWT with 15-minute expiration
}

public string GenerateRefreshToken()
{
    // 64-byte cryptographically secure random token
    return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
}
```

#### Password Hashing
- **Library**: BCrypt.NET
- **Work Factor**: 12 (2^12 = 4096 iterations)
- **Salt**: Auto-generated per password

```csharp
public string HashPassword(string password)
{
    return BCrypt.Net.BCrypt.HashPassword(password, 12);
}

public bool VerifyPassword(string password, string hash)
{
    return BCrypt.Net.BCrypt.Verify(password, hash);
}
```

#### Authentication Flow

**1. Registration**
- User provides email, password, company name, slug
- System creates Tenant, User, UserTenant (Owner)
- Returns JWT access + refresh tokens

**2. Login**
- User provides email + password
- Validates credentials with BCrypt
- Loads all tenants user has access to
- Generates tokens with tenant claims
- Returns user info + tenants + tokens

**3. Token Refresh**
- Client sends refresh token
- Validates token is not expired/revoked
- Generates new access + refresh tokens
- Revokes old refresh token (rotation)
- Returns new tokens

#### Multi-Tenant Authorization (TenantResolutionMiddleware)

**Purpose**: Validate user has access to requested tenant

**How it works**:
1. Extracts `X-Tenant-Id` header from request
2. Gets user ID from JWT claims
3. Queries UserTenants table for relationship
4. Sets tenant context if valid
5. Returns 403 if user doesn't have access

**Bypassed for**:
- `/api/v1/auth/*` endpoints
- Unauthenticated requests

**Usage in requests**:
```
GET /api/v1/products
Headers:
  Authorization: Bearer eyJhbGc...
  X-Tenant-Id: 123e4567-e89b-12d3-a456-426614174000
```

#### Authorization Attributes
```csharp
[Authorize]                    // Requires valid JWT
[AllowAnonymous]              // Bypass auth requirement
```

**Planned**: Custom policy-based authorization
- `[RequirePermission("invoices.delete")]`
- `[RequireRole("Admin")]`
- `[RequireSubscription("Premium")]`


### 7. Repository Pattern

#### Generic Repository Interface
```csharp
public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
    void Update(T entity);
    void Delete(T entity);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
}
```

#### Specific Repository Interfaces

**IUserRepository**
```csharp
public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);
}
```

**ITenantRepository**
```csharp
public interface ITenantRepository : IRepository<Tenant>
{
    Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken = default);
    Task<Tenant?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
}
```

**IUserTenantRepository**
```csharp
public interface IUserTenantRepository : IRepository<UserTenant>
{
    Task<List<UserTenant>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> HasAccessAsync(Guid userId, Guid tenantId, CancellationToken cancellationToken = default);
    Task<UserTenant?> GetByUserAndTenantAsync(Guid userId, Guid tenantId, CancellationToken cancellationToken = default);
}
```

**IRefreshTokenRepository**
```csharp
public interface IRefreshTokenRepository : IRepository<RefreshToken>
{
    Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<List<RefreshToken>> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}
```

#### Generic Repository Implementation
```csharp
public class Repository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FindAsync(new object[] { id }, cancellationToken);
    }

    public virtual async Task<List<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.ToListAsync(cancellationToken);
    }

    public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(entity, cancellationToken);
        return entity;
    }

    public virtual void Update(T entity)
    {
        _dbSet.Update(entity);
    }

    public virtual void Delete(T entity)
    {
        _dbSet.Remove(entity);
    }

    public virtual async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(e => e.Id == id, cancellationToken);
    }
}
```

#### Unit of Work Pattern
```csharp
public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    ITenantRepository Tenants { get; }
    IUserTenantRepository UserTenants { get; }
    IRefreshTokenRepository RefreshTokens { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;

    public IUserRepository Users { get; }
    public ITenantRepository Tenants { get; }
    public IUserTenantRepository UserTenants { get; }
    public IRefreshTokenRepository RefreshTokens { get; }

    public UnitOfWork(ApplicationDbContext context, /* ...inject repositories */)
    {
        _context = context;
        // Assign repositories
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
```

**Benefits**:
- Single transaction scope across multiple repositories
- Automatic audit field updates in SaveChangesAsync
- Simplified dependency injection (inject IUnitOfWork instead of many repos)
- Testable with mocking


### 8. Validation & Error Handling

#### FluentValidation Implementation

**Example Validator**:
```csharp
public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(255);

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8).WithMessage("Password must be at least 8 characters")
            .Matches(@"[A-Z]").WithMessage("Password must contain uppercase letter")
            .Matches(@"[a-z]").WithMessage("Password must contain lowercase letter")
            .Matches(@"[0-9]").WithMessage("Password must contain digit");

        RuleFor(x => x.CompanyName)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Slug)
            .NotEmpty()
            .Matches("^[a-z0-9-]+$").WithMessage("Slug can only contain lowercase letters, numbers, and hyphens");
    }
}
```

**Auto-Validation via Pipeline Behavior**:
- ValidationBehavior automatically runs before handler
- Throws `ValidationException` if validation fails
- Exception caught by ExceptionHandlingMiddleware
- Returns 400 with structured error messages

#### Global Exception Handling Middleware

```csharp
public class ExceptionHandlingMiddleware
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            await HandleValidationExceptionAsync(context, ex);  // 400
        }
        catch (UnauthorizedAccessException ex)
        {
            await HandleUnauthorizedAccessExceptionAsync(context, ex);  // 401
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);  // 500
        }
    }
}
```

**Error Response Format**:
```json
{
  "message": "Validation failed",
  "errors": [
    "Email is required",
    "Password must be at least 8 characters"
  ],
  "success": false
}
```

#### Exception Types
- **ValidationException** (FluentValidation) → 400 Bad Request
- **UnauthorizedAccessException** → 401 Unauthorized
- **KeyNotFoundException** → 404 Not Found
- **Exception** (catch-all) → 500 Internal Server Error

#### Result Pattern Usage
```csharp
// In handlers
if (await _unitOfWork.Users.EmailExistsAsync(command.Email))
    return Result<LoginResponseDto>.Failure("Email already registered");

if (user == null)
    return Result<UserDto>.Failure("User not found");

return Result<UserDto>.Success(userDto);

// In controllers
var result = await _mediator.Send(command);
if (!result.IsSuccess)
    return BadRequest(new { message = result.Error, errors = result.Errors });

return Ok(new { data = result.Value, success = true });
```

### 9. SOLID Principles Implementation

#### Single Responsibility Principle (SRP)
✅ **Implemented**:
- Each command handler has one responsibility
- Controllers only orchestrate (delegate to MediatR)
- Repositories handle data access only
- AuthService handles auth logic only
- Middleware classes handle single concerns

**Example**:
- `RegisterCommandHandler` only handles registration
- `TenantResolutionMiddleware` only resolves tenant context
- `UserRepository` only handles User data access

#### Open/Closed Principle (OCP)
✅ **Implemented**:
- Pipeline behaviors extend functionality without modifying handlers
- Generic `Repository<T>` extensible via specific repositories
- Middleware pipeline easily extended

**Example**: Add new validation without changing handlers - just add validator class

#### Liskov Substitution Principle (LSP)
✅ **Implemented**:
- `AuditableEntity` extends `BaseEntity` correctly
- All entity configurations implement `IEntityTypeConfiguration<T>`
- Repositories inherit from `Repository<T>` properly

#### Interface Segregation Principle (ISP)
✅ **Implemented**:
- Focused interfaces: `IAuthService`, `IUserRepository`, `ITenantContext`
- Separate `IRepository<T>` from specific repositories
- Clients depend only on methods they use

**Example**: `IAuthService` has only auth methods, not user management

#### Dependency Inversion Principle (DIP)
✅ **Implemented**:
- All layers depend on Application layer interfaces
- Infrastructure implements interfaces
- Controllers depend on `IMediator`, not handlers
- Handlers depend on `IUnitOfWork`, `IAuthService`, not concrete types

**Dependency Injection** (Program.cs):
```csharp
// Application Services
builder.Services.AddMediatR(cfg => { /* ... */ });
builder.Services.AddValidatorsFromAssembly(/* ... */);

// Infrastructure Services
builder.Services.AddScoped<ITenantContext, TenantContext>();
builder.Services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());
builder.Services.AddScoped<IAuthService, AuthService>();

// Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ITenantRepository, TenantRepository>();
builder.Services.AddScoped<IUserTenantRepository, UserTenantRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
```

### 10. Logging & Monitoring

#### Serilog Configuration
```csharp
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();
app.UseSerilogRequestLogging();
```

**Features**:
- Structured logging (JSON format)
- Daily rolling log files
- Console output in development
- Request/response logging
- Correlation IDs via LogContext

**Log Locations**:
- Console: Real-time in terminal
- Files: `backend/src/Api/Logs/log-YYYYMMDD.txt`

**Example Log Entry**:
```
[INF] HTTP POST /api/v1/auth/register responded 200 in 234ms
[ERR] Database connection failed: {Error}
[WRN] User attempted access to unauthorized tenant {TenantId}
```


## Current API Endpoints

### Authentication (`/api/v1/auth`)

#### POST `/register`
**Purpose**: Register new company and admin user
**Auth**: Anonymous
**Request**:
```json
{
  "companyName": "Acme Corp",
  "slug": "acme-corp",
  "email": "admin@acme.com",
  "password": "SecurePass123!",
  "name": "John Doe"
}
```
**Response**: `LoginResponseDto` with tokens

#### POST `/login`
**Purpose**: Authenticate user
**Auth**: Anonymous
**Request**:
```json
{
  "email": "admin@acme.com",
  "password": "SecurePass123!"
}
```
**Response**: `LoginResponseDto` with tokens + user info + tenants

#### POST `/refresh`
**Purpose**: Get new access token
**Auth**: Anonymous
**Request**:
```json
{
  "refreshToken": "base64-encoded-token",
  "ipAddress": "192.168.1.1"
}
```
**Response**: New tokens

#### GET `/me`
**Purpose**: Get current authenticated user
**Auth**: Required (Bearer token)
**Response**: `UserDto`

### Tenants (`/api/v1/tenants`)

#### GET `/`
**Purpose**: Get all tenants user has access to
**Auth**: Required (Bearer token)
**Response**: `List<TenantDto>`

## Development Setup

### Prerequisites
- .NET 8 SDK
- PostgreSQL 16
- Docker & Docker Compose (optional)

### Configuration

**appsettings.Development.json**:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=saasdb;Username=postgres;Password=postgres"
  },
  "JWT": {
    "Secret": "your-secret-key-minimum-32-characters-long",
    "Issuer": "SaaS.Api",
    "Audience": "SaaS.Client",
    "AccessTokenExpirationMinutes": 15,
    "RefreshTokenExpirationDays": 7
  }
}
```

### Running Locally

```powershell
# Navigate to backend
cd backend

# Restore packages
dotnet restore

# Run migrations
cd src/Api
dotnet ef database update --project ../Infrastructure

# Run API
dotnet run
```

**API will be available at**: `http://localhost:5000`
**Swagger UI**: `http://localhost:5000/swagger`

### Docker Setup

```powershell
# From project root
docker-compose up --build
```

## Testing Strategy

### Manual Testing with Swagger
1. Navigate to `/swagger`
2. Test `/auth/register` to create account
3. Copy access token from response
4. Click "Authorize" button
5. Enter `Bearer {token}`
6. Test protected endpoints

### Example Request Flow
```powershell
# 1. Register
POST http://localhost:5000/api/v1/auth/register
Body: { companyName, slug, email, password, name }

# 2. Login
POST http://localhost:5000/api/v1/auth/login
Body: { email, password }
Response: { accessToken, refreshToken, user, tenants }

# 3. Get Tenants (requires Bearer token + X-Tenant-Id header)
GET http://localhost:5000/api/v1/tenants
Headers:
  Authorization: Bearer {accessToken}
```

## Database Schema

### Users Table
```sql
CREATE TABLE "Users" (
    "Id" uuid PRIMARY KEY,
    "Email" varchar(255) UNIQUE NOT NULL,
    "PasswordHash" text NOT NULL,
    "Name" varchar(200) NOT NULL,
    "IsActive" boolean DEFAULT true,
    "EmailConfirmed" boolean DEFAULT false,
    "CreatedAt" timestamp NOT NULL,
    "UpdatedAt" timestamp NOT NULL,
    "IsDeleted" boolean DEFAULT false,
    "DeletedAt" timestamp NULL
);

CREATE INDEX "IX_Users_Email" ON "Users" ("Email");
```

### Tenants Table
```sql
CREATE TABLE "Tenants" (
    "Id" uuid PRIMARY KEY,
    "Name" varchar(200) NOT NULL,
    "Slug" varchar(100) UNIQUE NOT NULL,
    "Status" int NOT NULL,  -- 0=Active, 1=Suspended, 2=Trial
    "ConnectionString" text NULL,
    "SchemaName" varchar(100) NULL,
    "CreatedAt" timestamp NOT NULL,
    "UpdatedAt" timestamp NOT NULL,
    "IsDeleted" boolean DEFAULT false,
    "DeletedAt" timestamp NULL
);

CREATE UNIQUE INDEX "IX_Tenants_Slug" ON "Tenants" ("Slug");
```

### UserTenants Table (Junction with Role)
```sql
CREATE TABLE "UserTenants" (
    "UserId" uuid NOT NULL,
    "TenantId" uuid NOT NULL,
    "Role" int NOT NULL,  -- 0=Owner, 1=Admin, 2=User
    "CreatedAt" timestamp NOT NULL,
    PRIMARY KEY ("UserId", "TenantId"),
    FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE,
    FOREIGN KEY ("TenantId") REFERENCES "Tenants" ("Id") ON DELETE CASCADE
);
```

### RefreshTokens Table
```sql
CREATE TABLE "RefreshTokens" (
    "Id" uuid PRIMARY KEY,
    "UserId" uuid NOT NULL,
    "Token" text UNIQUE NOT NULL,
    "ExpiresAt" timestamp NOT NULL,
    "CreatedAt" timestamp NOT NULL,
    "CreatedByIp" varchar(50) NULL,
    "RevokedAt" timestamp NULL,
    "RevokedByIp" varchar(50) NULL,
    "ReplacedByToken" text NULL,
    FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_RefreshTokens_Token" ON "RefreshTokens" ("Token");
CREATE INDEX "IX_RefreshTokens_UserId" ON "RefreshTokens" ("UserId");
```


## Next Steps & Roadmap

### Immediate Extensions (Ready to Implement)

#### 1. Billing Module
**Entities**:
- Customer
- Invoice
- InvoiceItem
- Payment
- Subscription
- Plan

**Features**:
- Create/manage invoices
- Process payments
- Subscription management
- Usage tracking
- Billing reports

#### 2. Inventory Module
**Entities**:
- Product
- ProductVariant
- Category
- StockItem
- StockMovement
- Warehouse

**Features**:
- Product CRUD
- Stock management
- Low stock alerts
- Inventory reports
- Multi-warehouse support

#### 3. Advanced Authorization
**Implement**:
- Permission-based access control
- Custom authorization policies
- Role-based permissions
- Resource-based authorization
- Permission management UI

#### 4. Performance Optimizations
**Implement**:
- Redis caching for read-heavy data
- Query result caching
- Response compression
- Database query optimization
- Pagination for large datasets
- Background job processing (Hangfire)

#### 5. Enhanced Security
**Implement**:
- Email verification
- Password reset flow
- Two-factor authentication (2FA)
- Account lockout after failed attempts
- Audit logging for security events
- Rate limiting

#### 6. Multi-Tenancy Enhancements
**Implement**:
- Tenant-specific database schemas
- Tenant isolation verification
- Tenant settings/configuration
- Cross-tenant reporting (admin only)
- Tenant analytics

### Testing Implementation
- Unit tests (xUnit)
- Integration tests
- Repository tests
- API endpoint tests
- Mock IUnitOfWork for handler tests

### CI/CD Pipeline
- GitHub Actions workflow
- Automated testing
- Docker image building
- Database migration automation
- Deployment to staging/production

## Code Standards & Conventions

### Naming Conventions
- **Classes**: PascalCase (`UserRepository`, `RegisterCommand`)
- **Interfaces**: I + PascalCase (`IUserRepository`, `IAuthService`)
- **Methods**: PascalCase (`GetByIdAsync`, `ValidateCredentials`)
- **Private fields**: `_camelCase` (`_context`, `_mediator`)
- **Parameters/Variables**: camelCase (`userId`, `tenantId`)
- **Constants**: PascalCase (`MaxLoginAttempts`)

### File Organization
- One class per file
- File name matches class name
- Group related files in folders
- Feature-based organization for CQRS
- Shared code in Common folders

### Async/Await Guidelines
- All repository methods are async
- All handler methods are async
- Use `CancellationToken` parameter
- Suffix async methods with `Async`
- Always await async calls (no `.Result` or `.Wait()`)

### Error Handling
- Use Result pattern in handlers
- Throw exceptions for exceptional cases only
- Let middleware handle exception formatting
- Log errors with context
- Never expose sensitive data in errors

### Dependency Injection
- Use constructor injection
- Inject interfaces, not implementations
- Scope: AddScoped for request-scoped services
- Register all services in Program.cs
- Keep constructors clean (no logic)

## Constraints and Boundaries

### In Scope
✅ Backend architecture and implementation
✅ Entity Framework Core configuration
✅ CQRS pattern with MediatR
✅ API design and controller development
✅ Swagger/OpenAPI documentation
✅ JWT authentication & authorization
✅ Repository and Unit of Work patterns
✅ FluentValidation
✅ Multi-tenant architecture
✅ SOLID principles application
✅ Logging and error handling

### Out of Scope
❌ Frontend development (delegate to **Frontend Agent**)
❌ UX/UI design decisions (delegate to **UX Agent**)
❌ Infrastructure provisioning (delegate to **DevOps/Architecture Agent**)
❌ Business-specific logic (collaborate with domain experts)
❌ Payment gateway integrations (provide pattern guidance only)
❌ Email service implementation (provide interface only)

### Technology Constraints
- **Must use**: .NET 8, EF Core 8+, PostgreSQL 16
- **Must follow**: Clean Architecture, CQRS, Repository pattern
- **Must implement**: JWT auth, multi-tenancy, validation
- **Can extend**: Additional modules, features, optimizations

## Integration with Other Agents

| Agent | Integration Point | Example Collaboration |
|-------|------------------|----------------------|
| **Project Architecture Agent** | Receives system design, multi-tenant patterns | Database schema design, API contracts |
| **Frontend Agent** | Provides API contracts, DTOs, authentication flow | API endpoint specifications, error formats |
| **UX Agent** | Receives UX requirements for API responses | Pagination, filtering, error messages |

## Troubleshooting

### Common Issues

**Migration Errors**
```powershell
# Reset migrations
dotnet ef database drop --force
dotnet ef migrations remove
dotnet ef migrations add InitialCreate
dotnet ef database update
```

**Connection String Issues**
- Verify PostgreSQL is running
- Check port (default 5432)
- Verify credentials in appsettings.json
- Test connection string manually

**JWT Token Issues**
- Verify Secret is at least 32 characters
- Check token expiration time
- Verify Issuer and Audience match configuration
- Ensure Bearer token format in Authorization header

**Tenant Resolution Fails**
- Verify `X-Tenant-Id` header is present
- Check UserTenants table for relationship
- Verify user has access to tenant
- Check middleware order in Program.cs

## Version History

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | Feb 6, 2026 | Initial implementation with auth, multi-tenancy, CQRS |

---

## Contact & Support

For questions about backend implementation:
1. Check [BACKEND_SETUP_COMPLETE.md](../BACKEND_SETUP_COMPLETE.md) for setup details
2. Review Swagger docs at `/swagger` when API is running
3. Check Serilog output in console or `Logs/` directory
4. Consult this agent documentation for patterns and standards

**Framework Versions**:
- .NET: 8.0
- Entity Framework Core: 8.0+
- PostgreSQL: 16

**Last Updated**: February 6, 2026
