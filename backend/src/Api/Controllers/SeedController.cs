using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SaaS.Infrastructure.Persistence;
using SaaS.Domain.Entities;
using BCrypt.Net;

namespace SaaS.Api.Controllers;

/// <summary>
/// Controller for seeding demo/test data
/// WARNING: Only use in development! Remove or secure in production.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class SeedController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<SeedController> _logger;

    public SeedController(
        ApplicationDbContext context,
        IConfiguration configuration,
        ILogger<SeedController> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Seeds demo company with users and sample data
    /// POST /api/seed/demo
    /// </summary>
    [HttpPost("demo")]
    public async Task<IActionResult> SeedDemoData()
    {
        try
        {
            // Check if running in development
            var environment = _configuration["ASPNETCORE_ENVIRONMENT"];
            if (environment != "Development")
            {
                return BadRequest(new { message = "Demo seeding only available in Development environment" });
            }

            _logger.LogInformation("Starting demo data seeding...");

            // Check if demo company already exists
            var existingTenant = await _context.Tenants
                .FirstOrDefaultAsync(t => t.Slug == "demo-company");

            if (existingTenant != null)
            {
                return BadRequest(new { message = "Demo company already exists. Reset database first." });
            }

            var now = DateTime.UtcNow;
            var demoTenantId = Guid.NewGuid();

            // 1. Create Demo Tenant
            var demoTenant = new Tenant
            {
                Id = demoTenantId,
                Name = "Demo Company",
                Slug = "demo-company",
                Status = Domain.Enums.TenantStatus.Active,
                CreatedAt = now,
                UpdatedAt = now,
                IsDeleted = false
            };
            await _context.Tenants.AddAsync(demoTenant);

            // 2. Create Demo Users
            var passwordHash = BCrypt.Net.BCrypt.HashPassword("password");

            var ownerUser = new User
            {
                Id = Guid.NewGuid(),
                Name = "Demo Owner",
                Email = "owner@demo.com",
                PasswordHash = passwordHash,
                IsActive = true,
                EmailConfirmed = true,
                CreatedAt = now,
                UpdatedAt = now,
                IsDeleted = false
            };

            var adminUser = new User
            {
                Id = Guid.NewGuid(),
                Name = "Demo Admin",
                Email = "admin@demo.com",
                PasswordHash = passwordHash,
                IsActive = true,
                EmailConfirmed = true,
                CreatedAt = now,
                UpdatedAt = now,
                IsDeleted = false
            };

            var managerUser = new User
            {
                Id = Guid.NewGuid(),
                Name = "Demo Manager",
                Email = "manager@demo.com",
                PasswordHash = passwordHash,
                IsActive = true,
                EmailConfirmed = true,
                CreatedAt = now,
                UpdatedAt = now,
                IsDeleted = false
            };

            var regularUser = new User
            {
                Id = Guid.NewGuid(),
                Name = "Demo User",
                Email = "user@demo.com",
                PasswordHash = passwordHash,
                IsActive = true,
                EmailConfirmed = true,
                CreatedAt = now,
                UpdatedAt = now,
                IsDeleted = false
            };

            await _context.Users.AddRangeAsync(ownerUser, adminUser, managerUser, regularUser);
            await _context.SaveChangesAsync(); // Save users first

            // 3. Get or Create Roles for the demo tenant
            var ownerRole = await _context.Roles
                .FirstOrDefaultAsync(r => r.TenantId == demoTenantId && r.Name == "Owner");
            var adminRole = await _context.Roles
                .FirstOrDefaultAsync(r => r.TenantId == demoTenantId && r.Name == "Admin");
            var managerRole = await _context.Roles
                .FirstOrDefaultAsync(r => r.TenantId == demoTenantId && r.Name == "Manager");
            var userRole = await _context.Roles
                .FirstOrDefaultAsync(r => r.TenantId == demoTenantId && r.Name == "User");

            // Create roles if they don't exist (migration should create them, but this is a fallback)
            if (ownerRole == null)
            {
                ownerRole = new Role
                {
                    Id = Guid.NewGuid(),
                    TenantId = demoTenantId,
                    Name = "Owner",
                    Description = "Full system access",
                    Priority = 100,
                    IsSystemRole = true,
                    IsActive = true,
                    CreatedAt = now,
                    UpdatedAt = now
                };
                await _context.Roles.AddAsync(ownerRole);
            }
            if (adminRole == null)
            {
                adminRole = new Role
                {
                    Id = Guid.NewGuid(),
                    TenantId = demoTenantId,
                    Name = "Admin",
                    Description = "Administrative access",
                    Priority = 50,
                    IsSystemRole = true,
                    IsActive = true,
                    CreatedAt = now,
                    UpdatedAt = now
                };
                await _context.Roles.AddAsync(adminRole);
            }
            if (managerRole == null)
            {
                managerRole = new Role
                {
                    Id = Guid.NewGuid(),
                    TenantId = demoTenantId,
                    Name = "Manager",
                    Description = "Management access",
                    Priority = 25,
                    IsSystemRole = true,
                    IsActive = true,
                    CreatedAt = now,
                    UpdatedAt = now
                };
                await _context.Roles.AddAsync(managerRole);
            }
            if (userRole == null)
            {
                userRole = new Role
                {
                    Id = Guid.NewGuid(),
                    TenantId = demoTenantId,
                    Name = "User",
                    Description = "Standard user access",
                    Priority = 10,
                    IsSystemRole = true,
                    IsActive = true,
                    CreatedAt = now,
                    UpdatedAt = now
                };
                await _context.Roles.AddAsync(userRole);
            }

            await _context.SaveChangesAsync(); // Save roles

            // 3.5. Assign Permissions to Roles
            var allPermissions = await _context.Permissions.ToListAsync();

            // Owner: All permissions
            var ownerPermissions = allPermissions.Select(p => new RolePermission
            {
                Id = Guid.NewGuid(),
                RoleId = ownerRole.Id,
                PermissionId = p.Id
            }).ToList();
            await _context.RolePermissions.AddRangeAsync(ownerPermissions);

            // Admin: All permissions except tenant management
            var adminPermissions = allPermissions
                .Where(p => p.Resource != "tenants")
                .Select(p => new RolePermission
                {
                    Id = Guid.NewGuid(),
                    RoleId = adminRole.Id,
                    PermissionId = p.Id
                }).ToList();
            await _context.RolePermissions.AddRangeAsync(adminPermissions);

            // Manager: Warehouses, Products, Stock (all actions)
            var managerPermissions = allPermissions
                .Where(p => p.Resource == "warehouses" || p.Resource == "products" || p.Resource == "stock" || p.Resource == "roles")
                .Select(p => new RolePermission
                {
                    Id = Guid.NewGuid(),
                    RoleId = managerRole.Id,
                    PermissionId = p.Id
                }).ToList();
            await _context.RolePermissions.AddRangeAsync(managerPermissions);

            // User: Warehouses, Products, Stock (read-only)
            var userPermissions = allPermissions
                .Where(p => p.Action == "read" && (p.Resource == "warehouses" || p.Resource == "products" || p.Resource == "stock"))
                .Select(p => new RolePermission
                {
                    Id = Guid.NewGuid(),
                    RoleId = userRole.Id,
                    PermissionId = p.Id
                }).ToList();
            await _context.RolePermissions.AddRangeAsync(userPermissions);

            await _context.SaveChangesAsync(); // Save permissions

            // 4. Create UserTenant associations
            var userTenants = new List<UserTenant>
            {
                new UserTenant
                {
                    Id = Guid.NewGuid(),
                    UserId = ownerUser.Id,
                    TenantId = demoTenantId,
                    RoleId = ownerRole.Id,
                    IsActive = true,
                    JoinedAt = now
                },
                new UserTenant
                {
                    Id = Guid.NewGuid(),
                    UserId = adminUser.Id,
                    TenantId = demoTenantId,
                    RoleId = adminRole.Id,
                    IsActive = true,
                    JoinedAt = now
                },
                new UserTenant
                {
                    Id = Guid.NewGuid(),
                    UserId = managerUser.Id,
                    TenantId = demoTenantId,
                    RoleId = managerRole.Id,
                    IsActive = true,
                    JoinedAt = now
                },
                new UserTenant
                {
                    Id = Guid.NewGuid(),
                    UserId = regularUser.Id,
                    TenantId = demoTenantId,
                    RoleId = userRole.Id,
                    IsActive = true,
                    JoinedAt = now
                }
            };

            await _context.UserTenants.AddRangeAsync(userTenants);

            // 5. Create Sample Warehouses
            var warehouses = new List<Warehouse>
            {
                new Warehouse
                {
                    Id = Guid.NewGuid(),
                    TenantId = demoTenantId,
                    Name = "Main Warehouse",
                    Code = "WH-MAIN",
                    Description = "Primary distribution center",
                    StreetAddress = "123 Industrial Blvd",
                    City = "New York",
                    State = "NY",
                    PostalCode = "10001",
                    Country = "United States",
                    Phone = "+1 (555) 100-1000",
                    Email = "warehouse-main@demo.com",
                    SquareFootage = 50000,
                    Capacity = 10000,
                    IsActive = true,
                    CreatedAt = now,
                    UpdatedAt = now,
                    IsDeleted = false
                },
                new Warehouse
                {
                    Id = Guid.NewGuid(),
                    TenantId = demoTenantId,
                    Name = "West Coast Hub",
                    Code = "WH-WEST",
                    Description = "Western regional warehouse",
                    StreetAddress = "456 Commerce Way",
                    City = "Los Angeles",
                    State = "CA",
                    PostalCode = "90001",
                    Country = "United States",
                    Phone = "+1 (555) 200-2000",
                    Email = "warehouse-west@demo.com",
                    SquareFootage = 35000,
                    Capacity = 7000,
                    IsActive = true,
                    CreatedAt = now,
                    UpdatedAt = now,
                    IsDeleted = false
                },
                new Warehouse
                {
                    Id = Guid.NewGuid(),
                    TenantId = demoTenantId,
                    Name = "East Coast Depot",
                    Code = "WH-EAST",
                    Description = "Eastern regional storage facility",
                    StreetAddress = "789 Logistics Lane",
                    City = "Boston",
                    State = "MA",
                    PostalCode = "02101",
                    Country = "United States",
                    Phone = "+1 (555) 300-3000",
                    Email = "warehouse-east@demo.com",
                    SquareFootage = 40000,
                    Capacity = 8500,
                    IsActive = true,
                    CreatedAt = now,
                    UpdatedAt = now,
                    IsDeleted = false
                }
            };

            await _context.Warehouses.AddRangeAsync(warehouses);

            // Save all changes
            await _context.SaveChangesAsync();

            _logger.LogInformation("Demo data seeded successfully");

            return Ok(new
            {
                success = true,
                message = "Demo data seeded successfully",
                data = new
                {
                    tenant = new { id = demoTenant.Id, name = demoTenant.Name, slug = demoTenant.Slug },
                    users = new[]
                    {
                        new { email = "owner@demo.com", role = "Owner" },
                        new { email = "admin@demo.com", role = "Admin" },
                        new { email = "manager@demo.com", role = "Manager" },
                        new { email = "user@demo.com", role = "User" }
                    },
                    warehouses = warehouses.Count,
                    note = "All users have password: 'password'"
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding demo data");
            return StatusCode(500, new { success = false, message = "Error seeding demo data", error = ex.Message });
        }
    }
}
