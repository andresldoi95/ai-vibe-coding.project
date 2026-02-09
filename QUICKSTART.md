# Quick Start Guide

This guide will help you get the SaaS Billing & Inventory frontend up and running in minutes.

## 🎯 Prerequisites

Before you begin, ensure you have:

- ✅ Node.js 20 or higher installed
- ✅ npm or yarn package manager
- ✅ Docker and Docker Compose (optional, but recommended)

## 🚀 Quick Setup (5 minutes)

### Option 1: Local Development (Without Docker)

1. **Navigate to frontend directory**:

```bash
cd frontend
```

2. **Install dependencies**:

```bash
npm install
```

3. **Create environment file**:

```bash
cp .env.example .env
```

4. **Start development server**:

```bash
npm run dev
```

5. **Open your browser**:
   Navigate to `http://localhost:3000`

### Option 2: Docker Development (Recommended)

1. **From project root, start Docker Compose**:

```bash
docker-compose up
```

2. **Wait for build to complete** (first time takes 2-3 minutes)

3. **Open your browser**:
   Navigate to `http://localhost:3000`

That's it! 🎉

## 🎨 What You'll See

When you first load the application:

1. **Login Page**:
   - Default theme: Light mode with Teal accent
   - Demo credentials shown on the page

2. **Test the Features**:
   - Toggle dark/light mode (moon/sun icon)
   - Switch languages (globe icon dropdown)
   - Try responsive design (resize browser)

## 🔐 Demo Credentials

For testing (when backend is connected):

```
Email: admin@example.com
Password: password
```

### Reset Database with Demo Data

Use the **reset-demo-data.ps1** script to completely reset your database and seed it with demo data:

```powershell
# Interactive mode (asks for confirmation)
.\reset-demo-data.ps1

# Unattended mode (skips confirmation)
.\reset-demo-data.ps1 -Force
```

This will:
- **Drop and recreate the database** (⚠️ ALL DATA WILL BE LOST)
- Apply all migrations
- Create a demo company: **Demo Company** (`demo-company`)
- Create 4 demo users with different roles
- Create 3 sample warehouses

**Demo Users** (all passwords are `password`):
- **Owner**: owner@demo.com
- **Admin**: admin@demo.com
- **Manager**: manager@demo.com
- **User**: user@demo.com

**Use Case**: Perfect for:
- Starting fresh with clean test data
- Testing different user roles and permissions
- Demonstrating the application
- Development and testing workflows

## 🎨 Theme Testing

- Click the **moon icon** 🌙 to switch to dark mode
- Click the **sun icon** ☀️ to switch back to light mode
- Theme preference is automatically saved

## 🌍 Language Testing

- Click the **globe icon** 🌐 to open language selector
- Choose from: English, Spanish, French, or German
- All UI text updates immediately

## 📁 Project Structure Overview

```
frontend/
├── pages/          # Your pages (index.vue = dashboard, login.vue = login)
├── components/     # Reusable UI components
├── layouts/        # Layout templates (default, auth)
├── stores/         # Pinia stores (state management)
├── composables/    # Reusable Vue composition functions
├── types/          # TypeScript type definitions
├── locales/        # Translation files (en, es, fr, de)
└── middleware/     # Route guards (auth, tenant)
```

## 🛠️ Available Commands

**Development**:

```bash
npm run dev         # Start dev server with hot reload
npm run build       # Build for production
npm run preview     # Preview production build
```

**Quality**:

```bash
npm run typecheck   # Check TypeScript types
npm run lint        # Lint code
```

## 🐳 Docker Commands

**Start**:

```bash
docker-compose up              # Start in foreground
docker-compose up -d           # Start in background (detached)
```

**Stop**:

```bash
docker-compose down            # Stop and remove containers
```

**Logs**:

```bash
docker-compose logs -f         # Follow all logs
docker-compose logs -f frontend # Follow frontend logs only
```

**Rebuild**:

```bash
docker-compose up --build      # Rebuild and start
```

## 🎯 Next Steps

### 1. Explore the UI

- Navigate through the menu items
- Check out the dashboard
- Try the login page

### 2. Customize

- Edit `locales/en.json` to change text
- Modify `layouts/default.vue` to adjust navigation
- Update `pages/index.vue` to customize dashboard

### 3. Connect to Backend

- Update `.env` file with your backend API URL
- Configure CORS on your backend
- Test API integration

### 4. Add Features

- Create new pages in `pages/` directory
- Add components in `components/` directory
- Define types in `types/` directory

## 🔧 Configuration Files

| File                 | Purpose                                     |
| -------------------- | ------------------------------------------- |
| `nuxt.config.ts`     | Nuxt configuration (modules, plugins, etc.) |
| `tailwind.config.ts` | Tailwind CSS configuration                  |
| `tsconfig.json`      | TypeScript configuration                    |
| `.env`               | Environment variables                       |

## 🎨 Using PrimeVue Components

PrimeVue components are auto-imported. Just use them in your templates:

```vue
<template>
  <Button label="Click me" icon="pi pi-check" />
  <DataTable :value="items" :paginator="true" :rows="10">
    <Column field="name" header="Name" />
  </DataTable>
</template>
```

Available components:

- Button, InputText, Dropdown, Calendar
- DataTable, Card, Panel, Dialog
- Toast, Message, ConfirmDialog
- And 80+ more!

See: https://primevue.org/components

## 🌟 Best Practices

1. **Always use TypeScript types**:

```typescript
interface MyData {
  id: string;
  name: string;
}
```

2. **Use composables for reusable logic**:

```typescript
const { apiFetch } = useApi();
const toast = useToast();
```

3. **Internationalize all text**:

```vue
<template>
  <h1>{{ t("dashboard.welcome") }}</h1>
</template>
```

4. **Test in both themes**:

- Light mode ☀️
- Dark mode 🌙

## ❓ Troubleshooting

### Port 3000 already in use

```bash
# Kill process on port 3000
npx kill-port 3000

# Or change port in nuxt.config.ts
```

### Module not found errors

```bash
# Clear cache and reinstall
rm -rf node_modules .nuxt
npm install
```

### Docker build fails

```bash
# Clean Docker cache
docker-compose down -v
docker system prune -a
docker-compose up --build
```

### Hot reload not working

- Restart dev server
- Check file watchers limit (Linux)
- Disable antivirus/firewall temporarily

## 📚 Learn More

- **Nuxt 3**: https://nuxt.com/docs
- **PrimeVue**: https://primevue.org/
- **Tailwind CSS**: https://tailwindcss.com/
- **Pinia**: https://pinia.vuejs.org/
- **TypeScript**: https://www.typescriptlang.org/

## 🎉 You're Ready!

You now have a fully functional, production-ready frontend application with:

- ✅ Modern UI with PrimeVue
- ✅ Dark/Light theme support
- ✅ Multi-language support
- ✅ TypeScript for type safety
- ✅ Responsive design
- ✅ Docker containerization

Happy coding! 🚀
