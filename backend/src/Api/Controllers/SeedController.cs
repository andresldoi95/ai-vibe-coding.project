using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SaaS.Infrastructure.Persistence;
using SaaS.Domain.Entities;
using SaaS.Domain.Enums;
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
    /// Seeds multiple demo companies with shared users and comprehensive sample data
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

            _logger.LogInformation("Starting multi-tenant demo data seeding...");

            // Check if any demo companies already exist
            var allTenants = await _context.Tenants.ToListAsync();
            _logger.LogInformation($"Total tenants in database: {allTenants.Count}");

            var existingTenants = await _context.Tenants
                .Where(t => t.Slug == "demo-company" || t.Slug == "tech-startup" || t.Slug == "manufacturing-corp")
                .AnyAsync();

            _logger.LogInformation($"Existing demo tenants found: {existingTenants}");

            if (existingTenants)
            {
                return BadRequest(new { message = "Demo companies already exist. Reset database first." });
            }

            var now = DateTime.UtcNow;

            // 1. Seed Countries
            var usCountry = new Country
            {
                Id = Guid.NewGuid(),
                Code = "US",
                Name = "United States",
                Alpha3Code = "USA",
                NumericCode = "840",
                IsActive = true
            };

            var ecuadorCountry = new Country
            {
                Id = Guid.NewGuid(),
                Code = "EC",
                Name = "Ecuador",
                Alpha3Code = "ECU",
                NumericCode = "218",
                IsActive = true
            };

            await _context.Countries.AddRangeAsync(usCountry, ecuadorCountry);
            await _context.SaveChangesAsync();

            // 2. Create Demo Tenants
            var demoCompany = CreateTenant("Demo Company", "demo-company", now);
            var techStartup = CreateTenant("Tech Startup Inc", "tech-startup", now);
            var manufacturingCorp = CreateTenant("Manufacturing Corp", "manufacturing-corp", now);

            await _context.Tenants.AddRangeAsync(demoCompany, techStartup, manufacturingCorp);

            await _context.Tenants.AddRangeAsync(demoCompany, techStartup, manufacturingCorp);

            // 2. Create Demo Users (shared across all tenants)
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
            await _context.SaveChangesAsync(); // Save tenants and users first

            // 3. Create Roles and Permissions for all tenants
            var tenants = new[] { demoCompany, techStartup, manufacturingCorp };
            var rolesByTenant = new Dictionary<Guid, Dictionary<string, Role>>();

            foreach (var tenant in tenants)
            {
                var roles = await CreateRolesForTenant(tenant.Id, now);
                rolesByTenant[tenant.Id] = roles;
            }

            await _context.SaveChangesAsync(); // Save all roles

            // 4. Create UserTenant associations - each user has same role across all companies for consistency
            var userTenants = new List<UserTenant>
            {
                // owner@demo.com: Owner in all companies
                new UserTenant { Id = Guid.NewGuid(), UserId = ownerUser.Id, TenantId = demoCompany.Id,
                    RoleId = rolesByTenant[demoCompany.Id]["Owner"].Id, IsActive = true, JoinedAt = now },
                new UserTenant { Id = Guid.NewGuid(), UserId = ownerUser.Id, TenantId = techStartup.Id,
                    RoleId = rolesByTenant[techStartup.Id]["Owner"].Id, IsActive = true, JoinedAt = now },
                new UserTenant { Id = Guid.NewGuid(), UserId = ownerUser.Id, TenantId = manufacturingCorp.Id,
                    RoleId = rolesByTenant[manufacturingCorp.Id]["Owner"].Id, IsActive = true, JoinedAt = now },

                // admin@demo.com: Admin in all companies
                new UserTenant { Id = Guid.NewGuid(), UserId = adminUser.Id, TenantId = demoCompany.Id,
                    RoleId = rolesByTenant[demoCompany.Id]["Admin"].Id, IsActive = true, JoinedAt = now },
                new UserTenant { Id = Guid.NewGuid(), UserId = adminUser.Id, TenantId = techStartup.Id,
                    RoleId = rolesByTenant[techStartup.Id]["Admin"].Id, IsActive = true, JoinedAt = now },
                new UserTenant { Id = Guid.NewGuid(), UserId = adminUser.Id, TenantId = manufacturingCorp.Id,
                    RoleId = rolesByTenant[manufacturingCorp.Id]["Admin"].Id, IsActive = true, JoinedAt = now },

                // manager@demo.com: Manager in all companies
                new UserTenant { Id = Guid.NewGuid(), UserId = managerUser.Id, TenantId = demoCompany.Id,
                    RoleId = rolesByTenant[demoCompany.Id]["Manager"].Id, IsActive = true, JoinedAt = now },
                new UserTenant { Id = Guid.NewGuid(), UserId = managerUser.Id, TenantId = techStartup.Id,
                    RoleId = rolesByTenant[techStartup.Id]["Manager"].Id, IsActive = true, JoinedAt = now },
                new UserTenant { Id = Guid.NewGuid(), UserId = managerUser.Id, TenantId = manufacturingCorp.Id,
                    RoleId = rolesByTenant[manufacturingCorp.Id]["Manager"].Id, IsActive = true, JoinedAt = now },

                // user@demo.com: User in all companies
                new UserTenant { Id = Guid.NewGuid(), UserId = regularUser.Id, TenantId = demoCompany.Id,
                    RoleId = rolesByTenant[demoCompany.Id]["User"].Id, IsActive = true, JoinedAt = now },
                new UserTenant { Id = Guid.NewGuid(), UserId = regularUser.Id, TenantId = techStartup.Id,
                    RoleId = rolesByTenant[techStartup.Id]["User"].Id, IsActive = true, JoinedAt = now },
                new UserTenant { Id = Guid.NewGuid(), UserId = regularUser.Id, TenantId = manufacturingCorp.Id,
                    RoleId = rolesByTenant[manufacturingCorp.Id]["User"].Id, IsActive = true, JoinedAt = now }
            };

            await _context.UserTenants.AddRangeAsync(userTenants);
            await _context.SaveChangesAsync();

            // 5. Create data for each tenant
            var summaries = new List<object>();

            foreach (var tenant in tenants)
            {
                _logger.LogInformation($"Seeding data for tenant: {tenant.Name}");

                // Seed Tax Rates (Ecuador IVA rates)
                var taxRates = CreateTaxRatesForTenant(tenant.Id, now, ecuadorCountry);
                await _context.TaxRates.AddRangeAsync(taxRates);
                await _context.SaveChangesAsync();

                // TODO: Seed Invoice Configuration once entity is created
                // var invoiceConfig = CreateInvoiceConfigurationForTenant(tenant.Id, taxRates, now);
                // await _context.InvoiceConfigurations.AddAsync(invoiceConfig);
                // await _context.SaveChangesAsync();

                // Seed Establishments (Ecuador SRI requirement)
                var establishments = CreateEstablishmentsForTenant(tenant.Id, tenant.Slug, now);
                await _context.Establishments.AddRangeAsync(establishments);
                await _context.SaveChangesAsync();

                // Seed Emission Points (Ecuador SRI requirement)
                var emissionPoints = CreateEmissionPointsForTenant(tenant.Id, establishments, now);
                await _context.EmissionPoints.AddRangeAsync(emissionPoints);
                await _context.SaveChangesAsync();

                var warehouses = CreateWarehousesForTenant(tenant.Id, tenant.Slug, now, usCountry);
                await _context.Warehouses.AddRangeAsync(warehouses);
                await _context.SaveChangesAsync();

                var products = CreateProductsForTenant(tenant.Id, tenant.Slug, now);
                await _context.Products.AddRangeAsync(products);
                await _context.SaveChangesAsync();

                var customers = CreateCustomersForTenant(tenant.Id, tenant.Slug, now, usCountry);
                await _context.Customers.AddRangeAsync(customers);
                await _context.SaveChangesAsync();

                var stockMovements = CreateStockMovementsForTenant(tenant.Id, products, warehouses, now);
                await _context.StockMovements.AddRangeAsync(stockMovements);
                await _context.SaveChangesAsync();

                var inventoryLevels = CalculateWarehouseInventory(tenant.Id, products, warehouses, stockMovements, now);
                await _context.WarehouseInventory.AddRangeAsync(inventoryLevels);
                await _context.SaveChangesAsync();

                // Seed Invoices
                var invoices = CreateInvoicesForTenant(tenant.Id, customers, warehouses, emissionPoints, taxRates, now);
                await _context.Invoices.AddRangeAsync(invoices);
                await _context.SaveChangesAsync();

                // Seed Invoice Items
                var invoiceItems = CreateInvoiceItemsForInvoices(invoices, products, taxRates, now);
                await _context.InvoiceItems.AddRangeAsync(invoiceItems);
                await _context.SaveChangesAsync();

                // Seed SRI Error Logs
                var sriErrorLogs = CreateSriErrorLogsForInvoices(invoices, now);
                await _context.SriErrorLogs.AddRangeAsync(sriErrorLogs);
                await _context.SaveChangesAsync();

                summaries.Add(new
                {
                    tenant = new { id = tenant.Id, name = tenant.Name, slug = tenant.Slug },
                    taxRates = taxRates.Count,
                    invoiceConfiguration = 1,
                    establishments = establishments.Count,
                    emissionPoints = emissionPoints.Count,
                    warehouses = warehouses.Count,
                    products = products.Count,
                    customers = customers.Count,
                    stockMovements = stockMovements.Count,
                    inventoryRecords = inventoryLevels.Count,
                    invoices = invoices.Count,
                    invoiceItems = invoiceItems.Count,
                    sriErrorLogs = sriErrorLogs.Count
                });
            }

            _logger.LogInformation("Multi-tenant demo data seeded successfully");

            return Ok(new
            {
                success = true,
                message = "Multi-tenant demo data seeded successfully",
                tenants = summaries,
                users = new[]
                {
                    new { email = "owner@demo.com", password = "password", role = "Owner", companies = "All companies" },
                    new { email = "admin@demo.com", password = "password", role = "Admin", companies = "All companies" },
                    new { email = "manager@demo.com", password = "password", role = "Manager", companies = "All companies" },
                    new { email = "user@demo.com", password = "password", role = "User", companies = "All companies" }
                },
                note = "All users have consistent roles across all three demo companies"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding demo data");
            return StatusCode(500, new { success = false, message = "Error seeding demo data", error = ex.Message });
        }
    }

    #region Helper Methods

    private Tenant CreateTenant(string name, string slug, DateTime now)
    {
        return new Tenant
        {
            Id = Guid.NewGuid(),
            Name = name,
            Slug = slug,
            Status = TenantStatus.Active,
            CreatedAt = now,
            UpdatedAt = now,
            IsDeleted = false
        };
    }

    private async Task<Dictionary<string, Role>> CreateRolesForTenant(Guid tenantId, DateTime now)
    {
        var allPermissions = await _context.Permissions.ToListAsync();

        var ownerRole = new Role
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = "Owner",
            Description = "Full system access",
            Priority = 100,
            IsSystemRole = true,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };

        var adminRole = new Role
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = "Admin",
            Description = "Administrative access",
            Priority = 50,
            IsSystemRole = true,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };

        var managerRole = new Role
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = "Manager",
            Description = "Management access",
            Priority = 25,
            IsSystemRole = true,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };

        var userRole = new Role
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = "User",
            Description = "Standard user access",
            Priority = 10,
            IsSystemRole = true,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };

        await _context.Roles.AddRangeAsync(ownerRole, adminRole, managerRole, userRole);

        // Owner: All permissions
        var ownerPermissions = allPermissions.Select(p => new RolePermission
        {
            Id = Guid.NewGuid(),
            RoleId = ownerRole.Id,
            PermissionId = p.Id
        }).ToList();
        await _context.RolePermissions.AddRangeAsync(ownerPermissions);

        // Admin: All permissions except tenant deletion
        var adminPermissions = allPermissions
            .Where(p => !(p.Resource == "tenants" && p.Action == "delete"))
            .Select(p => new RolePermission
            {
                Id = Guid.NewGuid(),
                RoleId = adminRole.Id,
                PermissionId = p.Id
            }).ToList();
        await _context.RolePermissions.AddRangeAsync(adminPermissions);

        // Manager: Full access to warehouses, products, customers, stock, establishments, emission_points
        // Read/create/update/send/export/manage for invoices, read/create/update/void/complete for payments, read/update for tax-rates, read for invoice-config, sri_configuration
        // Read-only access to roles
        var managerPermissions = allPermissions
            .Where(p =>
                new[] { "warehouses", "products", "customers", "stock", "establishments", "emission_points" }.Contains(p.Resource) ||
                (p.Resource == "invoices" && new[] { "read", "create", "update", "send", "export", "manage" }.Contains(p.Action)) ||
                (p.Resource == "payments" && new[] { "read", "create", "update", "void", "complete" }.Contains(p.Action)) ||
                (p.Resource == "tax-rates" && new[] { "read", "create", "update" }.Contains(p.Action)) ||
                (p.Resource == "invoice-config" && new[] { "read", "update" }.Contains(p.Action)) ||
                (p.Resource == "sri_configuration" && new[] { "read", "update" }.Contains(p.Action)) ||
                (p.Resource == "roles" && p.Action == "read"))
            .Select(p => new RolePermission
            {
                Id = Guid.NewGuid(),
                RoleId = managerRole.Id,
                PermissionId = p.Id
            }).ToList();
        await _context.RolePermissions.AddRangeAsync(managerPermissions);

        // User: Read-only access
        var userPermissions = allPermissions
            .Where(p => p.Action == "read")
            .Select(p => new RolePermission
            {
                Id = Guid.NewGuid(),
                RoleId = userRole.Id,
                PermissionId = p.Id
            }).ToList();
        await _context.RolePermissions.AddRangeAsync(userPermissions);

        return new Dictionary<string, Role>
        {
            { "Owner", ownerRole },
            { "Admin", adminRole },
            { "Manager", managerRole },
            { "User", userRole }
        };
    }

    private List<Warehouse> CreateWarehousesForTenant(Guid tenantId, string slug, DateTime now, Country usCountry)
    {
        var prefix = slug.ToUpper().Replace("-", "").Substring(0, Math.Min(4, slug.Length));

        var locations = slug switch
        {
            "demo-company" => new[]
            {
                ("Main Warehouse", "MAIN", "123 Industrial Blvd", "New York", "NY", "10001", "+1 (555) 100-1000"),
                ("West Coast Hub", "WEST", "456 Commerce Way", "Los Angeles", "CA", "90001", "+1 (555) 200-2000"),
                ("East Coast Depot", "EAST", "789 Logistics Lane", "Boston", "MA", "02101", "+1 (555) 300-3000")
            },
            "tech-startup" => new[]
            {
                ("San Francisco HQ", "SF", "100 Tech Plaza", "San Francisco", "CA", "94105", "+1 (555) 400-4000"),
                ("Seattle Distribution", "SEA", "200 Innovation Way", "Seattle", "WA", "98101", "+1 (555) 500-5000"),
                ("Austin Fulfillment", "AUS", "300 Startup Ave", "Austin", "TX", "78701", "+1 (555) 600-6000")
            },
            "manufacturing-corp" => new[]
            {
                ("Chicago Plant", "CHI", "400 Factory Road", "Chicago", "IL", "60601", "+1 (555) 700-7000"),
                ("Detroit Assembly", "DET", "500 Manufacturing Dr", "Detroit", "MI", "48201", "+1 (555) 800-8000"),
                ("Houston Storage", "HOU", "600 Industrial Pkwy", "Houston", "TX", "77001", "+1 (555) 900-9000")
            },
            _ => throw new ArgumentException($"Unknown tenant slug: {slug}")
        };

        var warehouses = new List<Warehouse>();
        for (int i = 0; i < locations.Length; i++)
        {
            var loc = locations[i];
            warehouses.Add(new Warehouse
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Name = loc.Item1,
                Code = $"WH-{prefix}-{loc.Item2}",
                Description = $"{loc.Item1} for {slug}",
                StreetAddress = loc.Item3,
                City = loc.Item4,
                State = loc.Item5,
                PostalCode = loc.Item6,
                CountryId = usCountry.Id,
                Phone = loc.Item7,
                Email = $"warehouse-{loc.Item2.ToLower()}@{slug}.com",
                SquareFootage = 50000 - (i * 5000),
                Capacity = 10000 - (i * 1000),
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now,
                IsDeleted = false
            });
        }

        return warehouses;
    }

    private List<Product> CreateProductsForTenant(Guid tenantId, string slug, DateTime now)
    {
        var prefix = slug.ToUpper().Replace("-", "");

        return new List<Product>
        {
            new Product
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Name = "Laptop - Dell XPS 15",
                Code = $"PROD-{prefix}-001",
                SKU = $"{prefix}-LAPTOP-001",
                Description = "High-performance laptop with 15.6\" display, Intel i7, 16GB RAM, 512GB SSD",
                Category = "Electronics",
                Brand = "Dell",
                UnitPrice = 1499.99m,
                CostPrice = 1200.00m,
                MinimumStockLevel = 10,
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
                TenantId = tenantId,
                Name = "Wireless Mouse - Logitech MX Master 3",
                Code = $"PROD-{prefix}-002",
                SKU = $"{prefix}-MOUSE-001",
                Description = "Ergonomic wireless mouse with precision scrolling and customizable buttons",
                Category = "Accessories",
                Brand = "Logitech",
                UnitPrice = 99.99m,
                CostPrice = 65.00m,
                MinimumStockLevel = 50,
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
                TenantId = tenantId,
                Name = "Monitor - LG 27\" 4K UHD",
                Code = $"PROD-{prefix}-003",
                SKU = $"{prefix}-MONITOR-001",
                Description = "27-inch 4K UHD IPS monitor with HDR support and USB-C connectivity",
                Category = "Electronics",
                Brand = "LG",
                UnitPrice = 449.99m,
                CostPrice = 350.00m,
                MinimumStockLevel = 15,
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
                TenantId = tenantId,
                Name = "Keyboard - Mechanical RGB",
                Code = $"PROD-{prefix}-004",
                SKU = $"{prefix}-KEYBOARD-001",
                Description = "Mechanical gaming keyboard with RGB backlighting and programmable macro keys",
                Category = "Accessories",
                Brand = "Corsair",
                UnitPrice = 179.99m,
                CostPrice = 120.00m,
                MinimumStockLevel = 20,
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
                TenantId = tenantId,
                Name = "USB-C Hub - Anker 7-in-1",
                Code = $"PROD-{prefix}-005",
                SKU = $"{prefix}-HUB-001",
                Description = "7-in-1 USB-C hub with HDMI, ethernet, USB 3.0, and SD card reader",
                Category = "Accessories",
                Brand = "Anker",
                UnitPrice = 49.99m,
                CostPrice = 30.00m,
                MinimumStockLevel = 30,
                Weight = 0.22m,
                Dimensions = "4.5 x 2.0 x 0.6 inches",
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now,
                IsDeleted = false
            }
        };
    }

    private List<Customer> CreateCustomersForTenant(Guid tenantId, string slug, DateTime now, Country usCountry)
    {
        var customerSuffix = slug switch
        {
            "demo-company" => "NYC",
            "tech-startup" => "SF",
            "manufacturing-corp" => "CHI",
            _ => ""
        };

        return new List<Customer>
        {
            new Customer
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Name = $"Acme Corporation {customerSuffix}",
                Email = $"contact@acme-{slug}.example.com",
                Phone = "+1 (555) 123-4567",
                IdentificationType = IdentificationType.Ruc,
                TaxId = "1790123456001", // Valid Ecuador RUC format
                ContactPerson = "John Smith",
                BillingStreet = "123 Business Ave",
                BillingCity = "New York",
                BillingState = "NY",
                BillingPostalCode = "10001",
                BillingCountryId = usCountry.Id,
                ShippingStreet = "123 Business Ave",
                ShippingCity = "New York",
                ShippingState = "NY",
                ShippingPostalCode = "10001",
                ShippingCountryId = usCountry.Id,
                Website = $"https://acme-{slug}.example.com",
                Notes = "Major client - priority shipping",
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now,
                IsDeleted = false
            },
            new Customer
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Name = $"Tech Solutions Inc {customerSuffix}",
                Email = $"sales@techsolutions-{slug}.example.com",
                Phone = "+1 (555) 234-5678",
                IdentificationType = IdentificationType.Ruc,
                TaxId = "1791234567001", // Valid Ecuador RUC format
                ContactPerson = "Sarah Johnson",
                BillingStreet = "456 Innovation Dr",
                BillingCity = "San Francisco",
                BillingState = "CA",
                BillingPostalCode = "94105",
                BillingCountryId = usCountry.Id,
                ShippingStreet = "456 Innovation Dr",
                ShippingCity = "San Francisco",
                ShippingState = "CA",
                ShippingPostalCode = "94105",
                ShippingCountryId = usCountry.Id,
                Website = $"https://techsolutions-{slug}.example.com",
                Notes = "Net 30 payment terms",
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now,
                IsDeleted = false
            },
            new Customer
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Name = $"Global Enterprises LLC {customerSuffix}",
                Email = $"procurement@globalent-{slug}.example.com",
                Phone = "+1 (555) 345-6789",
                IdentificationType = IdentificationType.Cedula,
                TaxId = "1712345678", // Valid Ecuador Cédula format (10 digits)
                ContactPerson = "Michael Chen",
                BillingStreet = "789 Commerce Blvd",
                BillingCity = "Chicago",
                BillingState = "IL",
                BillingPostalCode = "60601",
                BillingCountryId = usCountry.Id,
                ShippingStreet = "321 Distribution Way",
                ShippingCity = "Chicago",
                ShippingState = "IL",
                ShippingPostalCode = "60602",
                ShippingCountryId = usCountry.Id,
                Website = $"https://globalent-{slug}.example.com",
                Notes = "Requires detailed invoicing",
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now,
                IsDeleted = false
            },
            new Customer
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Name = $"Retail Partners Co {customerSuffix}",
                Email = $"orders@retailpartners-{slug}.example.com",
                Phone = "+1 (555) 456-7890",
                IdentificationType = IdentificationType.Passport,
                TaxId = "AB123456", // Passport format
                ContactPerson = "Emily Rodriguez",
                BillingStreet = "555 Retail Plaza",
                BillingCity = "Miami",
                BillingState = "FL",
                BillingPostalCode = "33101",
                BillingCountryId = usCountry.Id,
                ShippingStreet = "555 Retail Plaza",
                ShippingCity = "Miami",
                ShippingState = "FL",
                ShippingPostalCode = "33101",
                ShippingCountryId = usCountry.Id,
                Website = $"https://retailpartners-{slug}.example.com",
                Notes = "Bulk orders - discount applied",
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now,
                IsDeleted = false
            },
            new Customer
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Name = $"Startup Innovations {customerSuffix}",
                Email = $"hello@startupinnovations-{slug}.example.com",
                Phone = "+1 (555) 567-8901",
                IdentificationType = IdentificationType.ConsumerFinal,
                TaxId = "9999999999999", // Consumidor Final
                ContactPerson = "David Park",
                BillingStreet = "100 Startup Lane",
                BillingCity = "Austin",
                BillingState = "TX",
                BillingPostalCode = "78701",
                BillingCountryId = usCountry.Id,
                ShippingStreet = "100 Startup Lane",
                ShippingCity = "Austin",
                ShippingState = "TX",
                ShippingPostalCode = "78701",
                ShippingCountryId = usCountry.Id,
                Website = $"https://startupinnovations-{slug}.example.com",
                Notes = "Growing account - good potential",
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now,
                IsDeleted = false
            }
        };
    }

    private List<StockMovement> CreateStockMovementsForTenant(
        Guid tenantId,
        List<Product> products,
        List<Warehouse> warehouses,
        DateTime now)
    {
        var movements = new List<StockMovement>();
        var random = new Random(tenantId.GetHashCode()); // Deterministic randomness per tenant

        // Initial Inventory (90 days ago) - Set starting stock in first warehouse
        foreach (var product in products)
        {
            movements.Add(new StockMovement
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                ProductId = product.Id,
                WarehouseId = warehouses[0].Id,
                MovementType = MovementType.InitialInventory,
                Quantity = random.Next(50, 200),
                UnitCost = product.CostPrice,
                TotalCost = product.CostPrice * random.Next(50, 200),
                Reference = $"INIT-{product.Code}",
                Notes = "Initial inventory setup",
                MovementDate = now.AddDays(-90),
                CreatedAt = now.AddDays(-90),
                UpdatedAt = now.AddDays(-90),
                IsDeleted = false
            });
        }

        // Purchases (30-60 days ago) - Incoming stock to various warehouses
        for (int i = 0; i < 8; i++)
        {
            var product = products[random.Next(products.Count)];
            var warehouse = warehouses[random.Next(warehouses.Count)];
            var quantity = random.Next(20, 100);

            movements.Add(new StockMovement
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                ProductId = product.Id,
                WarehouseId = warehouse.Id,
                MovementType = MovementType.Purchase,
                Quantity = quantity,
                UnitCost = product.CostPrice,
                TotalCost = product.CostPrice * quantity,
                Reference = $"PO-{random.Next(1000, 9999)}",
                Notes = $"Purchase order received",
                MovementDate = now.AddDays(-random.Next(30, 60)),
                CreatedAt = now.AddDays(-random.Next(30, 60)),
                UpdatedAt = now.AddDays(-random.Next(30, 60)),
                IsDeleted = false
            });
        }

        // Sales (last 30 days) - Outgoing stock
        for (int i = 0; i < 12; i++)
        {
            var product = products[random.Next(products.Count)];
            var warehouse = warehouses[random.Next(warehouses.Count)];
            var quantity = -random.Next(5, 25); // Negative for outgoing

            movements.Add(new StockMovement
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                ProductId = product.Id,
                WarehouseId = warehouse.Id,
                MovementType = MovementType.Sale,
                Quantity = quantity,
                Reference = $"INV-{random.Next(1000, 9999)}",
                Notes = $"Customer order fulfillment",
                MovementDate = now.AddDays(-random.Next(1, 30)),
                CreatedAt = now.AddDays(-random.Next(1, 30)),
                UpdatedAt = now.AddDays(-random.Next(1, 30)),
                IsDeleted = false
            });
        }

        // Transfers (last 45 days) - Between warehouses
        if (warehouses.Count >= 2)
        {
            for (int i = 0; i < 5; i++)
            {
                var product = products[random.Next(products.Count)];
                var sourceWarehouse = warehouses[random.Next(warehouses.Count)];
                var destWarehouse = warehouses.First(w => w.Id != sourceWarehouse.Id);
                var quantity = random.Next(10, 30);

                // Outgoing from source
                movements.Add(new StockMovement
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    ProductId = product.Id,
                    WarehouseId = sourceWarehouse.Id,
                    DestinationWarehouseId = destWarehouse.Id,
                    MovementType = MovementType.Transfer,
                    Quantity = -quantity,
                    Reference = $"TRF-{random.Next(1000, 9999)}",
                    Notes = $"Transfer to {destWarehouse.Name}",
                    MovementDate = now.AddDays(-random.Next(1, 45)),
                    CreatedAt = now.AddDays(-random.Next(1, 45)),
                    UpdatedAt = now.AddDays(-random.Next(1, 45)),
                    IsDeleted = false
                });

                // Incoming to destination
                movements.Add(new StockMovement
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    ProductId = product.Id,
                    WarehouseId = destWarehouse.Id,
                    DestinationWarehouseId = sourceWarehouse.Id,
                    MovementType = MovementType.Transfer,
                    Quantity = quantity,
                    Reference = $"TRF-{random.Next(1000, 9999)}",
                    Notes = $"Transfer from {sourceWarehouse.Name}",
                    MovementDate = now.AddDays(-random.Next(1, 45)),
                    CreatedAt = now.AddDays(-random.Next(1, 45)),
                    UpdatedAt = now.AddDays(-random.Next(1, 45)),
                    IsDeleted = false
                });
            }
        }

        // Adjustments (last 15 days) - Stock corrections
        for (int i = 0; i < 3; i++)
        {
            var product = products[random.Next(products.Count)];
            var warehouse = warehouses[random.Next(warehouses.Count)];
            var quantity = random.Next(-10, 10); // Can be positive or negative

            movements.Add(new StockMovement
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                ProductId = product.Id,
                WarehouseId = warehouse.Id,
                MovementType = MovementType.Adjustment,
                Quantity = quantity,
                Reference = $"ADJ-{random.Next(1000, 9999)}",
                Notes = quantity > 0 ? "Stock found during audit" : "Damaged items removed",
                MovementDate = now.AddDays(-random.Next(1, 15)),
                CreatedAt = now.AddDays(-random.Next(1, 15)),
                UpdatedAt = now.AddDays(-random.Next(1, 15)),
                IsDeleted = false
            });
        }

        // Returns (last 20 days) - Customer returns
        for (int i = 0; i < 2; i++)
        {
            var product = products[random.Next(products.Count)];
            var warehouse = warehouses[random.Next(warehouses.Count)];
            var quantity = random.Next(1, 5);

            movements.Add(new StockMovement
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                ProductId = product.Id,
                WarehouseId = warehouse.Id,
                MovementType = MovementType.Return,
                Quantity = quantity,
                Reference = $"RET-{random.Next(1000, 9999)}",
                Notes = "Customer return - item defective",
                MovementDate = now.AddDays(-random.Next(1, 20)),
                CreatedAt = now.AddDays(-random.Next(1, 20)),
                UpdatedAt = now.AddDays(-random.Next(1, 20)),
                IsDeleted = false
            });
        }

        return movements;
    }

    private List<WarehouseInventory> CalculateWarehouseInventory(
        Guid tenantId,
        List<Product> products,
        List<Warehouse> warehouses,
        List<StockMovement> movements,
        DateTime now)
    {
        var inventoryLevels = new List<WarehouseInventory>();

        foreach (var product in products)
        {
            foreach (var warehouse in warehouses)
            {
                var productMovements = movements
                    .Where(m => m.ProductId == product.Id && m.WarehouseId == warehouse.Id)
                    .ToList();

                if (productMovements.Any())
                {
                    var totalQuantity = productMovements.Sum(m => m.Quantity);
                    var lastMovement = productMovements.OrderByDescending(m => m.MovementDate).First();

                    // Only create inventory record if there's stock
                    if (totalQuantity > 0)
                    {
                        inventoryLevels.Add(new WarehouseInventory
                        {
                            Id = Guid.NewGuid(),
                            TenantId = tenantId,
                            ProductId = product.Id,
                            WarehouseId = warehouse.Id,
                            Quantity = totalQuantity,
                            ReservedQuantity = 0, // No reservations yet
                            LastMovementDate = lastMovement.MovementDate,
                            CreatedAt = now,
                            UpdatedAt = now,
                            IsDeleted = false
                        });
                    }
                }
            }
        }

        return inventoryLevels;
    }

    private List<TaxRate> CreateTaxRatesForTenant(Guid tenantId, DateTime now, Country ecuadorCountry)
    {
        return new List<TaxRate>
        {
            new TaxRate
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Code = "IVA_0",
                Name = "IVA 0%",
                Rate = 0.00m,
                IsDefault = false,
                IsActive = true,
                CountryId = ecuadorCountry.Id,
                CreatedAt = now,
                UpdatedAt = now,
                IsDeleted = false
            },
            new TaxRate
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Code = "IVA_12",
                Name = "IVA 12%",
                Rate = 0.12m,
                IsDefault = false,
                IsActive = true,
                CountryId = ecuadorCountry.Id,
                CreatedAt = now,
                UpdatedAt = now,
                IsDeleted = false
            },
            new TaxRate
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Code = "IVA_15",
                Name = "IVA 15%",
                Rate = 0.15m,
                IsDefault = true, // Ecuador's standard IVA rate
                IsActive = true,
                CountryId = ecuadorCountry.Id,
                CreatedAt = now,
                UpdatedAt = now,
                IsDeleted = false
            }
        };
    }

    /* TODO: Re-enable once InvoiceConfiguration entity is created
    private InvoiceConfiguration CreateInvoiceConfigurationForTenant(
        Guid tenantId,
        List<TaxRate> taxRates,
        DateTime now)
    {
        // Find default tax rate (IVA 15%)
        var defaultTaxRate = taxRates.FirstOrDefault(t => t.IsDefault);

        return new InvoiceConfiguration
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            EstablishmentCode = "001", // Ecuador standard establishment code
            EmissionPointCode = "001",  // Ecuador standard emission point code
            NextSequentialNumber = 1,
            DefaultTaxRateId = defaultTaxRate?.Id,
            DefaultWarehouseId = null, // Will be set by user later
            DueDays = 30, // Default payment terms: Net 30
            RequireCustomerTaxId = true, // Ecuador requires RUC/Cédula on invoices
            CreatedAt = now,
            UpdatedAt = now,
            IsDeleted = false
        };
    }
    */

    private List<EmissionPoint> CreateEmissionPointsForTenant(
        Guid tenantId,
        List<Establishment> establishments,
        DateTime now)
    {
        var emissionPoints = new List<EmissionPoint>();

        foreach (var establishment in establishments)
        {
            // Create 2 emission points per establishment
            emissionPoints.Add(new EmissionPoint
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                EstablishmentId = establishment.Id,
                EmissionPointCode = "001",
                Name = "Main Cashier",
                IsActive = true,
                InvoiceSequence = 1,
                CreditNoteSequence = 1,
                DebitNoteSequence = 1,
                RetentionSequence = 1,
                CreatedAt = now,
                UpdatedAt = now,
                IsDeleted = false
            });

            emissionPoints.Add(new EmissionPoint
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                EstablishmentId = establishment.Id,
                EmissionPointCode = "002",
                Name = "Secondary Cashier",
                IsActive = true,
                InvoiceSequence = 1,
                CreditNoteSequence = 1,
                DebitNoteSequence = 1,
                RetentionSequence = 1,
                CreatedAt = now,
                UpdatedAt = now,
                IsDeleted = false
            });
        }

        return emissionPoints;
    }

    private SriConfiguration CreateSriConfigurationForTenant(Guid tenantId, string slug, DateTime now)
    {
        var (ruc, legalName, tradeName, address) = slug switch
        {
            "demo-company" => ("1790001234001", "Demo Company S.A.", "Demo Company", "Av. Amazonas N123-456 y Naciones Unidas, Quito"),
            "tech-startup" => ("1791234567001", "Tech Startup Inc. S.A.S.", "Tech Startup", "Av. República del Salvador N345-678, Quito"),
            "manufacturing-corp" => ("1792345678001", "Manufacturing Corporation del Ecuador C.A.", "Manufacturing Corp", "Av. 6 de Diciembre N567-890, Quito"),
            _ => ("1790000000001", "Default Company", "Default", "Quito, Ecuador")
        };

        return new SriConfiguration
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            CompanyRuc = ruc,
            LegalName = legalName,
            TradeName = tradeName,
            MainAddress = address,
            AccountingRequired = true,
            Environment = SriEnvironment.Test, // Test environment for demo
            CreatedAt = now,
            UpdatedAt = now,
            IsDeleted = false
        };
    }

    private List<Establishment> CreateEstablishmentsForTenant(Guid tenantId, string slug, DateTime now)
    {
        var establishments = new List<Establishment>();

        var locations = slug switch
        {
            "demo-company" => new[]
            {
                ("001", "Matriz Quito", "Av. Amazonas N123-456, Quito", "+593-2-234-5678"),
                ("002", "Sucursal Guayaquil", "Av. 9 de Octubre 234-345, Guayaquil", "+593-4-345-6789")
            },
            "tech-startup" => new[]
            {
                ("001", "Oficina Principal", "Av. República 345-678, Quito", "+593-2-345-6789"),
                ("002", "Sucursal Cuenca", "Calle Larga 456-567, Cuenca", "+593-7-456-7890")
            },
            "manufacturing-corp" => new[]
            {
                ("001", "Planta Industrial", "Av. 6 de Diciembre 567-890, Quito", "+593-2-456-7890"),
                ("002", "Bodega Central", "Av. Simón Bolívar 678-901, Guayaquil", "+593-4-567-8901")
            },
            _ => new[]
            {
                ("001", "Matriz", "Quito, Ecuador", "+593-2-000-0000")
            }
        };

        foreach (var (code, name, address, phone) in locations)
        {
            establishments.Add(new Establishment
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                EstablishmentCode = code,
                Name = name,
                Address = address,
                Phone = phone,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now,
                IsDeleted = false
            });
        }

        return establishments;
    }

    private List<EmissionPoint> CreateEmissionPointsForEstablishments(
        List<Establishment> establishments,
        Guid tenantId,
        DateTime now)
    {
        var emissionPoints = new List<EmissionPoint>();

        foreach (var establishment in establishments)
        {
            // Add 2 emission points per establishment
            emissionPoints.Add(new EmissionPoint
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                EstablishmentId = establishment.Id,
                EmissionPointCode = "001",
                Name = "Caja Principal",
                IsActive = true,
                InvoiceSequence = 1,
                CreditNoteSequence = 1,
                DebitNoteSequence = 1,
                RetentionSequence = 1,
                CreatedAt = now,
                UpdatedAt = now,
                IsDeleted = false
            });

            emissionPoints.Add(new EmissionPoint
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                EstablishmentId = establishment.Id,
                EmissionPointCode = "002",
                Name = "Caja Secundaria",
                IsActive = true,
                InvoiceSequence = 1,
                CreditNoteSequence = 1,
                DebitNoteSequence = 1,
                RetentionSequence = 1,
                CreatedAt = now,
                UpdatedAt = now,
                IsDeleted = false
            });
        }

        return emissionPoints;
    }

    private List<Invoice> CreateInvoicesForTenant(
        Guid tenantId,
        List<Customer> customers,
        List<Warehouse> warehouses,
        List<EmissionPoint> emissionPoints,
        List<TaxRate> taxRates,
        DateTime now)
    {
        var invoices = new List<Invoice>();
        var random = new Random(tenantId.GetHashCode());

        // Create 15 invoices with various SRI workflow statuses
        for (int i = 0; i < 15; i++)
        {
            var customer = customers[random.Next(customers.Count)];
            var warehouse = warehouses[random.Next(warehouses.Count)];
            var emissionPoint = emissionPoints[random.Next(emissionPoints.Count)];

            var issueDate = now.AddDays(-random.Next(1, 60));
            var dueDate = issueDate.AddDays(30);

            // Determine status to cover all SRI workflow states
            InvoiceStatus status;
            if (i < 2)
                status = InvoiceStatus.Draft; // 2 invoices
            else if (i < 4)
                status = InvoiceStatus.PendingSignature; // 2 invoices (XML generated, awaiting signature)
            else if (i < 6)
                status = InvoiceStatus.PendingAuthorization; // 2 invoices (signed, awaiting SRI)
            else if (i < 9)
                status = InvoiceStatus.Authorized; // 3 invoices (approved by SRI)
            else if (i < 10)
                status = InvoiceStatus.Rejected; // 1 invoice (rejected by SRI)
            else if (i < 12)
                status = InvoiceStatus.Sent; // 2 invoices (sent to customer)
            else if (i < 14)
                status = InvoiceStatus.Paid; // 2 invoices
            else
                status = InvoiceStatus.Overdue; // 1 invoice

            var invoice = new Invoice
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                InvoiceNumber = $"{emissionPoint.Establishment.EstablishmentCode}-{emissionPoint.EmissionPointCode}-{(i + 1):D9}",
                CustomerId = customer.Id,
                IssueDate = issueDate,
                DueDate = dueDate,
                Status = status,
                WarehouseId = warehouse.Id,
                EmissionPointId = emissionPoint.Id,
                DocumentType = DocumentType.Invoice,
                PaymentMethod = (SriPaymentMethod)random.Next(1, 5), // Random payment method
                Environment = SriEnvironment.Test,
                SubtotalAmount = 0, // Will be calculated from items
                TaxAmount = 0,
                TotalAmount = 0,
                Notes = $"Factura de prueba #{i + 1} - {status}",
                CreatedAt = issueDate,
                UpdatedAt = issueDate,
                IsDeleted = false
            };

            // Set SRI workflow fields based on status
            if (status >= InvoiceStatus.PendingSignature)
            {
                // XML has been generated
                invoice.XmlFilePath = $"invoices/{invoice.Id}/factura_{invoice.InvoiceNumber}.xml";
            }

            if (status >= InvoiceStatus.PendingAuthorization)
            {
                // Document has been signed
                invoice.SignedXmlFilePath = $"invoices/{invoice.Id}/factura_{invoice.InvoiceNumber}_signed.xml";
                invoice.AccessKey = $"1120{issueDate:ddMMyyyy}01{emissionPoint.Establishment.EstablishmentCode}{emissionPoint.EmissionPointCode}{(i + 1):D9}12345678{random.Next(10000000, 99999999)}";
            }

            if (status == InvoiceStatus.Authorized || status == InvoiceStatus.Sent || status == InvoiceStatus.Paid || status == InvoiceStatus.Overdue)
            {
                // Invoice has been authorized by SRI
                invoice.SriAuthorization = invoice.AccessKey;
                invoice.AuthorizationDate = issueDate.AddHours(2);
                invoice.RideFilePath = $"invoices/{invoice.Id}/ride_{invoice.InvoiceNumber}.pdf";
            }

            if (status == InvoiceStatus.Rejected)
            {
                // Invoice was rejected by SRI (has AccessKey but no authorization)
                invoice.AuthorizationDate = null;
                invoice.RideFilePath = null;
            }

            // Set paid date for paid invoices
            if (status == InvoiceStatus.Paid)
            {
                invoice.PaidDate = issueDate.AddDays(random.Next(5, 25));
            }

            invoices.Add(invoice);
        }

        return invoices;
    }

    private List<InvoiceItem> CreateInvoiceItemsForInvoices(
        List<Invoice> invoices,
        List<Product> products,
        List<TaxRate> taxRates,
        DateTime now)
    {
        var items = new List<InvoiceItem>();
        var random = new Random();

        foreach (var invoice in invoices)
        {
            // Each invoice has 2-5 items
            var itemCount = random.Next(2, 6);
            decimal invoiceSubtotal = 0;
            decimal invoiceTax = 0;

            for (int i = 0; i < itemCount; i++)
            {
                var product = products[random.Next(products.Count)];
                var quantity = random.Next(1, 10);
                var unitPrice = product.UnitPrice;
                var taxRate = taxRates.FirstOrDefault(t => t.IsDefault) ?? taxRates.First();

                var subtotal = unitPrice * quantity;
                var tax = subtotal * taxRate.Rate;
                var total = subtotal + tax;

                invoiceSubtotal += subtotal;
                invoiceTax += tax;

                items.Add(new InvoiceItem
                {
                    Id = Guid.NewGuid(),
                    InvoiceId = invoice.Id,
                    ProductId = product.Id,
                    ProductCode = product.Code,
                    ProductName = product.Name,
                    Description = product.Description ?? product.Name,
                    Quantity = quantity,
                    UnitPrice = unitPrice,
                    TaxRateId = taxRate.Id,
                    TaxRate = taxRate.Rate,
                    SubtotalAmount = subtotal,
                    TaxAmount = tax,
                    TotalAmount = total
                });
            }

            // Update invoice totals
            invoice.SubtotalAmount = invoiceSubtotal;
            invoice.TaxAmount = invoiceTax;
            invoice.TotalAmount = invoiceSubtotal + invoiceTax;
        }

        return items;
    }

    private List<SriErrorLog> CreateSriErrorLogsForInvoices(
        List<Invoice> invoices,
        DateTime now)
    {
        var errorLogs = new List<SriErrorLog>();
        var random = new Random();

        // Sample error scenarios for rejected invoices and failures
        var sampleErrors = new[]
        {
            new { Operation = "GenerateXml", Code = "XML_001", Message = "Error al generar XML: Campo obligatorio 'RUC del comprador' no puede estar vacío" },
            new { Operation = "SignDocument", Code = "SIGN_001", Message = "Error al firmar documento: El certificado digital ha expirado" },
            new { Operation = "SignDocument", Code = "SIGN_002", Message = "Error al firmar documento: La contraseña del certificado es incorrecta" },
            new { Operation = "SubmitToSRI", Code = "SRI_001", Message = "Recepción SRI: CLAVE_ACCESO_INVALIDA - La clave de acceso no tiene el formato correcto" },
            new { Operation = "SubmitToSRI", Code = "SRI_002", Message = "Recepción SRI: SECUENCIAL_REGISTRADO - El secuencial de la factura ya está registrado" },
            new { Operation = "CheckAuthorization", Code = "SRI_003", Message = "Autorización SRI: DOCUMENTO_NO_AUTORIZADO - El documento no cumple con las validaciones del SRI" },
            new { Operation = "CheckAuthorization", Code = "SRI_004", Message = "Autorización SRI: ESTABLECIMIENTO_INACTIVO - El establecimiento emisor no está activo en el RUC" },
            new { Operation = "GenerateRIDE", Code = "RIDE_001", Message = "Error al generar RIDE: No se encontró la información de autorización del SRI" }
        };

        // Add error logs for rejected invoices
        var rejectedInvoices = invoices.Where(i => i.Status == InvoiceStatus.Rejected).ToList();
        foreach (var invoice in rejectedInvoices)
        {
            // Rejected invoices typically have errors in submission or authorization
            var errorScenario = sampleErrors[random.Next(3, 7)]; // Use SRI-related errors

            errorLogs.Add(new SriErrorLog
            {
                Id = Guid.NewGuid(),
                InvoiceId = invoice.Id,
                TenantId = invoice.TenantId,
                Operation = errorScenario.Operation,
                ErrorCode = errorScenario.Code,
                ErrorMessage = errorScenario.Message,
                StackTrace = GenerateSampleStackTrace(errorScenario.Operation),
                OccurredAt = invoice.IssueDate.AddHours(1),
                WasRetried = random.Next(0, 2) == 1,
                RetrySucceeded = false,
                CreatedAt = invoice.IssueDate.AddHours(1),
                UpdatedAt = invoice.IssueDate.AddHours(1),
                IsDeleted = false
            });
        }

        // Add some error logs for successful invoices (transient errors that were later resolved)
        var successfulInvoices = invoices
            .Where(i => i.Status == InvoiceStatus.Authorized || i.Status == InvoiceStatus.Sent)
            .Take(2)
            .ToList();

        foreach (var invoice in successfulInvoices)
        {
            // These had temporary failures but succeeded on retry
            var errorScenario = sampleErrors[random.Next(0, 3)]; // Use generation/signing errors

            errorLogs.Add(new SriErrorLog
            {
                Id = Guid.NewGuid(),
                InvoiceId = invoice.Id,
                TenantId = invoice.TenantId,
                Operation = errorScenario.Operation,
                ErrorCode = errorScenario.Code,
                ErrorMessage = errorScenario.Message,
                StackTrace = GenerateSampleStackTrace(errorScenario.Operation),
                OccurredAt = invoice.IssueDate.AddMinutes(30),
                WasRetried = true,
                RetrySucceeded = true,
                CreatedAt = invoice.IssueDate.AddMinutes(30),
                UpdatedAt = invoice.IssueDate.AddHours(1),
                IsDeleted = false
            });
        }

        return errorLogs;
    }

    private string GenerateSampleStackTrace(string operation)
    {
        return operation switch
        {
            "GenerateXml" => @"   at SaaS.Application.Features.Invoices.Commands.GenerateInvoiceXml.GenerateInvoiceXmlCommandHandler.Handle(GenerateInvoiceXmlCommand request, CancellationToken cancellationToken) in C:\Projects\SaaS\src\Application\Features\Invoices\Commands\GenerateInvoiceXml\GenerateInvoiceXmlCommandHandler.cs:line 87
   at MediatR.Pipeline.RequestExceptionProcessorBehavior`2.Handle(TRequest request, RequestHandlerDelegate`1 next, CancellationToken cancellationToken)",
            "SignDocument" => @"   at SaaS.Infrastructure.ExternalServices.SriSoapClient.SignDocumentAsync(String xmlContent, String certificatePath, String password) in C:\Projects\SaaS\src\Infrastructure\ExternalServices\SriSoapClient.cs:line 145
   at SaaS.Application.Features.Invoices.Commands.SignInvoice.SignInvoiceCommandHandler.Handle(SignInvoiceCommand request, CancellationToken cancellationToken) in C:\Projects\SaaS\src\Application\Features\Invoices\Commands\SignInvoice\SignInvoiceCommandHandler.cs:line 95",
            "SubmitToSRI" => @"   at SaaS.Infrastructure.ExternalServices.SriSoapClient.SubmitDocumentAsync(String signedXml) in C:\Projects\SaaS\src\Infrastructure\ExternalServices\SriSoapClient.cs:line 178
   at SaaS.Application.Features.Invoices.Commands.SubmitToSri.SubmitToSriCommandHandler.Handle(SubmitToSriCommand request, CancellationToken cancellationToken) in C:\Projects\SaaS\src\Application\Features\Invoices\Commands\SubmitToSri\SubmitToSriCommandHandler.cs:line 112",
            "CheckAuthorization" => @"   at SaaS.Infrastructure.ExternalServices.SriSoapClient.CheckAuthorizationAsync(String accessKey) in C:\Projects\SaaS\src\Infrastructure\ExternalServices\SriSoapClient.cs:line 203
   at SaaS.Application.Features.Invoices.Commands.CheckAuthorizationStatus.CheckAuthorizationStatusCommandHandler.Handle(CheckAuthorizationStatusCommand request, CancellationToken cancellationToken) in C:\Projects\SaaS\src\Application\Features\Invoices\Commands\CheckAuthorizationStatus\CheckAuthorizationStatusCommandHandler.cs:line 98",
            "GenerateRIDE" => @"   at QuestPDF.Infrastructure.Document.GeneratePdf() in C:\Projects\QuestPDF\Infrastructure\Document.cs:line 234
   at SaaS.Infrastructure.Services.RideGenerationService.GenerateRidePdfAsync(Invoice invoice) in C:\Projects\SaaS\src\Infrastructure\Services\RideGenerationService.cs:line 287
   at SaaS.Application.Features.Invoices.Commands.GenerateRide.GenerateRideCommandHandler.Handle(GenerateRideCommand request, CancellationToken cancellationToken) in C:\Projects\SaaS\src\Application\Features\Invoices\Commands\GenerateRide\GenerateRideCommandHandler.cs:line 125",
            _ => "   at System.Runtime.CompilerServices.TaskAwaiter.ThrowForNonSuccess(Task task)\n   at System.Runtime.CompilerServices.TaskAwaiter.HandleNonSuccessAndDebuggerNotification(Task task)"
        };
    }

    #endregion
}
