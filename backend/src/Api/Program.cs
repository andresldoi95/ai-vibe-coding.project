using System.Text;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SaaS.Api.Authorization;
using SaaS.Api.Middleware;
using SaaS.Application.Common.Behaviors;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.Interfaces;
using SaaS.Infrastructure.Persistence;
using SaaS.Infrastructure.Persistence.Repositories;
using SaaS.Infrastructure.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger
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

// Configure Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JWT");
var secretKey = jwtSettings["Secret"] ?? throw new InvalidOperationException("JWT Secret not configured");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization(options =>
{
    // Define permission-based policies
    var permissions = new[]
    {
        // Warehouses
        "warehouses.read", "warehouses.create", "warehouses.update", "warehouses.delete",
        // Products
        "products.read", "products.create", "products.update", "products.delete",
        // Customers
        "customers.read", "customers.create", "customers.update", "customers.delete",
        // Stock
        "stock.read", "stock.create", "stock.update", "stock.delete",
        // Tenants
        "tenants.read", "tenants.create", "tenants.update", "tenants.delete",
        // Users
        "users.read", "users.create", "users.update", "users.delete", "users.invite", "users.remove",
        // Roles
        "roles.read", "roles.manage",
        // Tax Rates
        "tax-rates.read", "tax-rates.create", "tax-rates.update", "tax-rates.delete",
        // Invoice Configurations
        "invoice-config.read", "invoice-config.update",
        // Invoices
        "invoices.read", "invoices.create", "invoices.update", "invoices.delete",
        "invoices.send", "invoices.void", "invoices.export",
        // SRI - Establishments
        "establishments.read", "establishments.create", "establishments.update", "establishments.delete",
        // SRI - Emission Points
        "emission_points.read", "emission_points.create", "emission_points.update", "emission_points.delete",
        // SRI - Configuration
        "sri_configuration.read", "sri_configuration.update"
    };

    foreach (var permission in permissions)
    {
        options.AddPolicy(permission, policy =>
            policy.Requirements.Add(new PermissionRequirement(permission)));
    }
});

// Register authorization handler
builder.Services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .WithExposedHeaders("X-Tenant-Id");
    });
});

// Register Application Services
builder.Services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssembly(typeof(SaaS.Application.DTOs.UserDto).Assembly);

    // Register pipeline behaviors
    cfg.AddOpenBehavior(typeof(UnhandledExceptionBehavior<,>));
    cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
});

// Register FluentValidation
builder.Services.AddValidatorsFromAssembly(typeof(SaaS.Application.DTOs.UserDto).Assembly);

// Register Infrastructure Services
builder.Services.AddScoped<ITenantContext, TenantContext>();
builder.Services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserPermissionService, UserPermissionService>();
builder.Services.AddScoped<SaaS.Application.Services.IStockLevelService, SaaS.Application.Services.StockLevelService>();
builder.Services.AddScoped<IExportService, SaaS.Application.Services.ExportService>();
builder.Services.AddScoped<ITaxCalculationService, TaxCalculationService>();
builder.Services.AddScoped<IInvoiceNumberService, InvoiceNumberService>();

// Register SRI Services for electronic invoicing
builder.Services.AddScoped<ISriAccessKeyService, SriAccessKeyService>();
builder.Services.AddScoped<IInvoiceXmlService, InvoiceXmlService>();
builder.Services.AddScoped<IXmlSignatureService, XmlSignatureService>();

// Configure Email Settings
builder.Services.Configure<SaaS.Application.Common.Models.EmailSettings>(
    builder.Configuration.GetSection("Email"));

// Register Email Service
builder.Services.AddScoped<IEmailService, SaaS.Infrastructure.Services.EmailService>();

// Register Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ITenantRepository, TenantRepository>();
builder.Services.AddScoped<IUserTenantRepository, UserTenantRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IWarehouseRepository, WarehouseRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IStockMovementRepository, StockMovementRepository>();
builder.Services.AddScoped<IWarehouseInventoryRepository, WarehouseInventoryRepository>();
builder.Services.AddScoped<IEmailLogRepository, SaaS.Infrastructure.Persistence.Repositories.EmailLogRepository>();
builder.Services.AddScoped<IEmailTemplateRepository, SaaS.Infrastructure.Persistence.Repositories.EmailTemplateRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IPermissionRepository, PermissionRepository>();
builder.Services.AddScoped<ITaxRateRepository, TaxRateRepository>();
builder.Services.AddScoped<IInvoiceConfigurationRepository, InvoiceConfigurationRepository>();
builder.Services.AddScoped<IInvoiceRepository, InvoiceRepository>();
builder.Services.AddScoped<IRepository<SaaS.Domain.Entities.InvoiceItem>, Repository<SaaS.Domain.Entities.InvoiceItem>>();
builder.Services.AddScoped<IEstablishmentRepository, EstablishmentRepository>();
builder.Services.AddScoped<IEmissionPointRepository, EmissionPointRepository>();
builder.Services.AddScoped<ISriConfigurationRepository, SriConfigurationRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

// Tenant resolution must come after authentication
app.UseMiddleware<TenantResolutionMiddleware>();

app.MapControllers();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
        dbContext.Database.Migrate();
        Log.Information("Database migration completed successfully");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "An error occurred while migrating the database");
    }
}

Log.Information("Starting SaaS API");

app.Run();
