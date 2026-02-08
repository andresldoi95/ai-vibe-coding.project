# Email Agent

## Specialization
Expert in email service implementation, SMTP configuration, email templating with MJML framework, and testing with Mailpit for the SaaS Billing + Inventory Management System. Handles transactional emails, notifications, and email testing workflows.

## Tech Stack
- **SMTP Client**: MailKit 4.14.1 (production-ready .NET library)
- **Testing**: Mailpit (modern SMTP testing server with web UI)
- **Templating**: MJML 4.15+ (responsive email framework)
- **Template Engine**: MJML CLI for compilation (MJML → HTML)
- **Queue**: Background jobs for async sending (future: Hangfire)
- **Storage**: Email audit log in PostgreSQL
- **Dark Mode**: CSS media queries for `prefers-color-scheme`

## Core Responsibilities

### 1. Email Service Architecture

#### Email Types
- **Authentication**: Registration confirmation, password reset, email verification
- **Invitations**: User invitation to company, role assignment notifications
- **Notifications**: Invoice created, payment received, stock alerts
- **Alerts**: Low stock warnings, subscription expiration
- **Reports**: Monthly summaries, billing statements

#### Design Patterns
- **Repository Pattern**: Email template management
- **Factory Pattern**: Email builder based on type
- **Strategy Pattern**: Different sending strategies (immediate, queued, scheduled)
- **Observer Pattern**: Event-driven email triggers

## Implementation

### 1. Mailpit Setup (Docker)

#### Add to `docker-compose.yml`
```yaml
services:
  # ... existing services (postgres, backend, frontend)

  mailpit:
    image: axllent/mailpit:latest
    container_name: saas_mailpit
    ports:
      - "1025:1025"  # SMTP server
      - "8025:8025"  # Web UI
    environment:
      MP_SMTP_AUTH_ACCEPT_ANY: 1
      MP_SMTP_AUTH_ALLOW_INSECURE: 1
      MP_MAX_MESSAGES: 500
    networks:
      - saas-network
    restart: unless-stopped

networks:
  saas-network:
    driver: bridge
```

#### Add to `docker-compose.prod.yml` (Override for Production)
```yaml
services:
  # mailpit is disabled in production
  # Use real SMTP service (SendGrid, AWS SES, etc.)
```

#### Access Mailpit Web UI
- URL: `http://localhost:8025`
- Features: View emails, search, API access, webhook testing

### 2. Backend Configuration

#### Install NuGet Packages
```bash
cd backend/src/Infrastructure
dotnet add package MailKit
dotnet add package MimeKit
```

#### AppSettings Configuration

**`appsettings.json`** (Default/Production)
```json
{
  "Email": {
    "SmtpHost": "smtp.sendgrid.net",
    "SmtpPort": 587,
    "SmtpUsername": "",
    "SmtpPassword": "",
    "FromEmail": "noreply@yoursaas.com",
    "FromName": "Your SaaS Platform",
    "EnableSsl": true,
    "Timeout": 30000
  }
}
```

**`appsettings.Development.json`** (Mailpit)
```json
{
  "Email": {
    "SmtpHost": "localhost",
    "SmtpPort": 1025,
    "SmtpUsername": "",
    "SmtpPassword": "",
    "FromEmail": "dev@yoursaas.local",
    "FromName": "Your SaaS (Dev)",
    "EnableSsl": false,
    "Timeout": 10000
  }
}
```

### 3. Domain Entities

#### EmailLog (Audit Trail)
Located in: `Domain/Entities/EmailLog.cs`

```csharp
namespace Domain.Entities;

public class EmailLog : TenantEntity
{
    public Guid? UserId { get; set; }
    public string To { get; set; } = string.Empty;
    public string? Cc { get; set; }
    public string? Bcc { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public EmailType Type { get; set; }
    public EmailStatus Status { get; set; } = EmailStatus.Pending;
    public string? ErrorMessage { get; set; }
    public DateTime? SentAt { get; set; }
    public int RetryCount { get; set; } = 0;

    // Navigation
    public User? User { get; set; }
}
```

#### EmailTemplate (Reusable Templates)
Located in: `Domain/Entities/EmailTemplate.cs`

```csharp
namespace Domain.Entities;

public class EmailTemplate : TenantEntity
{
    public string Name { get; set; } = string.Empty; // "UserInvitation", "PasswordReset"
    public string Subject { get; set; } = string.Empty;
    public string BodyHtml { get; set; } = string.Empty;
    public string? BodyText { get; set; }
    public EmailType Type { get; set; }
    public bool IsSystemTemplate { get; set; } = false;
    public Dictionary<string, string>? Variables { get; set; } // JSON: {"{UserName}": "description"}
}
```

#### Enums
Located in: `Domain/Enums/EmailType.cs`

```csharp
namespace Domain.Enums;

public enum EmailType
{
    UserInvitation = 1,
    PasswordReset = 2,
    EmailVerification = 3,
    WelcomeEmail = 4,
    InvoiceCreated = 5,
    PaymentReceived = 6,
    LowStockAlert = 7,
    SubscriptionExpiring = 8,
    MonthlyReport = 9,
    Custom = 99
}

public enum EmailStatus
{
    Pending = 1,
    Sent = 2,
    Failed = 3,
    Queued = 4,
    Cancelled = 5
}
```

### 4. Application Layer

#### Email Configuration Model
Located in: `Application/Common/Models/EmailSettings.cs`

```csharp
namespace Application.Common.Models;

public class EmailSettings
{
    public string SmtpHost { get; set; } = string.Empty;
    public int SmtpPort { get; set; }
    public string SmtpUsername { get; set; } = string.Empty;
    public string SmtpPassword { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
    public bool EnableSsl { get; set; } = true;
    public int Timeout { get; set; } = 30000;
}
```

#### Email Service Interface
Located in: `Application/Common/Interfaces/IEmailService.cs`

```csharp
namespace Application.Common.Interfaces;

public interface IEmailService
{
    // Send immediate email
    Task<Result> SendEmailAsync(EmailMessage message, CancellationToken cancellationToken = default);

    // Send from template
    Task<Result> SendTemplateEmailAsync(
        string templateName,
        string to,
        Dictionary<string, string> variables,
        CancellationToken cancellationToken = default);

    // Common transactional emails
    Task<Result> SendUserInvitationAsync(string email, string companyName, string inviteToken, Guid tenantId);
    Task<Result> SendPasswordResetAsync(string email, string resetToken);
    Task<Result> SendEmailVerificationAsync(string email, string verificationToken);
    Task<Result> SendWelcomeEmailAsync(string email, string userName, Guid tenantId);

    // Billing emails
    Task<Result> SendInvoiceCreatedAsync(string email, Guid invoiceId, Guid tenantId);
    Task<Result> SendPaymentReceivedAsync(string email, decimal amount, Guid tenantId);

    // Alerts
    Task<Result> SendLowStockAlertAsync(string email, string productName, int currentStock, Guid tenantId);
}

public class EmailMessage
{
    public string To { get; set; } = string.Empty;
    public string? Cc { get; set; }
    public string? Bcc { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string BodyHtml { get; set; } = string.Empty;
    public string? BodyText { get; set; }
    public EmailType Type { get; set; } = EmailType.Custom;
    public Guid? TenantId { get; set; }
    public Guid? UserId { get; set; }
    public List<EmailAttachment>? Attachments { get; set; }
}

public class EmailAttachment
{
    public string FileName { get; set; } = string.Empty;
    public byte[] Content { get; set; } = Array.Empty<byte>();
    public string ContentType { get; set; } = "application/octet-stream";
}
```

### 5. Infrastructure Implementation

#### Email Service
Located in: `Infrastructure/Services/EmailService.cs`

```csharp
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly IEmailLogRepository _emailLogRepository;
    private readonly IEmailTemplateRepository _templateRepository;
    private readonly ITenantContext _tenantContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<EmailService> _logger;

    public EmailService(
        IOptions<EmailSettings> settings,
        IEmailLogRepository emailLogRepository,
        IEmailTemplateRepository templateRepository,
        ITenantContext tenantContext,
        IUnitOfWork unitOfWork,
        ILogger<EmailService> logger)
    {
        _settings = settings.Value;
        _emailLogRepository = emailLogRepository;
        _templateRepository = templateRepository;
        _tenantContext = tenantContext;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> SendEmailAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        var emailLog = new EmailLog
        {
            TenantId = message.TenantId ?? _tenantContext.TenantId,
            UserId = message.UserId,
            To = message.To,
            Cc = message.Cc,
            Bcc = message.Bcc,
            Subject = message.Subject,
            Body = message.BodyHtml,
            Type = message.Type,
            Status = EmailStatus.Pending
        };

        try
        {
            var mimeMessage = new MimeMessage();
            mimeMessage.From.Add(new MailboxAddress(_settings.FromName, _settings.FromEmail));
            mimeMessage.To.Add(MailboxAddress.Parse(message.To));

            if (!string.IsNullOrEmpty(message.Cc))
                mimeMessage.Cc.Add(MailboxAddress.Parse(message.Cc));

            if (!string.IsNullOrEmpty(message.Bcc))
                mimeMessage.Bcc.Add(MailboxAddress.Parse(message.Bcc));

            mimeMessage.Subject = message.Subject;

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = message.BodyHtml,
                TextBody = message.BodyText ?? StripHtml(message.BodyHtml)
            };

            // Add attachments
            if (message.Attachments != null)
            {
                foreach (var attachment in message.Attachments)
                {
                    bodyBuilder.Attachments.Add(
                        attachment.FileName,
                        attachment.Content,
                        ContentType.Parse(attachment.ContentType));
                }
            }

            mimeMessage.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            client.Timeout = _settings.Timeout;

            await client.ConnectAsync(
                _settings.SmtpHost,
                _settings.SmtpPort,
                _settings.EnableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None,
                cancellationToken);

            if (!string.IsNullOrEmpty(_settings.SmtpUsername))
            {
                await client.AuthenticateAsync(_settings.SmtpUsername, _settings.SmtpPassword, cancellationToken);
            }

            await client.SendAsync(mimeMessage, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);

            emailLog.Status = EmailStatus.Sent;
            emailLog.SentAt = DateTime.UtcNow;

            _logger.LogInformation("Email sent successfully to {To}", message.To);

            return Result.Success();
        }
        catch (Exception ex)
        {
            emailLog.Status = EmailStatus.Failed;
            emailLog.ErrorMessage = ex.Message;

            _logger.LogError(ex, "Failed to send email to {To}", message.To);

            return Result.Failure($"Failed to send email: {ex.Message}");
        }
        finally
        {
            await _emailLogRepository.AddAsync(emailLog);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<Result> SendTemplateEmailAsync(
        string templateName,
        string to,
        Dictionary<string, string> variables,
        CancellationToken cancellationToken = default)
    {
        var template = await _templateRepository.GetByNameAsync(templateName, _tenantContext.TenantId);

        if (template == null)
            return Result.Failure($"Email template '{templateName}' not found");

        var subject = ReplaceVariables(template.Subject, variables);
        var bodyHtml = ReplaceVariables(template.BodyHtml, variables);
        var bodyText = template.BodyText != null ? ReplaceVariables(template.BodyText, variables) : null;

        var message = new EmailMessage
        {
            To = to,
            Subject = subject,
            BodyHtml = bodyHtml,
            BodyText = bodyText,
            Type = template.Type,
            TenantId = _tenantContext.TenantId
        };

        return await SendEmailAsync(message, cancellationToken);
    }

    public async Task<Result> SendUserInvitationAsync(string email, string companyName, string inviteToken, Guid tenantId)
    {
        var inviteUrl = $"https://yoursaas.com/accept-invitation?token={inviteToken}";

        var variables = new Dictionary<string, string>
        {
            { "{CompanyName}", companyName },
            { "{InviteUrl}", inviteUrl },
            { "{Email}", email }
        };

        return await SendTemplateEmailAsync("UserInvitation", email, variables);
    }

    public async Task<Result> SendPasswordResetAsync(string email, string resetToken)
    {
        var resetUrl = $"https://yoursaas.com/reset-password?token={resetToken}";

        var variables = new Dictionary<string, string>
        {
            { "{ResetUrl}", resetUrl },
            { "{Email}", email }
        };

        return await SendTemplateEmailAsync("PasswordReset", email, variables);
    }

    public async Task<Result> SendEmailVerificationAsync(string email, string verificationToken)
    {
        var verifyUrl = $"https://yoursaas.com/verify-email?token={verificationToken}";

        var variables = new Dictionary<string, string>
        {
            { "{VerifyUrl}", verifyUrl },
            { "{Email}", email }
        };

        return await SendTemplateEmailAsync("EmailVerification", email, variables);
    }

    public async Task<Result> SendWelcomeEmailAsync(string email, string userName, Guid tenantId)
    {
        var variables = new Dictionary<string, string>
        {
            { "{UserName}", userName },
            { "{DashboardUrl}", "https://yoursaas.com/dashboard" }
        };

        return await SendTemplateEmailAsync("WelcomeEmail", email, variables);
    }

    public async Task<Result> SendInvoiceCreatedAsync(string email, Guid invoiceId, Guid tenantId)
    {
        var invoiceUrl = $"https://yoursaas.com/invoices/{invoiceId}";

        var variables = new Dictionary<string, string>
        {
            { "{InvoiceUrl}", invoiceUrl },
            { "{InvoiceId}", invoiceId.ToString() }
        };

        return await SendTemplateEmailAsync("InvoiceCreated", email, variables);
    }

    public async Task<Result> SendPaymentReceivedAsync(string email, decimal amount, Guid tenantId)
    {
        var variables = new Dictionary<string, string>
        {
            { "{Amount}", amount.ToString("C") },
            { "{Date}", DateTime.UtcNow.ToString("yyyy-MM-dd") }
        };

        return await SendTemplateEmailAsync("PaymentReceived", email, variables);
    }

    public async Task<Result> SendLowStockAlertAsync(string email, string productName, int currentStock, Guid tenantId)
    {
        var variables = new Dictionary<string, string>
        {
            { "{ProductName}", productName },
            { "{CurrentStock}", currentStock.ToString() }
        };

        return await SendTemplateEmailAsync("LowStockAlert", email, variables);
    }

    private static string ReplaceVariables(string template, Dictionary<string, string> variables)
    {
        foreach (var variable in variables)
        {
            template = template.Replace(variable.Key, variable.Value);
        }
        return template;
    }

    private static string StripHtml(string html)
    {
        return System.Text.RegularExpressions.Regex.Replace(html, "<.*?>", string.Empty);
    }
}
```

### 6. Email Templates (MJML Framework)

#### Directory Structure
```
Infrastructure/Data/EmailTemplates/
├── src/                    # MJML source files
│   ├── PasswordReset.mjml
│   ├── WelcomeEmail.mjml
│   ├── UserInvitation.mjml
│   └── _base-layout.mjml  # Base template (reference only)
├── dist/                   # Compiled HTML (generated)
│   ├── PasswordReset.html
│   ├── WelcomeEmail.html
│   └── UserInvitation.html
├── assets/                 # Images, logos
│   └── logo.svg
├── old-templates/          # Legacy templates (backup)
├── package.json            # MJML dependencies
├── compile-templates.ps1   # Build script
├── test-templates.ps1      # Testing script
└── README.md               # Template documentation
```

#### Why MJML?
- **Responsive Design**: Automatically adapts to mobile/desktop
- **Email Client Compatibility**: Works across Outlook, Gmail, Apple Mail, etc.
- **Dark Mode Support**: Built-in CSS media queries
- **Maintainability**: Cleaner syntax than raw HTML
- **Industry Standard**: Used by major SaaS companies

#### MJML Template Structure

**PasswordReset.mjml** (Example with Dark Mode)
```mjml
<mjml>
  <mj-head>
    <mj-title>Reset Your Password</mj-title>
    <mj-preview>Reset your password for SaaS Platform</mj-preview>

    <!-- Dark Mode Styles -->
    <mj-style>
      @media (prefers-color-scheme: dark) {
        .dark-mode-bg { background-color: #111827 !important; }
        .dark-mode-card { background-color: #1f2937 !important; }
        .text-primary { color: #f3f4f6 !important; }
      }
    </mj-style>
  </mj-head>

  <mj-body background-color="#f9fafb" css-class="dark-mode-bg">

    <!-- Header with Logo -->
    <mj-section background-color="#ffffff" css-class="dark-mode-card">
      <mj-column>
        <mj-image
          src="https://via.placeholder.com/200x60/14b8a6/ffffff?text=SaaS+Platform"
          alt="Logo"
          width="200px"
        />
      </mj-column>
    </mj-section>

    <!-- Main Content -->
    <mj-section background-color="#ffffff" css-class="dark-mode-card">
      <mj-column>
        <mj-text font-size="28px" font-weight="700" color="#1f2937" css-class="text-primary">
          Reset Your Password
        </mj-text>

        <mj-text font-size="16px" color="#374151" line-height="1.7">
          We received a request to reset your password for {userName}.
        </mj-text>

        <mj-button
          href="{resetLink}"
          background-color="#14b8a6"
          color="#ffffff"
          font-size="16px"
          border-radius="6px"
        >
          Reset Password
        </mj-button>

        <mj-text font-size="14px" color="#6b7280">
          This link expires in <strong>{expiryTime}</strong>.
        </mj-text>
      </mj-column>
    </mj-section>

  </mj-body>
</mjml>
```

**Template Variables**

All templates use `{VariableName}` syntax for dynamic content:

- **PasswordReset**:
  - `{userName}` - User's display name
  - `{resetLink}` - Password reset URL
  - `{expiryTime}` - Link expiration time
  - `{frontendUrl}` - Base frontend URL

- **WelcomeEmail**:
  - `{userName}` - User's display name
  - `{companyName}` - Company/tenant name
  - `{loginLink}` - Login page URL
  - `{frontendUrl}` - Base frontend URL

- **UserInvitation**:
  - `{inviteeName}` - Person being invited
  - `{inviterName}` - Person sending invite
  - `{companyName}` - Company name
  - `{invitationLink}` - Acceptance URL
  - `{role}` - User role
  - `{frontendUrl}` - Base frontend URL

#### Compiling Templates

**Option 1: PowerShell Script (Recommended)**
```powershell
cd backend/src/Infrastructure/Data/EmailTemplates
.\compile-templates.ps1
```

**Option 2: NPM Commands**
```bash
cd backend/src/Infrastructure/Data/EmailTemplates
npm install    # First time only
npm run build  # Compile all templates
npm run watch  # Auto-recompile on changes
```

**Build Output:**
```
Compiling templates from src/ to dist/...
✓ PasswordReset.html
✓ WelcomeEmail.html
✓ UserInvitation.html
```

#### Creating New Templates

1. **Create MJML file** in `src/` directory:
```bash
touch src/NewTemplate.mjml
```

2. **Use MJML components** (see [MJML docs](https://documentation.mjml.io/)):
```mjml
<mjml>
  <mj-head>
    <mj-title>Your Template</mj-title>
    <!-- Include dark mode styles -->
  </mj-head>
  <mj-body>
    <!-- Your content using mj-section, mj-column, mj-text, mj-button, etc. -->
  </mj-body>
</mjml>
```

3. **Compile to HTML**:
```powershell
.\compile-templates.ps1
```

4. **Update EmailService** to use new template:
```csharp
public async Task<Result> SendNewTemplateAsync(string email, Dictionary<string, string> variables)
{
    var message = new EmailMessage
    {
        To = email,
        Subject = "Your Subject",
        BodyHtml = await GetTemplateContentAsync("NewTemplate", variables),
        Type = EmailType.Custom
    };

    return await SendEmailAsync(message);
}
```

#### Dark Mode Implementation

All templates include dark mode support via CSS media queries:

```css
@media (prefers-color-scheme: dark) {
  .dark-mode-bg { background-color: #111827 !important; }
  .dark-mode-card { background-color: #1f2937 !important; }
  .text-primary { color: #f3f4f6 !important; }
  .text-secondary { color: #9ca3af !important; }
  .brand-primary { color: #5eead4 !important; }
}
```

**Tested in:**
- ✅ Apple Mail (macOS, iOS)
- ✅ Gmail App (Android, iOS)
- ✅ Outlook Mobile
- ⚠️ Gmail Web (limited support)

#### Template Testing Workflow

1. **Start Mailpit**:
```powershell
docker-compose up mailpit
```

2. **Compile Templates**:
```powershell
cd backend/src/Infrastructure/Data/EmailTemplates
.\compile-templates.ps1
```

3. **Send Test Emails**:
```powershell
.\test-templates.ps1
# Or manually via API:
# POST http://localhost:5000/api/v1/testemail/send-password-reset
# Body: "test@example.com"
```

4. **View in Mailpit**: http://localhost:8025

5. **Verify**:
   - ✓ Template renders correctly
   - ✓ Variables replaced properly
   - ✓ Responsive on mobile
   - ✓ Dark mode triggers correctly
   - ✓ Buttons/links work
```

### 7. Template Seeding

Located in: `Infrastructure/Data/Seeders/EmailTemplateSeeder.cs`

```csharp
namespace Infrastructure.Data.Seeders;

public class EmailTemplateSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context, Guid tenantId)
    {
        if (await context.EmailTemplates.AnyAsync(t => t.TenantId == tenantId))
            return;

        var templates = new List<EmailTemplate>
        {
            new()
            {
                TenantId = tenantId,
                Name = "UserInvitation",
                Subject = "You're invited to join {CompanyName}",
                BodyHtml = await File.ReadAllTextAsync("Infrastructure/Data/EmailTemplates/UserInvitation.html"),
                Type = EmailType.UserInvitation,
                IsSystemTemplate = true,
                Variables = new Dictionary<string, string>
                {
                    { "{CompanyName}", "The company name" },
                    { "{InviteUrl}", "Invitation acceptance URL" },
                    { "{Email}", "Recipient email address" }
                }
            },
            new()
            {
                TenantId = tenantId,
                Name = "PasswordReset",
                Subject = "Reset your password",
                BodyHtml = await File.ReadAllTextAsync("Infrastructure/Data/EmailTemplates/PasswordReset.html"),
                Type = EmailType.PasswordReset,
                IsSystemTemplate = true,
                Variables = new Dictionary<string, string>
                {
                    { "{ResetUrl}", "Password reset URL" },
                    { "{Email}", "User email address" }
                }
            },
            new()
            {
                TenantId = tenantId,
                Name = "WelcomeEmail",
                Subject = "Welcome to {CompanyName}!",
                BodyHtml = "<h1>Welcome {UserName}!</h1><p>We're excited to have you on board.</p>",
                Type = EmailType.WelcomeEmail,
                IsSystemTemplate = true,
                Variables = new Dictionary<string, string>
                {
                    { "{UserName}", "User's full name" },
                    { "{DashboardUrl}", "Dashboard URL" }
                }
            }
        };

        await context.EmailTemplates.AddRangeAsync(templates);
        await context.SaveChangesAsync();
    }
}
```

### 8. Repositories

#### IEmailLogRepository
Located in: `Application/Common/Interfaces/IEmailLogRepository.cs`

```csharp
namespace Application.Common.Interfaces;

public interface IEmailLogRepository : IRepository<EmailLog>
{
    Task<List<EmailLog>> GetByUserIdAsync(Guid userId, Guid tenantId);
    Task<List<EmailLog>> GetFailedEmailsAsync(Guid tenantId);
    Task<List<EmailLog>> GetPendingEmailsAsync(Guid tenantId);
    Task<EmailLog?> GetByIdWithUserAsync(Guid id, Guid tenantId);
}
```

#### IEmailTemplateRepository
Located in: `Application/Common/Interfaces/IEmailTemplateRepository.cs`

```csharp
namespace Application.Common.Interfaces;

public interface IEmailTemplateRepository : IRepository<EmailTemplate>
{
    Task<EmailTemplate?> GetByNameAsync(string name, Guid tenantId);
    Task<List<EmailTemplate>> GetByTypeAsync(EmailType type, Guid tenantId);
    Task<bool> ExistsAsync(string name, Guid tenantId);
}
```

### 9. Registration in Program.cs

```csharp
// Configure Email Settings
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("Email"));

// Register Email Service
builder.Services.AddScoped<IEmailService, EmailService>();

// Register Repositories
builder.Services.AddScoped<IEmailLogRepository, EmailLogRepository>();
builder.Services.AddScoped<IEmailTemplateRepository, EmailTemplateRepository>();
```

### 10. Database Migration

```bash
cd backend
dotnet ef migrations add AddEmailSupport --project src/Infrastructure --startup-project src/Api
dotnet ef database update --project src/Infrastructure --startup-project src/Api
```

## Testing with Mailpit

### 1. Start Mailpit
```bash
docker-compose up -d mailpit
```

### 2. Access Web UI
Open browser: `http://localhost:8025`

### 3. Test Email Sending

#### Via API Endpoint (Testing)
```csharp
[ApiController]
[Route("api/[controller]")]
public class TestController : BaseController
{
    private readonly IEmailService _emailService;

    public TestController(IEmailService emailService)
    {
        _emailService = emailService;
    }

    [HttpPost("send-test-email")]
    public async Task<IActionResult> SendTestEmail([FromBody] string toEmail)
    {
        var result = await _emailService.SendEmailAsync(new EmailMessage
        {
            To = toEmail,
            Subject = "Test Email from SaaS Platform",
            BodyHtml = "<h1>Test Email</h1><p>This is a test email sent via Mailpit.</p>",
            Type = EmailType.Custom
        });

        return result.IsSuccess ? Ok("Email sent") : BadRequest(result.Error);
    }
}
```

### 4. Mailpit Features

#### View Emails
- All sent emails appear in the Mailpit inbox
- Click to view HTML/text versions
- Inspect headers and attachments

#### Search & Filter
- Search by recipient, subject, sender
- Filter by date range

#### API Testing
```bash
# Get all messages
curl http://localhost:8025/api/v1/messages

# Get specific message
curl http://localhost:8025/api/v1/message/{id}

# Delete all messages
curl -X DELETE http://localhost:8025/api/v1/messages
```

## Email Conventions

### 1. Naming Conventions

#### Template Names
- Use PascalCase: `UserInvitation`, `PasswordReset`
- Be descriptive: `InvoiceCreated`, `LowStockAlert`
- Prefix by module: `Auth_PasswordReset`, `Billing_InvoiceCreated` (optional)

#### Variables
- Use braces: `{VariableName}`
- PascalCase: `{CompanyName}`, `{UserName}`, `{InvoiceUrl}`
- Be specific: `{InvoiceNumber}` not `{Number}`

### 2. Email Structure

#### Subject Lines
- Keep under 50 characters
- Be specific and actionable
- Include company/product name when relevant
- Example: "Reset your password - Your SaaS"

#### Body Content
- Use responsive HTML templates
- Include plain text alternative
- Clear call-to-action button
- Footer with unsubscribe link (for marketing emails)
- Security warnings for sensitive actions

### 3. Styling Standards

#### Colors (Match PrimeVue Teal Theme)
- Primary: `#14b8a6` (Teal 500)
- Secondary: `#0f766e` (Teal 700)
- Background: `#f9fafb` (Gray 50)
- Text: `#1f2937` (Gray 800)
- Muted: `#6b7280` (Gray 500)

#### Typography
- Font: Arial, sans-serif (safe fallback)
- Headings: 24px, bold
- Body: 16px, line-height: 1.6
- Footer: 12px

#### Layout
- Max width: 600px
- Mobile-responsive
- Padding: 20-30px
- Button: min 44px height (touch-friendly)

### 4. Security Best Practices

#### Tokens
- Use cryptographically secure random tokens
- Set expiration times (password reset: 1 hour, invitation: 7 days)
- One-time use only
- Store hashed in database

#### Content
- Never include passwords in emails
- Use HTTPS URLs only
- Include security warnings for sensitive actions
- Add "If you didn't request this..." notices

#### Rate Limiting
- Limit password reset requests (3 per hour)
- Limit invitation sends (10 per day)
- Track failed send attempts

### 5. Error Handling

#### Retry Logic
- Retry failed emails (max 3 attempts)
- Exponential backoff: 5min, 30min, 2hr
- Move to dead letter queue after max retries

#### Logging
- Log all email attempts (success/failure)
- Include recipient, type, timestamp
- Store error messages
- Track delivery metrics

#### Validation
- Validate email format before sending
- Check for disposable email domains (optional)
- Verify DNS records (optional, advanced)

## Integration with Auth Flow

### User Registration
```csharp
// Command: RegisterUserCommand
public async Task<Result<LoginResponseDto>> Handle(RegisterUserCommand request)
{
    // 1. Create user and tenant
    var user = new User { Email = request.Email, ... };
    var tenant = new Tenant { Name = request.CompanyName, ... };

    // 2. Generate verification token
    var verificationToken = GenerateSecureToken();

    // 3. Send verification email
    await _emailService.SendEmailVerificationAsync(user.Email, verificationToken);

    // 4. Send welcome email
    await _emailService.SendWelcomeEmailAsync(user.Email, user.FullName, tenant.Id);

    return Result.Success(loginResponse);
}
```

### User Invitation
```csharp
// Command: InviteUserToCompanyCommand
public async Task<Result> Handle(InviteUserToCompanyCommand request)
{
    var tenantId = _tenantContext.TenantId;
    var tenant = await _tenantRepository.GetByIdAsync(tenantId);

    // Generate invitation token
    var inviteToken = GenerateSecureToken();

    // Send invitation email
    await _emailService.SendUserInvitationAsync(
        request.Email,
        tenant.Name,
        inviteToken,
        tenantId);

    return Result.Success();
}
```

## Background Job Processing (Future)

### Using Hangfire for Async Email Queue

```csharp
// Queue email for later processing
BackgroundJob.Enqueue<IEmailService>(x =>
    x.SendEmailAsync(message, CancellationToken.None));

// Schedule email
BackgroundJob.Schedule<IEmailService>(x =>
    x.SendMonthlyReportAsync(tenantId),
    TimeSpan.FromDays(30));

// Recurring email (e.g., daily digest)
RecurringJob.AddOrUpdate<IEmailService>(
    "daily-digest",
    x => x.SendDailyDigestAsync(),
    Cron.Daily);
```

## Monitoring & Analytics

### Email Metrics to Track
- **Delivery Rate**: Sent / Total
- **Failure Rate**: Failed / Total
- **Average Send Time**: Performance metric
- **Emails by Type**: Distribution chart
- **Failed Emails by Error**: Common issues

### Dashboard Queries
```csharp
// Get email statistics
public async Task<EmailStats> GetEmailStatsAsync(Guid tenantId, DateTime from, DateTime to)
{
    var logs = await _context.EmailLogs
        .Where(e => e.TenantId == tenantId && e.CreatedAt >= from && e.CreatedAt <= to)
        .ToListAsync();

    return new EmailStats
    {
        TotalSent = logs.Count(e => e.Status == EmailStatus.Sent),
        TotalFailed = logs.Count(e => e.Status == EmailStatus.Failed),
        AverageSendTime = logs.Where(e => e.SentAt.HasValue)
            .Average(e => (e.SentAt.Value - e.CreatedAt).TotalSeconds),
        ByType = logs.GroupBy(e => e.Type)
            .ToDictionary(g => g.Key, g => g.Count())
    };
}
```

## Production Considerations

### When Moving to Production

1. **Choose SMTP Provider**
   - SendGrid (12k free emails/month)
   - AWS SES (62k free emails/month for 12 months)
   - Mailgun (5k free emails/month)
   - Postmark (100 free emails/month)

2. **Update Configuration**
   ```json
   {
     "Email": {
       "SmtpHost": "smtp.sendgrid.net",
       "SmtpPort": 587,
       "SmtpUsername": "apikey",
       "SmtpPassword": "YOUR_SENDGRID_API_KEY",
       "EnableSsl": true
     }
   }
   ```

3. **Domain Verification**
   - Verify sending domain
   - Set up SPF, DKIM, DMARC records
   - Use custom FROM address

4. **Disable Mailpit**
   ```yaml
   # docker-compose.prod.yml
   # Remove or comment out mailpit service
   ```

5. **Implement Webhooks**
   - Track bounces
   - Monitor spam complaints
   - Handle unsubscribes

## API Endpoints

```
POST   /api/emails/send                    # Send custom email [admin]
GET    /api/emails/logs                    # View email logs [admin]
GET    /api/emails/logs/{id}               # View specific email [admin]
POST   /api/emails/retry/{id}              # Retry failed email [admin]
GET    /api/emails/templates               # List templates [admin]
POST   /api/emails/templates               # Create template [admin]
PUT    /api/emails/templates/{id}          # Update template [admin]
DELETE /api/emails/templates/{id}          # Delete template [admin]
POST   /api/emails/test                    # Send test email [dev only]
```

## Testing Checklist

- [ ] Mailpit container running and accessible
- [ ] Email service registered in DI
- [ ] Templates seeded for new tenants
- [ ] All transactional emails working (registration, password reset, invitation)
- [ ] Email logs created for all attempts
- [ ] Failed emails show error messages
- [ ] HTML emails render correctly
- [ ] Plain text fallback works
- [ ] Links in emails are correct
- [ ] Tokens expire properly
- [ ] Email variables replaced correctly

## Future Enhancements

- [ ] Email templates visual editor (WYSIWYG)
- [ ] Per-tenant email branding (logo, colors)
- [ ] Unsubscribe management
- [ ] Email scheduling
- [ ] Batch email sending
- [ ] Email analytics dashboard
- [ ] A/B testing for email content
- [ ] Multi-language templates with i18n
- [ ] Email previews before sending
- [ ] Webhook integration for delivery status

## References

- **Mailpit**: https://github.com/axllent/mailpit
- **MailKit**: https://github.com/jstedfast/MailKit
- **MimeKit**: https://github.com/jstedfast/MimeKit
- **Related Agents**: Backend Agent, Auth Agent
- **Key Files**:
  - `Infrastructure/Services/EmailService.cs`
  - `Domain/Entities/EmailLog.cs`, `EmailTemplate.cs`
  - `docker-compose.yml` (Mailpit configuration)
