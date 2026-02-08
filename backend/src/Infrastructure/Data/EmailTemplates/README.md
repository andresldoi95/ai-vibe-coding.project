# Email Templates

Professional email templates built with MJML framework, featuring responsive design, dark mode support, and brand consistency.

## 📁 Directory Structure

```
EmailTemplates/
├── src/              # MJML source files (.mjml)
├── dist/             # Compiled HTML files (generated)
├── assets/           # Images, logos, icons
├── package.json      # MJML dependencies
└── compile-templates.ps1  # Compilation script
```

## 🚀 Quick Start

### Prerequisites
- Node.js 18+ installed
- npm package manager

### Compile Templates

**Windows PowerShell:**
```powershell
.\compile-templates.ps1
```

**Manual compilation:**
```bash
npm install
npm run build
```

**Watch mode (auto-recompile on changes):**
```bash
npm run watch
```

## 📧 Available Templates

| Template | File | Purpose |
|----------|------|---------|
| Password Reset | `PasswordReset.mjml` | Secure password reset links |
| Welcome Email | `WelcomeEmail.mjml` | Onboard new users |
| User Invitation | `UserInvitation.mjml` | Invite users to join company |

## 🎨 Branding & Customization

### Color Palette
- **Primary**: `#14b8a6` (Teal)
- **Secondary**: `#0891b2` (Cyan)
- **Success**: `#10b981` (Green)
- **Warning**: `#f59e0b` (Amber)
- **Error**: `#ef4444` (Red)
- **Dark Mode Background**: `#1a1a1a`
- **Dark Mode Text**: `#e5e5e5`

### Template Variables

Templates use `{VariableName}` syntax for dynamic content:

**PasswordReset.mjml:**
- `{userName}` - User's display name
- `{resetLink}` - Password reset URL
- `{expiryTime}` - Link expiration time

**WelcomeEmail.mjml:**
- `{userName}` - User's display name
- `{loginLink}` - Login page URL
- `{companyName}` - Company/tenant name

**UserInvitation.mjml:**
- `{inviteeName}` - Person being invited
- `{inviterName}` - Person sending invite
- `{companyName}` - Company name
- `{invitationLink}` - Acceptance URL
- `{role}` - User role

## 🌙 Dark Mode Support

All templates automatically adapt to dark mode using CSS media queries:

```css
@media (prefers-color-scheme: dark) {
  /* Styles automatically applied in dark mode */
}
```

Tested in:
- ✅ Apple Mail (macOS, iOS)
- ✅ Gmail App (Android, iOS)
- ✅ Outlook Mobile
- ⚠️ Gmail Web (limited support)

## 🏗️ Creating New Templates

1. **Create MJML file** in `src/`:
   ```bash
   touch src/NewTemplate.mjml
   ```

2. **Use base structure**:
   ```mjml
   <mjml>
     <mj-head>
       <!-- Include base styles -->
     </mj-head>
     <mj-body>
       <!-- Your content -->
     </mj-body>
   </mjml>
   ```

3. **Compile**:
   ```bash
   npm run build
   ```

4. **Test** in Mailpit (http://localhost:8025)

## 🧪 Testing

### Local Testing with Mailpit

1. Start Mailpit:
   ```powershell
   docker-compose up mailpit
   ```

2. Compile templates:
   ```powershell
   .\compile-templates.ps1
   ```

3. Trigger test email via API:
   ```bash
   POST http://localhost:5000/api/test-email/password-reset
   ```

4. View in Mailpit: http://localhost:8025

### Email Client Testing

Test compiled HTML in:
- Gmail (web + mobile app)
- Outlook (desktop + mobile)
- Apple Mail
- Yahoo Mail
- ProtonMail

## 📚 MJML Resources

- [MJML Documentation](https://documentation.mjml.io/)
- [MJML Components](https://documentation.mjml.io/mjml-tags/)
- [MJML Try It Live](https://mjml.io/try-it-live)

## 🔧 Troubleshooting

**Templates not compiling?**
- Check MJML syntax with `npm run build`
- Validate at https://mjml.io/try-it-live

**Variables not replacing?**
- Ensure `{VariableName}` format in MJML source
- Check EmailService variable dictionary

**Dark mode not working?**
- Test in supported email client (Apple Mail, Gmail Mobile)
- Verify `@media (prefers-color-scheme: dark)` in compiled HTML

## 📝 Notes

- Always compile MJML to HTML before deploying
- Commit both `.mjml` source and `.html` compiled files
- EmailService reads from `dist/` folder
- Use inline CSS only (MJML handles this automatically)
