# SaaS Billing & Inventory - Frontend

Modern, multi-tenant SaaS application for billing and inventory management built with Nuxt 3, PrimeVue, and TypeScript.

## 🚀 Tech Stack

- **Framework**: Nuxt 3
- **UI Library**: PrimeVue 4+ (Teal theme with dark/light mode)
- **CSS**: Tailwind CSS
- **State Management**: Pinia
- **Language**: TypeScript
- **i18n**: Multi-language support (EN, ES, FR, DE)
- **Icons**: PrimeIcons
- **Validation**: Vuelidate

## ✨ Features

- 🎨 **Modern UI**: PrimeVue components with Teal theme
- 🌓 **Dark/Light Mode**: Automatic theme switching
- 🌍 **Multi-language**: Support for 4 languages out of the box
- 🏢 **Multi-tenant**: Full tenant isolation and context management
- 🔐 **Authentication**: JWT-based auth with refresh tokens
- 📱 **Responsive**: Mobile-first design
- 🎯 **Type-safe**: Full TypeScript support
- 🐳 **Containerized**: Docker support for dev and production

## 📋 Prerequisites

- Node.js 20+
- npm or yarn
- Docker (optional, for containerized deployment)

## 🛠️ Installation

### Local Development

1. **Install dependencies**:
```bash
npm install
```

2. **Configure environment**:
```bash
cp .env.example .env
```

Edit `.env` and set your API base URL:
```
NUXT_PUBLIC_API_BASE=http://localhost:5000/api/v1
```

3. **Run development server**:
```bash
npm run dev
```

The application will be available at `http://localhost:3000`

### Docker Development

1. **Start with Docker Compose**:
```bash
cd ..
docker-compose up
```

The application will be available at `http://localhost:3000` with hot reload enabled.

### Production Build

1. **Build the application**:
```bash
npm run build
```

2. **Preview production build**:
```bash
npm run preview
```

### Docker Production

1. **Build and run with Docker Compose**:
```bash
cd ..
docker-compose -f docker-compose.prod.yml up --build
```

## 📁 Project Structure

```
frontend/
├── assets/
│   └── styles/
│       └── main.css          # Tailwind imports
├── components/
│   └── shared/
│       └── LanguageSwitcher.vue
├── composables/
│   ├── useApi.ts             # API client
│   ├── useTheme.ts           # Theme management
│   ├── useToast.ts           # Toast notifications
│   └── useFormatters.ts      # Date/number formatting
├── layouts/
│   ├── default.vue           # Main layout with navigation
│   └── auth.vue              # Authentication layout
├── locales/
│   ├── en.json               # English translations
│   ├── es.json               # Spanish translations
│   ├── fr.json               # French translations
│   └── de.json               # German translations
├── middleware/
│   ├── auth.ts               # Authentication guard
│   └── tenant.ts             # Tenant context guard
├── pages/
│   ├── index.vue             # Dashboard
│   └── login.vue             # Login page
├── plugins/
│   ├── primevue.ts           # PrimeVue configuration
│   └── api.ts                # API plugin
├── stores/
│   ├── auth.ts               # Authentication store
│   ├── tenant.ts             # Tenant management store
│   └── ui.ts                 # UI state store
├── types/
│   ├── auth.ts               # Auth types
│   ├── tenant.ts             # Tenant types
│   ├── billing.ts            # Billing module types
│   ├── inventory.ts          # Inventory module types
│   └── api.ts                # API response types
├── nuxt.config.ts            # Nuxt configuration
├── tailwind.config.ts        # Tailwind configuration
└── tsconfig.json             # TypeScript configuration
```

## 🎨 Theme Configuration

The application uses PrimeVue's Lara theme in Teal color:
- **Light Mode**: `lara-light-teal`
- **Dark Mode**: `lara-dark-teal`

Themes are automatically loaded based on user preference and can be toggled via the theme button in the navigation.

## 🌍 Internationalization

Supported languages:
- English (en)
- Spanish (es)
- French (fr)
- German (de)

Add new languages by:
1. Creating a new JSON file in `locales/`
2. Adding the locale configuration in `nuxt.config.ts`
3. Importing PrimeVue locale in the theme plugin

## 🔐 Authentication

The app uses JWT-based authentication:
- Access token stored in Pinia + localStorage
- Refresh token for automatic token renewal
- Auto-logout on 401 responses
- Protected routes with auth middleware

## 🏢 Multi-tenant Support

Tenant context is managed via:
- Tenant selection dropdown in navigation
- Tenant ID automatically added to all API requests via `X-Tenant-Id` header
- Tenant state persisted in localStorage
- Auto-selection of first available tenant on login

## 📝 Available Scripts

- `npm run dev` - Start development server
- `npm run build` - Build for production
- `npm run preview` - Preview production build
- `npm run generate` - Generate static site
- `npm run lint` - Lint code
- `npm run typecheck` - Run TypeScript type checking

## 🐳 Docker Commands

### Development
```bash
# Start development environment
docker-compose up

# Stop and remove containers
docker-compose down

# View logs
docker-compose logs -f frontend
```

### Production
```bash
# Build and start production environment
docker-compose -f docker-compose.prod.yml up --build -d

# Stop production environment
docker-compose -f docker-compose.prod.yml down

# View logs
docker-compose -f docker-compose.prod.yml logs -f frontend
```

## 🔧 Configuration

### Environment Variables

- `NUXT_PUBLIC_API_BASE` - Backend API base URL (default: `http://localhost:5000/api/v1`)

### Nuxt Config

Key configurations in `nuxt.config.ts`:
- PrimeVue module with auto-import
- Tailwind CSS integration
- Color mode for dark/light themes
- i18n with lazy loading
- TypeScript strict mode

## 📦 Key Dependencies

- `nuxt` - The Nuxt 3 framework
- `primevue` - UI component library
- `@primevue/nuxt-module` - PrimeVue Nuxt integration
- `@nuxtjs/tailwindcss` - Tailwind CSS module
- `@nuxtjs/color-mode` - Dark/light mode support
- `@nuxtjs/i18n` - Internationalization
- `pinia` - State management
- `@vuelidate/core` - Form validation

## 🎯 Best Practices

1. **Components**: Use PrimeVue components without heavy customization
2. **Styling**: Prefer Tailwind utilities over custom CSS
3. **Types**: Always define TypeScript interfaces for data structures
4. **State**: Use Pinia stores for shared state
5. **i18n**: Never hardcode text - use translation keys
6. **Layouts**: Use Tailwind for responsive design
7. **Theme**: Test both light and dark modes

## 🚀 Next Steps

1. Connect to backend API
2. Implement billing module pages
3. Implement inventory module pages
4. Add data tables with CRUD operations
5. Add charts and analytics
6. Implement user profile and settings

## 📄 License

MIT

## 👥 Author

SaaS Billing & Inventory Team
