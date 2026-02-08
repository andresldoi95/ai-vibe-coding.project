# Email Templates Upgrade - MJML Implementation

## Summary

Successfully redesigned all email templates using **MJML framework** with modern design, branding elements, and **dark mode support**.

## What Changed

### Before (Old Implementation)
- ✗ Basic HTML with minimal inline CSS
- ✗ Plain 2010-style design
- ✗ No dark mode support
- ✗ Limited responsiveness
- ✗ No branding consistency
- ✗ Manual HTML maintenance

### After (New Implementation)
- ✅ MJML framework with professional responsive design
- ✅ Modern, clean UI with improved typography and spacing
- ✅ Full dark mode support (CSS media queries)
- ✅ Branded header with logo placeholder
- ✅ Consistent color scheme (Teal #14b8a6 primary)
- ✅ Maintainable MJML source files with compilation workflow
- ✅ Email client compatibility (Gmail, Outlook, Apple Mail)

## Templates Upgraded

1. **PasswordReset.mjml** → `dist/PasswordReset.html`
   - Security-focused design with warning boxes
   - Clear CTA button with expiration notice
   - Alternative link fallback
   - Dark mode: Dark backgrounds, adjusted colors

2. **WelcomeEmail.mjml** → `dist/WelcomeEmail.html`
   - Hero section with colorful welcome banner
   - Feature boxes showcasing platform capabilities
   - Getting started steps with icons
   - Dark mode: Optimized for readability

3. **UserInvitation.mjml** → `dist/UserInvitation.html`
   - Professional invitation card design
   - Clear company/role information display
   - Highlighted invitation details box
   - Dark mode: Consistent with brand

## New Infrastructure

### Directory Structure
```
EmailTemplates/
├── src/              # MJML source files (.mjml)
├── dist/             # Compiled HTML (auto-generated)
├── assets/           # Logo and images
├── old-templates/    # Legacy HTML (backup)
├── package.json      # MJML dependencies
├── compile-templates.ps1  # Build script
├── test-templates.ps1     # Testing script
└── README.md              # Template documentation
```

### Build Workflow
- **Compile**: `.\compile-templates.ps1` (PowerShell)
- **Watch Mode**: `npm run watch` (auto-recompile)
- **Test**: `.\test-templates.ps1` (send to Mailpit)

### Backend Changes
- Updated `EmailService.cs` to load from `dist/` folder
- Template path: `Infrastructure/Data/EmailTemplates/dist/{TemplateName}.html`
- Backward compatible with fallback mechanism

## Design System

### Color Palette
- **Primary**: `#14b8a6` (Teal) - Buttons, links, brand elements
- **Secondary**: `#0891b2` (Cyan) - Accents
- **Success**: `#10b981` (Green)
- **Warning**: `#f59e0b` (Amber) - Security notices
- **Error**: `#ef4444` (Red)

### Dark Mode Colors
- **Background**: `#111827` (Very dark)
- **Cards**: `#1f2937` (Dark gray)
- **Text**: `#f3f4f6` (Light)
- **Primary**: `#5eead4` (Light teal for dark backgrounds)

### Typography
- **Font Family**: 'Segoe UI', 'Helvetica Neue', Arial, sans-serif
- **Headings**: 28px, font-weight 700
- **Body**: 14-16px, line-height 1.6-1.7
- **Small Text**: 12-13px for footer/legal

### Spacing
- **Sections**: 32-40px padding
- **Elements**: 16-24px margins between content blocks
- **Mobile**: Responsive padding adjustments

## Template Variables

All templates use `{VariableName}` syntax:

### PasswordReset
- `{userName}` - Display name
- `{resetLink}` - Reset URL
- `{expiryTime}` - Link expiry
- `{frontendUrl}` - Base URL

### WelcomeEmail
- `{userName}` - Display name
- `{companyName}` - Tenant name
- `{loginLink}` - Login URL
- `{frontendUrl}` - Base URL

### UserInvitation
- `{inviteeName}` - Invited user name
- `{inviterName}` - Sender name
- `{companyName}` - Company name
- `{invitationLink}` - Invite URL
- `{role}` - User role
- `{frontendUrl}` - Base URL

## Testing

### Quick Test
```powershell
# 1. Ensure Mailpit running
docker-compose up mailpit

# 2. Compile templates (if modified)
cd backend/src/Infrastructure/Data/EmailTemplates
.\compile-templates.ps1

# 3. Send test emails
.\test-templates.ps1

# 4. View in Mailpit
# Open http://localhost:8025
```

### Manual API Testing
```bash
# Password Reset
POST http://localhost:5000/api/v1/testemail/send-password-reset
Body: "test@example.com"

# Welcome Email
POST http://localhost:5000/api/v1/testemail/send-welcome
Body: {"email":"user@example.com","userName":"John Doe","tenantId":"00000000-0000-0000-0000-000000000000"}

# Invitation
POST http://localhost:5000/api/v1/testemail/send-invitation
Body: {"email":"invited@example.com","companyName":"Acme Corp","tenantId":"00000000-0000-0000-0000-000000000000"}
```

## Dark Mode Support

### How It Works
- Uses CSS media query: `@media (prefers-color-scheme: dark)`
- Automatically detects user's system/email client preference
- No JavaScript required
- Gracefully degrades in unsupported clients

### Client Support
- ✅ **Apple Mail** (macOS, iOS) - Full support
- ✅ **Gmail App** (iOS, Android) - Full support
- ✅ **Outlook Mobile** - Full support
- ⚠️ **Gmail Web** - Limited/no support (uses light mode)
- ⚠️ **Outlook Desktop** - No support (Windows default theme)

### Testing Dark Mode
1. Set your system to dark mode
2. Open email in Apple Mail or Gmail app
3. Email should automatically use dark colors
4. Toggle system theme to verify switching

## Future Improvements

### Potential Enhancements
- [ ] Add per-tenant branding (custom logos, colors)
- [ ] Implement email preview system in admin UI
- [ ] A/B testing for email templates
- [ ] Create remaining 5 email types (Invoice, Payment, Stock Alert, etc.)
- [ ] Add social media icons/links in footer
- [ ] Multi-language template support (i18n)
- [ ] Email journey analytics (open rates, click rates)

## Migration Notes

### For Developers
- **Old templates backed up** in `old-templates/` folder
- **EmailService updated** to read from `dist/` folder
  - File: `backend/src/Infrastructure/Services/EmailService.cs`
  - Line 236: Updated template path
- **No breaking changes** to EmailService API
- **Existing code** works without modification

### For Designers
- Edit `.mjml` files in `src/` directory (not HTML directly)
- Compile after changes: `.\compile-templates.ps1`
- Use MJML components: https://documentation.mjml.io/
- Test locally with Mailpit before production

### Production Deployment
1. Ensure Node.js installed on build server
2. Run `npm install` in EmailTemplates directory
3. Run `npm run build` before deployment
4. Deploy compiled `dist/` folder with backend
5. Verify templates load from `dist/` in production logs

## Resources

- **MJML Documentation**: https://documentation.mjml.io/
- **Email Client CSS Support**: https://www.caniemail.com/
- **Mailpit Web UI**: http://localhost:8025 (local dev)
- **Template README**: `backend/src/Infrastructure/Data/EmailTemplates/README.md`
- **Email Agent Docs**: `docs/email-agent.md`

## Files Modified

### Created
- `backend/src/Infrastructure/Data/EmailTemplates/src/*.mjml` (3 templates)
- `backend/src/Infrastructure/Data/EmailTemplates/dist/*.html` (compiled)
- `backend/src/Infrastructure/Data/EmailTemplates/package.json`
- `backend/src/Infrastructure/Data/EmailTemplates/compile-templates.ps1`
- `backend/src/Infrastructure/Data/EmailTemplates/test-templates.ps1`
- `backend/src/Infrastructure/Data/EmailTemplates/README.md`
- `backend/src/Infrastructure/Data/EmailTemplates/assets/logo.svg`
- `backend/src/Infrastructure/Data/EmailTemplates/.gitignore`

### Modified
- `backend/src/Infrastructure/Services/EmailService.cs` - Updated template path
- `docs/email-agent.md` - Added MJML documentation

### Moved
- `backend/src/Infrastructure/Data/EmailTemplates/*.html` → `old-templates/` (backup)

---

**Result**: Professional, modern email templates ready for production use! 🎉
