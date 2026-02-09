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

            // Manager: Warehouses, Products, Customers, Stock (all actions)
            var managerPermissions = allPermissions
                .Where(p => p.Resource == "warehouses" || p.Resource == "products" || p.Resource == "customers" || p.Resource == "stock" || p.Resource == "roles")
                .Select(p => new RolePermission
                {
                    Id = Guid.NewGuid(),
                    RoleId = managerRole.Id,
                    PermissionId = p.Id
                }).ToList();
            await _context.RolePermissions.AddRangeAsync(managerPermissions);

            // User: Warehouses, Products, Customers, Stock (read-only)
            var userPermissions = allPermissions
                .Where(p => p.Action == "read" && (p.Resource == "warehouses" || p.Resource == "products" || p.Resource == "customers" || p.Resource == "stock"))
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

            // 6. Create Sample Products
            var products = new List<Product>
            {
                new Product
                {
                    Id = Guid.NewGuid(),
                    TenantId = demoTenantId,
                    Name = "Laptop - Dell XPS 15",
                    Code = "LAPTOP-DELL-XPS15",
                    SKU = "DELL-XPS-15-001",
                    Description = "High-performance laptop with 15.6\" display, Intel i7, 16GB RAM, 512GB SSD",
                    Category = "Electronics",
                    Brand = "Dell",
                    UnitPrice = 1499.99m,
                    CostPrice = 1200.00m,
                    MinimumStockLevel = 10,
                    CurrentStockLevel = 25,
                    Weight = 4.5m,
                    Dimensions = "14.0 x 9.3 x 0.7 inches",
                    IsActive = true,
                    CreatedAt = now,
                    UpdatedAt = now,
                    IsDeleted = false
                },
                new Product
                {
                    Id = Guid.NewGuid(),
                    TenantId = demoTenantId,
                    Name = "Wireless Mouse - Logitech MX Master 3",
                    Code = "MOUSE-LOG-MX3",
                    SKU = "LOG-MX-MASTER3-BLK",
                    Description = "Ergonomic wireless mouse with precision scrolling and customizable buttons",
                    Category = "Accessories",
                    Brand = "Logitech",
                    UnitPrice = 99.99m,
                    CostPrice = 65.00m,
                    MinimumStockLevel = 50,
                    CurrentStockLevel = 120,
                    Weight = 0.31m,
                    Dimensions = "4.9 x 3.3 x 2.0 inches",
                    IsActive = true,
                    CreatedAt = now,
                    UpdatedAt = now,
                    IsDeleted = false
                },
                new Product
                {
                    Id = Guid.NewGuid(),
                    TenantId = demoTenantId,
                    Name = "Monitor - LG 27\" 4K UHD",
                    Code = "MON-LG-27-4K",
                    SKU = "LG-27UK850-W",
                    Description = "27-inch 4K UHD IPS monitor with HDR support and USB-C connectivity",
                    Category = "Electronics",
                    Brand = "LG",
                    UnitPrice = 449.99m,
                    CostPrice = 350.00m,
                    MinimumStockLevel = 15,
                    CurrentStockLevel = 8,
                    Weight = 13.2m,
                    Dimensions = "24.1 x 18.5 x 7.9 inches",
                    IsActive = true,
                    CreatedAt = now,
                    UpdatedAt = now,
                    IsDeleted = false
                },
                new Product
                {
                    Id = Guid.NewGuid(),
                    TenantId = demoTenantId,
                    Name = "Keyboard - Mechanical RGB",
                    Code = "KEY-MECH-RGB-001",
                    SKU = "CORSAIR-K95-RGB",
                    Description = "Mechanical gaming keyboard with RGB backlighting and programmable macro keys",
                    Category = "Accessories",
                    Brand = "Corsair",
                    UnitPrice = 179.99m,
                    CostPrice = 120.00m,
                    MinimumStockLevel = 20,
                    CurrentStockLevel = 45,
                    Weight = 2.7m,
                    Dimensions = "17.4 x 6.5 x 1.5 inches",
                    IsActive = true,
                    CreatedAt = now,
                    UpdatedAt = now,
                    IsDeleted = false
                },
                new Product
                {
                    Id = Guid.NewGuid(),
                    TenantId = demoTenantId,
                    Name = "USB-C Hub - Anker 7-in-1",
                    Code = "HUB-ANKER-7IN1",
                    SKU = "ANKER-A8346",
                    Description = "7-in-1 USB-C hub with HDMI, ethernet, USB 3.0, and SD card reader",
                    Category = "Accessories",
                    Brand = "Anker",
                    UnitPrice = 49.99m,
                    CostPrice = 30.00m,
                    MinimumStockLevel = 30,
                    CurrentStockLevel = 75,
                    Weight = 0.22m,
                    Dimensions = "4.5 x 2.0 x 0.6 inches",
                    IsActive = true,
                    CreatedAt = now,
                    UpdatedAt = now,
                    IsDeleted = false
                }
            };

            await _context.Products.AddRangeAsync(products);

            // 7. Create Sample Customers
            var customers = new List<Customer>
            {
                new Customer
                {
                    Id = Guid.NewGuid(),
                    TenantId = demoTenantId,
                    Name = "Acme Corporation",
                    Email = "contact@acmecorp.example.com",
                    Phone = "+1 (555) 123-4567",
                    TaxId = "TAX-ACME-001",
                    ContactPerson = "John Smith",
                    BillingStreet = "123 Business Ave",
                    BillingCity = "New York",
                    BillingState = "NY",
                    BillingPostalCode = "10001",
                    BillingCountry = "United States",
                    ShippingStreet = "123 Business Ave",
                    ShippingCity = "New York",
                    ShippingState = "NY",
                    ShippingPostalCode = "10001",
                    ShippingCountry = "United States",
                    Website = "https://acmecorp.example.com",
                    Notes = "Major client - priority shipping",
                    IsActive = true,
                    CreatedAt = now,
                    UpdatedAt = now,
                    IsDeleted = false
                },
                new Customer
                {
                    Id = Guid.NewGuid(),
                    TenantId = demoTenantId,
                    Name = "Tech Solutions Inc",
                    Email = "sales@techsolutions.example.com",
                    Phone = "+1 (555) 234-5678",
                    TaxId = "TAX-TECH-002",
                    ContactPerson = "Sarah Johnson",
                    BillingStreet = "456 Innovation Dr",
                    BillingCity = "San Francisco",
                    BillingState = "CA",
                    BillingPostalCode = "94105",
                    BillingCountry = "United States",
                    ShippingStreet = "456 Innovation Dr",
                    ShippingCity = "San Francisco",
                    ShippingState = "CA",
                    ShippingPostalCode = "94105",
                    ShippingCountry = "United States",
                    Website = "https://techsolutions.example.com",
                    Notes = "Net 30 payment terms",
                    IsActive = true,
                    CreatedAt = now,
                    UpdatedAt = now,
                    IsDeleted = false
                },
                new Customer
                {
                    Id = Guid.NewGuid(),
                    TenantId = demoTenantId,
                    Name = "Global Enterprises LLC",
                    Email = "procurement@globalent.example.com",
                    Phone = "+1 (555) 345-6789",
                    TaxId = "TAX-GLOBAL-003",
                    ContactPerson = "Michael Chen",
                    BillingStreet = "789 Commerce Blvd",
                    BillingCity = "Chicago",
                    BillingState = "IL",
                    BillingPostalCode = "60601",
                    BillingCountry = "United States",
                    ShippingStreet = "321 Distribution Way",
                    ShippingCity = "Chicago",
                    ShippingState = "IL",
                    ShippingPostalCode = "60602",
                    ShippingCountry = "United States",
                    Website = "https://globalenterprises.example.com",
                    Notes = "Requires detailed invoicing",
                    IsActive = true,
                    CreatedAt = now,
                    UpdatedAt = now,
                    IsDeleted = false
                },
                new Customer
                {
                    Id = Guid.NewGuid(),
                    TenantId = demoTenantId,
                    Name = "Retail Partners Co",
                    Email = "orders@retailpartners.example.com",
                    Phone = "+1 (555) 456-7890",
                    TaxId = "TAX-RETAIL-004",
                    ContactPerson = "Emily Rodriguez",
                    BillingStreet = "555 Retail Plaza",
                    BillingCity = "Miami",
                    BillingState = "FL",
                    BillingPostalCode = "33101",
                    BillingCountry = "United States",
                    ShippingStreet = "555 Retail Plaza",
                    ShippingCity = "Miami",
                    ShippingState = "FL",
                    ShippingPostalCode = "33101",
                    ShippingCountry = "United States",
                    Website = "https://retailpartners.example.com",
                    Notes = "Bulk orders - discount applied",
                    IsActive = true,
                    CreatedAt = now,
                    UpdatedAt = now,
                    IsDeleted = false
                },
                new Customer
                {
                    Id = Guid.NewGuid(),
                    TenantId = demoTenantId,
                    Name = "Startup Innovations",
                    Email = "hello@startupinnovations.example.com",
                    Phone = "+1 (555) 567-8901",
                    TaxId = "TAX-STARTUP-005",
                    ContactPerson = "David Park",
                    BillingStreet = "100 Startup Lane",
                    BillingCity = "Austin",
                    BillingState = "TX",
                    BillingPostalCode = "78701",
                    BillingCountry = "United States",
                    ShippingStreet = "100 Startup Lane",
                    ShippingCity = "Austin",
                    ShippingState = "TX",
                    ShippingPostalCode = "78701",
                    ShippingCountry = "United States",
                    Website = "https://startupinnovations.example.com",
                    Notes = "Growing account - good potential",
                    IsActive = true,
                    CreatedAt = now,
                    UpdatedAt = now,
                    IsDeleted = false
                }
            };

            await _context.Customers.AddRangeAsync(customers);

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
                    products = products.Count,
                    customers = customers.Count,
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
