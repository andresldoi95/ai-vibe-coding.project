# Auth Agent

## Specialization
Expert in authentication, authorization, role-based access control (RBAC), and multi-tenant security for the SaaS Billing + Inventory Management System. Handles user identity, permissions, company associations, and policy-based authorization.

## Tech Stack
- **Authentication**: JWT Bearer Tokens with Refresh Tokens
- **Password Security**: BCrypt.NET
- **Authorization**: Policy-based authorization (.NET Identity patterns)
- **Multi-Tenancy**: Schema-per-tenant with user-company associations
- **ORM**: Entity Framework Core 8+ (for auth entities)
- **Validation**: FluentValidation

## Core Concepts

### Multi-Tenant RBAC Architecture

#### Key Principles
1. **Users are global**: A single user account can access multiple companies
2. **Roles are company-scoped**: Roles are defined per tenant/company
3. **Permissions are granular**: Roles contain multiple permissions
4. **Context-aware authorization**: Authorization checks consider both user identity and current tenant context

#### Entity Relationships
```
User (1) ──────< UserCompany (N) >────── (1) Company/Tenant
                      │
                      │ has
                      ▼
                    Role (N)
                      │
                      │ contains
                      ▼
                 Permission (N)
```

## Data Model

### Core Auth Entities

#### 1. User (Global Scope)
Located in: `Domain/Entities/User.cs`

```csharp
public class User : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public bool IsEmailVerified { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? LastLoginAt { get; set; }

    // Navigation
    public ICollection<UserCompany> UserCompanies { get; set; } = new List<UserCompany>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
```

**Constraints**:
- Email must be unique globally
- Cannot be soft-deleted (account deactivation via `IsActive`)
- Audit fields inherited from BaseEntity

#### 2. UserCompany (Junction with Role)
Located in: `Domain/Entities/UserCompany.cs`

```csharp
public class UserCompany : TenantEntity
{
    public Guid UserId { get; set; }
    public Guid CompanyId { get; set; } // Same as TenantId
    public Guid RoleId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public User User { get; set; } = null!;
    public Tenant Company { get; set; } = null!;
    public Role Role { get; set; } = null!;
}
```

**Business Rules**:
- A user can belong to multiple companies
- Each user-company association has exactly one role
- TenantId = CompanyId for multi-tenant isolation
- Unique constraint: `(UserId, CompanyId)` - user can only have one role per company
- Soft delete supported (when user leaves company)

**Key Scenarios**:
- User switches companies → Load different `UserCompany` record → Different role
- User removed from company → Soft delete `UserCompany` record
- User rejoins company → Create new `UserCompany` or restore soft-deleted record

#### 3. Role (Tenant-Scoped)
Located in: `Domain/Entities/Role.cs`

```csharp
public class Role : TenantEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsSystemRole { get; set; } = false; // Pre-defined roles
    public int Priority { get; set; } = 0; // For hierarchy (higher = more privileged)

    // Navigation
    public ICollection<UserCompany> UserCompanies { get; set; } = new List<UserCompany>();
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
```

**Constraints**:
- Unique: `(TenantId, Name)`
- System roles (Owner, Admin, User) created during tenant initialization
- Custom roles can be created by company admins
- Cannot delete system roles or roles with active users

**Default System Roles** (created per tenant):
```csharp
public enum SystemRoleType
{
    Owner,      // Full access, cannot be deleted from company
    Admin,      // Manage users, roles, billing, settings
    Manager,    // Manage inventory, view reports
    User        // Basic access
}
```

#### 4. Permission (Global Scope)
Located in: `Domain/Entities/Permission.cs`

```csharp
public class Permission : BaseEntity
{
    public string Name { get; set; } = string.Empty; // e.g., "invoices.create"
    public string Resource { get; set; } = string.Empty; // e.g., "invoices"
    public string Action { get; set; } = string.Empty; // e.g., "create"
    public string Description { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty; // "Billing", "Inventory"

    // Navigation
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
```

**Naming Convention**: `{resource}.{action}`
Examples:
- `invoices.create`
- `invoices.read`
- `invoices.update`
- `invoices.delete`
- `products.read`
- `warehouses.manage`
- `users.manage`
- `settings.billing`

**Constraints**:
- Unique: `Name`
- Permissions are seeded during application initialization
- Cannot be created/modified by users (system-managed)

#### 5. RolePermission (Junction Table)
Located in: `Domain/Entities/RolePermission.cs`

```csharp
public class RolePermission : TenantEntity
{
    public Guid RoleId { get; set; }
    public Guid PermissionId { get; set; }

    // Navigation
    public Role Role { get; set; } = null!;
    public Permission Permission { get; set; } = null!;
}
```

**Constraints**:
- Unique: `(RoleId, PermissionId, TenantId)`
- Inherits TenantId from Role for multi-tenant isolation

## Authorization Patterns

### 1. Policy-Based Authorization

#### Define Policies (Startup/Program.cs)
```csharp
builder.Services.AddAuthorization(options =>
{
    // Permission-based policies
    options.AddPolicy("invoices.create", policy =>
        policy.RequireAssertion(context =>
            HasPermission(context.User, "invoices.create")));

    options.AddPolicy("users.manage", policy =>
        policy.RequireAssertion(context =>
            HasPermission(context.User, "users.manage")));

    // Role-based policies
    options.AddPolicy("RequireAdmin", policy =>
        policy.RequireAssertion(context =>
            HasRole(context.User, "Admin") || HasRole(context.User, "Owner")));
});
```

#### Custom Authorization Handler
Located in: `Infrastructure/Authorization/PermissionAuthorizationHandler.cs`

```csharp
public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly ITenantContext _tenantContext;
    private readonly IUserPermissionService _permissionService;

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var userId = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var tenantId = _tenantContext.TenantId;

        if (await _permissionService.UserHasPermissionAsync(
            Guid.Parse(userId),
            tenantId,
            requirement.Permission))
        {
            context.Succeed(requirement);
        }
    }
}
```

### 2. Controller Authorization

#### Using Policies
```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize] // Requires authentication
public class InvoicesController : BaseController
{
    [HttpGet]
    [Authorize(Policy = "invoices.read")]
    public async Task<IActionResult> GetAll() { }

    [HttpPost]
    [Authorize(Policy = "invoices.create")]
    public async Task<IActionResult> Create() { }

    [HttpDelete("{id}")]
    [Authorize(Policy = "invoices.delete")]
    public async Task<IActionResult> Delete(Guid id) { }
}
```

#### Using Custom Attributes
```csharp
[RequirePermission("users.manage")]
public async Task<IActionResult> InviteUser() { }

[RequireRole("Admin", "Owner")]
public async Task<IActionResult> DeleteCompany() { }
```

### 3. Tenant Context Resolution

#### TenantContext Service
Located in: `Infrastructure/Services/TenantContext.cs`

```csharp
public class TenantContext : ITenantContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public Guid TenantId => GetTenantIdFromClaims();
    public Guid UserId => GetUserIdFromClaims();
    public Guid UserCompanyId => GetUserCompanyIdFromClaims();

    private Guid GetTenantIdFromClaims()
    {
        var claim = _httpContextAccessor.HttpContext?.User
            .FindFirst("TenantId");
        return claim != null ? Guid.Parse(claim.Value) : Guid.Empty;
    }
}
```

#### JWT Claims Structure
```json
{
  "sub": "user-guid",
  "email": "user@example.com",
  "TenantId": "company-guid",
  "UserCompanyId": "user-company-guid",
  "RoleId": "role-guid",
  "RoleName": "Admin",
  "permissions": ["invoices.read", "invoices.create", "products.read"],
  "exp": 1234567890
}
```

**Important**:
- JWT contains current tenant context
- When user switches companies, new JWT is issued
- Permissions are cached in JWT to avoid DB lookups

## Implementation Workflows

### 1. User Registration & Company Creation

**Flow**: New user creates account + new company
```csharp
// Command: RegisterUserCommand
public async Task<Result<LoginResponseDto>> Handle(RegisterUserCommand request)
{
    // 1. Create User (global)
    var user = new User { Email = request.Email, ... };

    // 2. Create Tenant/Company
    var tenant = new Tenant { Name = request.CompanyName, ... };

    // 3. Seed Default Roles for Tenant
    var ownerRole = new Role
    {
        TenantId = tenant.Id,
        Name = "Owner",
        IsSystemRole = true,
        Priority = 100
    };

    // 4. Create UserCompany association with Owner role
    var userCompany = new UserCompany
    {
        UserId = user.Id,
        CompanyId = tenant.Id,
        TenantId = tenant.Id,
        RoleId = ownerRole.Id
    };

    // 5. Generate JWT with tenant context
    var token = _authService.GenerateToken(user, tenant, userCompany);

    return Result.Success(new LoginResponseDto { Token = token });
}
```

### 2. User Login with Company Selection

**Flow**: User logs in and selects company
```csharp
// Command: LoginCommand
public async Task<Result<LoginResponseDto>> Handle(LoginCommand request)
{
    // 1. Validate credentials
    var user = await _userRepository.GetByEmailAsync(request.Email);
    if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        return Result.Failure("Invalid credentials");

    // 2. Get user's companies
    var userCompanies = await _userCompanyRepository
        .GetByUserIdAsync(user.Id);

    if (userCompanies.Count == 0)
        return Result.Failure("No companies associated");

    // 3. If multiple companies, return list for selection
    if (userCompanies.Count > 1 && request.CompanyId == null)
    {
        return Result.Success(new LoginResponseDto
        {
            RequiresCompanySelection = true,
            Companies = userCompanies.Select(uc => new CompanyDto
            {
                Id = uc.CompanyId,
                Name = uc.Company.Name
            })
        });
    }

    // 4. Select company (first one or requested)
    var selectedUserCompany = request.CompanyId.HasValue
        ? userCompanies.First(uc => uc.CompanyId == request.CompanyId)
        : userCompanies.First();

    // 5. Load role and permissions
    var role = await _roleRepository.GetByIdAsync(selectedUserCompany.RoleId);
    var permissions = await _permissionRepository
        .GetByRoleIdAsync(role.Id);

    // 6. Generate JWT with full context
    var token = _authService.GenerateToken(
        user,
        selectedUserCompany.Company,
        selectedUserCompany,
        role,
        permissions);

    return Result.Success(new LoginResponseDto { Token = token });
}
```

### 3. Company Switching

**Flow**: Authenticated user switches to different company
```csharp
// Command: SwitchCompanyCommand
[Authorize]
public async Task<Result<LoginResponseDto>> Handle(SwitchCompanyCommand request)
{
    var userId = _tenantContext.UserId;

    // 1. Verify user has access to target company
    var userCompany = await _userCompanyRepository
        .GetByUserAndCompanyAsync(userId, request.CompanyId);

    if (userCompany == null || !userCompany.IsActive)
        return Result.Failure("Access denied");

    // 2. Load role and permissions for new company
    var role = await _roleRepository.GetByIdAsync(userCompany.RoleId);
    var permissions = await _permissionRepository.GetByRoleIdAsync(role.Id);

    // 3. Generate new JWT with new tenant context
    var user = await _userRepository.GetByIdAsync(userId);
    var token = _authService.GenerateToken(user, userCompany.Company, userCompany, role, permissions);

    return Result.Success(new LoginResponseDto { Token = token });
}
```

### 4. Invite User to Company

**Flow**: Admin invites existing or new user to company
```csharp
// Command: InviteUserToCompanyCommand
[Authorize(Policy = "users.manage")]
public async Task<Result> Handle(InviteUserToCompanyCommand request)
{
    var tenantId = _tenantContext.TenantId;

    // 1. Check if user exists
    var user = await _userRepository.GetByEmailAsync(request.Email);

    // 2. If new user, create account (without password - will set via invitation link)
    if (user == null)
    {
        user = new User
        {
            Email = request.Email,
            IsEmailVerified = false,
            IsActive = false // Activated when they set password
        };
        await _userRepository.AddAsync(user);
    }

    // 3. Check if already member
    var existingAssociation = await _userCompanyRepository
        .GetByUserAndCompanyAsync(user.Id, tenantId);

    if (existingAssociation != null && existingAssociation.IsActive)
        return Result.Failure("User already a member");

    // 4. Create/restore UserCompany association
    if (existingAssociation != null)
    {
        existingAssociation.IsActive = true;
        existingAssociation.RoleId = request.RoleId;
        existingAssociation.IsDeleted = false;
    }
    else
    {
        var userCompany = new UserCompany
        {
            UserId = user.Id,
            CompanyId = tenantId,
            TenantId = tenantId,
            RoleId = request.RoleId
        };
        await _userCompanyRepository.AddAsync(userCompany);
    }

    // 5. Send invitation email
    await _emailService.SendInvitationAsync(user.Email, tenantId);

    return Result.Success();
}
```

### 5. Permission Check

**Service**: `IUserPermissionService`
```csharp
public class UserPermissionService : IUserPermissionService
{
    public async Task<bool> UserHasPermissionAsync(
        Guid userId,
        Guid tenantId,
        string permissionName)
    {
        // 1. Get UserCompany for current tenant
        var userCompany = await _userCompanyRepository
            .GetByUserAndCompanyAsync(userId, tenantId);

        if (userCompany == null || !userCompany.IsActive)
            return false;

        // 2. Get role permissions
        var permissions = await _dbContext.RolePermissions
            .Where(rp => rp.RoleId == userCompany.RoleId && rp.TenantId == tenantId)
            .Include(rp => rp.Permission)
            .Select(rp => rp.Permission.Name)
            .ToListAsync();

        return permissions.Contains(permissionName);
    }

    public async Task<List<string>> GetUserPermissionsAsync(Guid userId, Guid tenantId)
    {
        var userCompany = await _userCompanyRepository
            .GetByUserAndCompanyAsync(userId, tenantId);

        if (userCompany == null)
            return new List<string>();

        return await _dbContext.RolePermissions
            .Where(rp => rp.RoleId == userCompany.RoleId && rp.TenantId == tenantId)
            .Include(rp => rp.Permission)
            .Select(rp => rp.Permission.Name)
            .ToListAsync();
    }
}
```

## Role & Permission Management

### 1. Create Custom Role

```csharp
// Command: CreateRoleCommand
[Authorize(Policy = "roles.manage")]
public async Task<Result<RoleDto>> Handle(CreateRoleCommand request)
{
    var tenantId = _tenantContext.TenantId;

    // Check for duplicate name in tenant
    var exists = await _dbContext.Roles
        .AnyAsync(r => r.TenantId == tenantId && r.Name == request.Name);

    if (exists)
        return Result.Failure("Role name already exists");

    var role = new Role
    {
        TenantId = tenantId,
        Name = request.Name,
        Description = request.Description,
        IsSystemRole = false,
        Priority = request.Priority
    };

    await _roleRepository.AddAsync(role);
    await _unitOfWork.SaveChangesAsync();

    return Result.Success(role.ToDto());
}
```

### 2. Assign Permissions to Role

```csharp
// Command: UpdateRolePermissionsCommand
[Authorize(Policy = "roles.manage")]
public async Task<Result> Handle(UpdateRolePermissionsCommand request)
{
    var tenantId = _tenantContext.TenantId;
    var role = await _roleRepository.GetByIdAsync(request.RoleId);

    if (role == null || role.TenantId != tenantId)
        return Result.Failure("Role not found");

    // Prevent modification of system roles in production (optional)
    if (role.IsSystemRole)
        return Result.Failure("Cannot modify system roles");

    // Remove existing permissions
    var existing = await _dbContext.RolePermissions
        .Where(rp => rp.RoleId == request.RoleId)
        .ToListAsync();
    _dbContext.RolePermissions.RemoveRange(existing);

    // Add new permissions
    foreach (var permissionId in request.PermissionIds)
    {
        _dbContext.RolePermissions.Add(new RolePermission
        {
            RoleId = request.RoleId,
            PermissionId = permissionId,
            TenantId = tenantId
        });
    }

    await _unitOfWork.SaveChangesAsync();
    return Result.Success();
}
```

### 3. Change User Role in Company

```csharp
// Command: UpdateUserRoleCommand
[Authorize(Policy = "users.manage")]
public async Task<Result> Handle(UpdateUserRoleCommand request)
{
    var tenantId = _tenantContext.TenantId;

    var userCompany = await _userCompanyRepository
        .GetByUserAndCompanyAsync(request.UserId, tenantId);

    if (userCompany == null)
        return Result.Failure("User not member of company");

    // Prevent removing last Owner
    if (userCompany.Role.Name == "Owner")
    {
        var ownerCount = await _dbContext.UserCompanies
            .CountAsync(uc => uc.CompanyId == tenantId &&
                             uc.Role.Name == "Owner" &&
                             uc.IsActive);

        if (ownerCount == 1)
            return Result.Failure("Cannot remove last owner");
    }

    userCompany.RoleId = request.NewRoleId;
    await _unitOfWork.SaveChangesAsync();

    return Result.Success();
}
```

## Permission Seeding

### Default Permissions (Application Startup)

```csharp
public class PermissionSeeder
{
    public static List<Permission> GetDefaultPermissions()
    {
        return new List<Permission>
        {
            // Billing Module
            new() { Name = "invoices.read", Resource = "invoices", Action = "read", Module = "Billing" },
            new() { Name = "invoices.create", Resource = "invoices", Action = "create", Module = "Billing" },
            new() { Name = "invoices.update", Resource = "invoices", Action = "update", Module = "Billing" },
            new() { Name = "invoices.delete", Resource = "invoices", Action = "delete", Module = "Billing" },
            new() { Name = "customers.read", Resource = "customers", Action = "read", Module = "Billing" },
            new() { Name = "customers.manage", Resource = "customers", Action = "manage", Module = "Billing" },
            new() { Name = "payments.read", Resource = "payments", Action = "read", Module = "Billing" },
            new() { Name = "payments.create", Resource = "payments", Action = "create", Module = "Billing" },

            // Inventory Module
            new() { Name = "products.read", Resource = "products", Action = "read", Module = "Inventory" },
            new() { Name = "products.create", Resource = "products", Action = "create", Module = "Inventory" },
            new() { Name = "products.update", Resource = "products", Action = "update", Module = "Inventory" },
            new() { Name = "products.delete", Resource = "products", Action = "delete", Module = "Inventory" },
            new() { Name = "warehouses.read", Resource = "warehouses", Action = "read", Module = "Inventory" },
            new() { Name = "warehouses.manage", Resource = "warehouses", Action = "manage", Module = "Inventory" },
            new() { Name = "stock.read", Resource = "stock", Action = "read", Module = "Inventory" },
            new() { Name = "stock.adjust", Resource = "stock", Action = "adjust", Module = "Inventory" },

            // User Management
            new() { Name = "users.read", Resource = "users", Action = "read", Module = "Settings" },
            new() { Name = "users.manage", Resource = "users", Action = "manage", Module = "Settings" },
            new() { Name = "roles.read", Resource = "roles", Action = "read", Module = "Settings" },
            new() { Name = "roles.manage", Resource = "roles", Action = "manage", Module = "Settings" },

            // Settings
            new() { Name = "settings.company", Resource = "settings", Action = "company", Module = "Settings" },
            new() { Name = "settings.billing", Resource = "settings", Action = "billing", Module = "Settings" },
        };
    }
}
```

### Default Role Permissions Mapping

```csharp
public class RolePermissionSeeder
{
    public static Dictionary<string, List<string>> GetDefaultRolePermissions()
    {
        return new Dictionary<string, List<string>>
        {
            ["Owner"] = new List<string>
            {
                "*.*" // All permissions
            },

            ["Admin"] = new List<string>
            {
                "invoices.*", "customers.*", "payments.*",
                "products.*", "warehouses.*", "stock.*",
                "users.read", "users.manage",
                "roles.read", "roles.manage",
                "settings.company"
            },

            ["Manager"] = new List<string>
            {
                "invoices.read", "invoices.create", "invoices.update",
                "customers.read",
                "products.*", "warehouses.*", "stock.*",
                "users.read"
            },

            ["User"] = new List<string>
            {
                "invoices.read", "customers.read",
                "products.read", "warehouses.read", "stock.read"
            }
        };
    }
}
```

## Database Migrations

### Adding Auth Tables

```bash
# Add migration
dotnet ef migrations add AddRolesAndPermissions --project src/Infrastructure --startup-project src/Api

# Update database
dotnet ef database update --project src/Infrastructure --startup-project src/Api
```

### Migration Class Example
```csharp
public partial class AddRolesAndPermissions : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Create Permissions table (global)
        migrationBuilder.CreateTable(
            name: "Permissions",
            columns: table => new
            {
                Id = table.Column<Guid>(nullable: false),
                Name = table.Column<string>(maxLength: 100, nullable: false),
                Resource = table.Column<string>(maxLength: 50, nullable: false),
                Action = table.Column<string>(maxLength: 50, nullable: false),
                Description = table.Column<string>(maxLength: 500),
                Module = table.Column<string>(maxLength: 50, nullable: false),
                CreatedAt = table.Column<DateTime>(nullable: false),
                UpdatedAt = table.Column<DateTime>(nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Permissions", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Permissions_Name",
            table: "Permissions",
            column: "Name",
            unique: true);

        // Create Roles table (tenant-scoped)
        migrationBuilder.CreateTable(
            name: "Roles",
            columns: table => new
            {
                Id = table.Column<Guid>(nullable: false),
                TenantId = table.Column<Guid>(nullable: false),
                Name = table.Column<string>(maxLength: 100, nullable: false),
                Description = table.Column<string>(maxLength: 500),
                IsSystemRole = table.Column<bool>(nullable: false, defaultValue: false),
                Priority = table.Column<int>(nullable: false, defaultValue: 0),
                CreatedAt = table.Column<DateTime>(nullable: false),
                UpdatedAt = table.Column<DateTime>(nullable: true),
                IsDeleted = table.Column<bool>(nullable: false, defaultValue: false),
                DeletedAt = table.Column<DateTime>(nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Roles", x => x.Id);
                table.ForeignKey(
                    name: "FK_Roles_Tenants_TenantId",
                    column: x => x.TenantId,
                    principalTable: "Tenants",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Roles_TenantId_Name",
            table: "Roles",
            columns: new[] { "TenantId", "Name" },
            unique: true,
            filter: "\"IsDeleted\" = false");

        // Create RolePermissions junction table
        migrationBuilder.CreateTable(
            name: "RolePermissions",
            columns: table => new
            {
                Id = table.Column<Guid>(nullable: false),
                TenantId = table.Column<Guid>(nullable: false),
                RoleId = table.Column<Guid>(nullable: false),
                PermissionId = table.Column<Guid>(nullable: false),
                CreatedAt = table.Column<DateTime>(nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_RolePermissions", x => x.Id);
                table.ForeignKey(
                    name: "FK_RolePermissions_Roles_RoleId",
                    column: x => x.RoleId,
                    principalTable: "Roles",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_RolePermissions_Permissions_PermissionId",
                    column: x => x.PermissionId,
                    principalTable: "Permissions",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_RolePermissions_RoleId_PermissionId_TenantId",
            table: "RolePermissions",
            columns: new[] { "RoleId", "PermissionId", "TenantId" },
            unique: true);

        // Update UserCompany to add RoleId
        migrationBuilder.AddColumn<Guid>(
            name: "RoleId",
            table: "UserCompanies",
            nullable: false,
            defaultValue: Guid.Empty);

        migrationBuilder.CreateIndex(
            name: "IX_UserCompanies_RoleId",
            table: "UserCompanies",
            column: "RoleId");

        migrationBuilder.AddForeignKey(
            name: "FK_UserCompanies_Roles_RoleId",
            table: "UserCompanies",
            column: "RoleId",
            principalTable: "Roles",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);
    }
}
```

## API Endpoints

### Auth Endpoints

```
POST   /api/auth/register              # Register user + create company
POST   /api/auth/login                 # Login (returns companies if multiple)
POST   /api/auth/refresh               # Refresh JWT token
POST   /api/auth/switch-company        # Switch to different company
POST   /api/auth/logout                # Invalidate refresh token
```

### User Management Endpoints

```
GET    /api/users                      # List company users [users.read]
POST   /api/users/invite               # Invite user to company [users.manage]
PUT    /api/users/{id}/role            # Change user role [users.manage]
DELETE /api/users/{id}                 # Remove user from company [users.manage]
GET    /api/users/me                   # Current user profile
GET    /api/users/me/companies         # User's companies
```

### Role Management Endpoints

```
GET    /api/roles                      # List roles in current company [roles.read]
POST   /api/roles                      # Create custom role [roles.manage]
PUT    /api/roles/{id}                 # Update role [roles.manage]
DELETE /api/roles/{id}                 # Delete role [roles.manage]
GET    /api/roles/{id}/permissions     # Get role permissions [roles.read]
PUT    /api/roles/{id}/permissions     # Update role permissions [roles.manage]
```

### Permission Endpoints

```
GET    /api/permissions                # List all available permissions [roles.read]
GET    /api/permissions/modules        # Group permissions by module [roles.read]
```

## Frontend Integration

### Auth Store (Pinia)

```typescript
// stores/auth.ts
export const useAuthStore = defineStore('auth', {
  state: () => ({
    user: null as User | null,
    currentCompany: null as Company | null,
    companies: [] as Company[],
    permissions: [] as string[],
    role: null as Role | null,
  }),

  getters: {
    isAuthenticated: (state) => !!state.user,
    hasPermission: (state) => (permission: string) => {
      return state.permissions.includes(permission) ||
             state.permissions.includes('*.*');
    },
    hasRole: (state) => (role: string) => {
      return state.role?.name === role;
    },
    isOwner: (state) => state.role?.name === 'Owner',
    isAdmin: (state) => ['Owner', 'Admin'].includes(state.role?.name || ''),
  },

  actions: {
    async login(email: string, password: string, companyId?: string) {
      const response = await $fetch('/api/auth/login', {
        method: 'POST',
        body: { email, password, companyId }
      });

      if (response.requiresCompanySelection) {
        this.companies = response.companies;
        return { requiresSelection: true };
      }

      this.setAuthData(response);
      return { requiresSelection: false };
    },

    async switchCompany(companyId: string) {
      const response = await $fetch('/api/auth/switch-company', {
        method: 'POST',
        body: { companyId }
      });

      this.setAuthData(response);
      this.$router.push('/');
    },

    setAuthData(data: LoginResponse) {
      this.user = data.user;
      this.currentCompany = data.company;
      this.permissions = data.permissions;
      this.role = data.role;

      // Store token
      const token = useCookie('auth_token');
      token.value = data.token;
    },
  }
});
```

### Permission Directive

```typescript
// plugins/permissions.ts
export default defineNuxtPlugin((nuxtApp) => {
  nuxtApp.vueApp.directive('permission', {
    mounted(el, binding) {
      const authStore = useAuthStore();
      const permission = binding.value;

      if (!authStore.hasPermission(permission)) {
        el.style.display = 'none';
      }
    }
  });
});
```

### Usage in Components

```vue
<template>
  <Button
    v-permission="'invoices.create'"
    @click="createInvoice"
    label="Create Invoice"
  />

  <Button
    v-if="authStore.hasPermission('users.manage')"
    @click="inviteUser"
    label="Invite User"
  />

  <div v-if="authStore.isAdmin">
    <!-- Admin-only content -->
  </div>
</template>

<script setup lang="ts">
const authStore = useAuthStore();
</script>
```

## Testing Considerations

### Unit Tests
- Permission service logic
- Role hierarchy validation
- JWT token generation with claims
- Multi-company context switching

### Integration Tests
- User registration with company creation
- Login with multiple companies
- Permission-based endpoint access
- Role assignment and updates
- Tenant isolation in authorization

### Test Data Scenarios
1. User with single company
2. User with multiple companies
3. User switching between companies
4. Permission checks across different roles
5. System role protection (cannot delete/modify)
6. Last owner protection

## Security Best Practices

1. **Never expose all permissions in JWT** - Include only essential claims
2. **Validate tenant context** - Always verify user belongs to tenant
3. **Cache permission checks** - Use JWT claims when possible
4. **Audit sensitive operations** - Log role changes, permission grants
5. **Rate limit auth endpoints** - Prevent brute force attacks
6. **Implement MFA for owners** - Additional security for privileged accounts
7. **Session management** - Track active sessions per user-company
8. **Permission checks on read operations** - Don't assume reads are always allowed

## Future Enhancements

- [ ] Hierarchical roles (role inheritance)
- [ ] Time-based role assignments (temporary access)
- [ ] Permission groups / categories
- [ ] Custom permission creation by tenants
- [ ] Approval workflows for sensitive permissions
- [ ] Audit log for all authorization events
- [ ] API rate limiting per role
- [ ] Field-level permissions (e.g., can view but not edit specific fields)
- [ ] Multi-factor authentication (MFA)
- [ ] IP whitelisting per company
- [ ] Session management dashboard
- [ ] Delegated administration (sub-admins)

## References

- **Related Agents**: Backend Agent, Frontend Agent, Project Architecture Agent
- **Key Files**:
  - `Domain/Entities/User.cs`, `Role.cs`, `Permission.cs`, `UserCompany.cs`, `RolePermission.cs`
  - `Application/Features/Auth/` - Commands and queries
  - `Infrastructure/Authorization/` - Policy handlers
  - `stores/auth.ts` - Frontend auth state
- **Documentation**:
  - [Microsoft Identity Documentation](https://learn.microsoft.com/en-us/aspnet/core/security/)
  - [JWT Best Practices](https://datatracker.ietf.org/doc/html/rfc8725)
