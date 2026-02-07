# Frontend Agent

## Specialization

Expert in Nuxt 3 + TypeScript frontend development with PrimeVue (Teal theme) and Tailwind CSS for the SaaS Billing + Inventory Management System. Focuses on rapid development using PrimeVue default components and standards with minimal customization.

## Tech Stack

- **Framework**: Nuxt 3 with TypeScript
- **UI Library**: PrimeVue 4+ (Aura preset, design token-based theming)
- **Theming**: @primeuix/themes with Aura preset (customizable with teal primary color)
- **CSS Framework**: Tailwind CSS (for layouts and utilities)
- **State Management**: Pinia
- **HTTP Client**: Nuxt $fetch / ofetch
- **Form Validation**: Vuelidate / Zod
- **Icons**: PrimeIcons
- **Date Handling**: date-fns or Day.js
- **Charts**: PrimeVue Chart component (Chart.js wrapper)
- **Theme**: Dark/Light mode support with PrimeVue v4 design tokens
- **i18n**: Multi-language support with @nuxtjs/i18n

## Code Quality & Standards

### ESLint Configuration

The project uses [@antfu/eslint-config](https://github.com/antfu/eslint-config) which enforces Anthony Fu's opinionated ESLint rules for TypeScript and Vue projects. **All generated code MUST comply with these ESLint rules.**

#### Key ESLint Rules to Follow

**1. String Quotes**

- Always use **single quotes** for strings
- ❌ `const name = "John"`
- ✅ `const name = 'John'`

**2. No Semicolons**

- Do NOT use semicolons at the end of statements
- ❌ `const x = 1;`
- ✅ `const x = 1`

**3. Trailing Commas**

- Always add trailing commas in multi-line arrays, objects, and function parameters
- ❌ `const obj = { a: 1, b: 2 }`
- ✅ `const obj = { a: 1, b: 2, }`

**4. No Trailing Spaces**

- Remove all trailing whitespace from lines
- Configure your editor to trim trailing whitespace automatically

**5. Top-Level Functions**

- Use `function` keyword for top-level function declarations
- ❌ `const myFunction = () => { ... }`
- ✅ `function myFunction() { ... }`

**6. Unused Variables**

- Prefix unused function parameters with underscore `_`
- ❌ `function handler(event: Event) { /* event not used */ }`
- ✅ `function handler(_event: Event) { ... }`
- Remove completely unused variables

**7. No `any` Type**

- Never use `any` type in TypeScript
- Use proper type definitions or `unknown` if type is truly unknown
- ❌ `catch (error: any) { error.message }`
- ✅ `catch (error) { const errMessage = error instanceof Error ? error.message : 'Unknown error' }`

**8. Object Shorthand**

- Use ES6 object property shorthand
- ❌ `return { data: data, loading: loading }`
- ✅ `return { data, loading }`

**9. Brace Style**

- Use consistent brace style (1tbs - one true brace style)
- Opening brace on same line, closing brace on new line
- ❌ `if (condition) { doSomething() } else { doOther() }`
- ✅ `if (condition) { doSomething() } \n else { doOther() }`

**10. Vue-Specific Rules**

- Use `import.meta.client` instead of `process.client` in components
- Hyphenate component attribute names (`:option-label` not `:optionLabel`)
- Self-close HTML void elements (`<i />` not `<i></i>`)
- Remove unused template refs
- Follow Vue attribute order conventions

**11. Process/Node Globals**

- In `nuxt.config.ts`, use `process.env` (add `eslint-disable-next-line` if needed)
- In components/composables, use `import.meta.env` for environment variables
- Use `import.meta.client` for client-side checks

**12. Imports**

- Sort and organize imports
- Remove unused imports automatically

#### Running ESLint

```bash
# Check for ESLint issues
npm run lint

# Auto-fix ESLint issues (recommended after generating code)
npm run lint:fix
```

#### Pre-commit Checklist

Before committing code, ensure:

1. ✅ Run `npm run lint:fix` to auto-fix formatting issues
2. ✅ Run `npm run lint` to verify no remaining errors
3. ✅ Fix any remaining errors manually
4. ✅ Run `npm run typecheck` to verify TypeScript types

**IMPORTANT**: Always generate code that passes ESLint validation. When creating new files or modifying existing ones, follow the ESLint configuration strictly to avoid formatting inconsistencies.

## Core Responsibilities

### 1. Project Structure & Organization

#### Nuxt 3 Directory Structure

```
/frontend/
├── assets/
│   └── styles/
│       └── main.css (Tailwind imports)
├── components/
│   ├── billing/ (billing-specific components)
│   ├── inventory/ (inventory-specific components)
│   └── shared/ (reusable components)
│       ├── DataTableActions.vue
│       ├── EmptyState.vue
│       ├── LanguageSwitcher.vue
│       ├── LoadingState.vue
│       ├── PageHeader.vue
│       ├── StatCard.vue
│       └── ThemeSwitcher.vue
├── composables/
│   ├── useApi.ts (API client wrapper)
│   ├── useFormatters.ts (date, currency, number formatters)
│   ├── useNotification.ts (toast notifications)
│   ├── useTheme.ts (dark/light theme management)
│   └── useWarehouse.ts (warehouse API integration - example pattern)
├── i18n/
│   └── locales/
│       ├── en.json (English translations)
│       ├── es.json (Spanish translations)
│       ├── fr.json (French translations)
│       └── de.json (German translations)
├── layouts/
│   ├── default.vue (authenticated app layout)
│   └── auth.vue (login/authentication layout)
├── middleware/
│   ├── auth.ts (authentication guard)
│   └── tenant.ts (tenant resolution)
├── pages/
│   ├── index.vue (dashboard)
│   ├── login.vue (login page)
│   ├── billing/
│   │   └── invoices/
│   │       └── index.vue
│   └── inventory/
│       ├── products/
│       │   └── index.vue
│       └── warehouses/  # Full CRUD implementation example
│           ├── index.vue (list page with DataTable)
│           ├── new.vue (create form)
│           └── [id]/
│               ├── index.vue (view/details page)
│               └── edit.vue (edit form)
├── plugins/
│   └── api.ts (API plugin configuration)
├── stores/
│   ├── auth.ts (authentication state)
│   ├── tenant.ts (tenant context)
│   └── ui.ts (UI state, breadcrumbs)
├── types/
│   ├── api.ts (API types)
│   ├── auth.ts (authentication types)
│   ├── billing.ts (billing domain types)
│   ├── inventory.ts (inventory domain types - includes Warehouse interface)
│   └── tenant.ts (tenant types)
├── utils/
│   ├── constants.ts (app constants)
│   ├── helpers.ts (utility functions)
│   ├── status.ts (status utilities)
│   └── validators.ts (validation helpers)
└── nuxt.config.ts
```

### 2. PrimeVue Integration & Theming

#### PrimeVue v4 Setup with Aura Preset

**Installation**:
```bash
npm install primevue @primeuix/themes primeicons
```

**Configuration Approach**:
- Use PrimeVue v4 with design token-based theming
- Use Aura preset as the base (PrimeTek's recommended design system)
- Customize primary color to teal using `definePreset`
- Use unstyled mode: **NO** (use default styled components)
- Configure auto-import for PrimeVue components
- Import PrimeIcons CSS
- No custom theme building - customize via design tokens only

#### Theme Configuration (Dark/Light Mode)

**PrimeVue v4 Plugin Setup**:
```typescript
// plugins/primevue.ts
import PrimeVue from 'primevue/config'
import { definePreset } from '@primeuix/themes'
import Aura from '@primeuix/themes/aura'

// Customize Aura preset with teal primary color
const TealPreset = definePreset(Aura, {
  semantic: {
    primary: {
      50: '{teal.50}',
      100: '{teal.100}',
      200: '{teal.200}',
      300: '{teal.300}',
      400: '{teal.400}',
      500: '{teal.500}',
      600: '{teal.600}',
      700: '{teal.700}',
      800: '{teal.800}',
      900: '{teal.900}',
      950: '{teal.950}',
    },
  },
})

export default defineNuxtPlugin((nuxtApp) => {
  nuxtApp.vueApp.use(PrimeVue, {
    theme: {
      preset: TealPreset,
      options: {
        prefix: 'p',
        darkModeSelector: '.app-dark',
        cssLayer: false,
      },
    },
  })
})
```

**Key Features**:
- Design token-based architecture (primitive, semantic, component tokens)
- Support both light and dark themes via `darkModeSelector`
- Implement theme toggle in user menu
- Persist theme preference in localStorage
- Sync with system preferences (optional)

**No CSS Imports Needed** - PrimeVue v4 handles styling via design tokens:
```typescript
// In plugins/primevue.ts - NO CSS imports needed!
import 'primeicons/primeicons.css' // Only PrimeIcons CSS needed
```

#### Dark Mode Implementation with PrimeVue v4

```typescript
// composables/useTheme.ts
export function useTheme() {
  const colorMode = useColorMode()

  const isDark = computed(() => colorMode.value === 'dark')

  const toggleTheme = () => {
    colorMode.preference = colorMode.value === 'dark' ? 'light' : 'dark'
  }

  const setTheme = (theme: 'light' | 'dark') => {
    colorMode.preference = theme
  }

  // Apply dark mode class to html element for PrimeVue v4
  const applyThemeClass = (theme: 'light' | 'dark') => {
    if (import.meta.client) {
      const htmlElement = document.documentElement
      if (theme === 'dark') {
        htmlElement.classList.add('app-dark')
      }
      else {
        htmlElement.classList.remove('app-dark')
      }
    }
  }

  watch(
    () => colorMode.value,
    (newTheme) => {
      applyThemeClass(newTheme as 'light' | 'dark')
    },
    { immediate: true },
  )

  return { isDark, toggleTheme, setTheme }
}
```

**PrimeVue v4 Design Token Benefits**:
- No dynamic CSS loading required
- Theme changes via design tokens automatically apply
- Better performance (no CSS file switching)
- Easier customization via `definePreset` and `updatePreset`
- Full TypeScript support for design tokens

**PrimeVue v4 Documentation References**:
- Homepage: https://primevue.org/
- Installation: https://primevue.org/installation/
- Styled Mode & Theming: https://primevue.org/theming/styled/
- Configuration: https://primevue.org/configuration/
- All Components: Browse from homepage navigation
```

#### Tailwind Dark Mode Setup

```typescript
// tailwind.config.ts
export default {
  darkMode: "class", // Use class-based dark mode
  content: [
    "./components/**/*.{vue,ts}",
    "./layouts/**/*.vue",
    "./pages/**/*.vue",
    "./app.vue",
  ],
  theme: {
    extend: {},
  },
  plugins: [],
};
```

#### Nuxt Color Mode Module

```typescript
// nuxt.config.ts
export default defineNuxtConfig({
  modules: ["@nuxtjs/color-mode"],
  colorMode: {
    classSuffix: "", // Use 'dark' class instead of 'dark-mode'
    preference: "system", // Default to system preference
    fallback: "light", // Fallback if system preference not available
  },
});
```

#### Component Usage Standards

- **Always use PrimeVue components when available**
- No custom recreations of existing PrimeVue components
- Use default component configurations
- Minimal prop customization - stick to essentials
- Leverage PrimeVue's built-in features (filtering, sorting, pagination)

#### Core PrimeVue Components to Use

- **Layout**: Menubar, Sidebar, Toolbar, Breadcrumb, Divider
- **Forms**: InputText, InputNumber, Calendar, Dropdown, MultiSelect, AutoComplete, Checkbox, RadioButton, Textarea
- **Buttons**: Button, SplitButton, SpeedDial
- **Data**: DataTable, DataView, Tree, TreeTable, Paginator
- **Panels**: Panel, Card, Fieldset, Accordion, TabView, Toolbar
- **Overlays**: Dialog, ConfirmDialog, Sidebar, OverlayPanel
- **Messages**: Toast, Message, InlineMessage, ConfirmPopup
- **Menus**: Menu, PanelMenu, MegaMenu, ContextMenu
- **Charts**: Chart (for analytics and reporting)
- **Misc**: ProgressBar, Skeleton, Tag, Badge, Chip, Avatar

### 3. Tailwind CSS Integration

#### Tailwind Usage Guidelines

- Use for **layout utilities**: flex, grid, spacing, sizing
- Use for **responsive design**: breakpoint utilities (sm:, md:, lg:, xl:)
- Use for **spacing**: margin, padding utilities
- Use for **positioning**: absolute, relative, fixed utilities
- **DO NOT** override PrimeVue component styles with Tailwind
- **DO NOT** recreate components with Tailwind when PrimeVue has them

#### Tailwind Configuration

```typescript
// tailwind.config.ts
export default {
  content: [
    "./components/**/*.{vue,ts}",
    "./layouts/**/*.vue",
    "./pages/**/*.vue",
    "./app.vue",
  ],
  theme: {
    extend: {
      // Minimal custom extensions
      // Let PrimeVue handle colors
    },
  },
  plugins: [],
};
```

#### Layout Patterns with Tailwind

- Container layouts: `class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8"`
- Grid layouts: `class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4"`
- Flex layouts: `class="flex items-center justify-between gap-4"`
- Responsive spacing: `class="mt-4 md:mt-6 lg:mt-8"`

### 4. Styling Standards and Best Practices

#### ❌ AVOID Custom CSS in Components

**DO NOT** use `<style>` or `<style scoped>` blocks in Vue components unless absolutely necessary for edge cases. This violates our rapid development philosophy and makes maintenance harder.

**Why avoid custom CSS:**
- ❌ Creates inconsistency across the application
- ❌ Harder to maintain and refactor
- ❌ Breaks the design system
- ❌ Duplicates functionality already provided by Tailwind/PrimeVue
- ❌ Increases bundle size
- ❌ Creates specificity conflicts
- ❌ Not easily reusable

#### ✅ PREFERRED Approach

**Use Tailwind utilities and PrimeVue components:**

```vue
<!-- ❌ BAD: Custom CSS -->
<template>
  <div class="custom-card">
    <h1 class="custom-title">Title</h1>
  </div>
</template>

<style scoped>
.custom-card {
  padding: 24px;
  border-radius: 8px;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
}

.custom-title {
  font-size: 24px;
  font-weight: 600;
  color: #1f2937;
}
</style>

<!-- ✅ GOOD: Tailwind utilities + PrimeVue components -->
<template>
  <Card>
    <template #content>
      <h1 class="text-2xl font-semibold text-gray-900 dark:text-white">
        Title
      </h1>
    </template>
  </Card>
</template>
```

#### When Custom CSS is Acceptable

Custom CSS should ONLY be used in these rare cases:

1. **Global app configuration** (in `assets/styles/main.css`)
   - Tailwind imports
   - CSS custom properties for theming
   - Global reset or normalize

2. **Complex animations** not achievable with Tailwind
   - Only if `animate-*` utilities are insufficient
   - Must be reusable across components

3. **Third-party library integration** requiring CSS overrides
   - Only when library doesn't work with Tailwind
   - Document the reason with comments

4. **PrimeVue deep customization** (very rare)
   - Use `:deep()` selector sparingly
   - Prefer PrimeVue props and slots when available

#### Best Practices for Styling

**1. Use PrimeVue Built-in Components**

```vue
<!-- ✅ Use IconField for input icons -->
<IconField icon-position="left">
  <InputIcon class="pi pi-search" />
  <InputText v-model="search" placeholder="Search..." />
</IconField>

<!-- ❌ Don't manually position icons with absolute positioning -->
<div class="relative">
  <i class="absolute left-3 top-3 pi pi-search" />
  <InputText class="pl-10" />
</div>
```

**2. Use Tailwind Utilities for Layout**

```vue
<!-- ✅ Tailwind for spacing and layout -->
<div class="flex items-center justify-between gap-4 p-6">
  <h2 class="text-xl font-semibold">Title</h2>
  <Button label="Action" />
</div>

<!-- ❌ Don't create custom CSS for layouts -->
<div class="header-container">
  <h2>Title</h2>
  <Button label="Action" />
</div>
<style>
.header-container {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 1rem;
  padding: 1.5rem;
}
</style>
```

**3. Use PrimeVue Severity Props**

```vue
<!-- ✅ Use severity props for colors -->
<Button label="Submit" severity="info" />
<Message severity="success">Success!</Message>
<Tag value="Active" severity="success" />

<!-- ❌ Don't create custom CSS for component colors -->
<Button label="Submit" class="custom-blue" />
<style>
.custom-blue {
  background: #0ea5e9 !important;
}
</style>
```

**4. Use Tailwind Dark Mode Classes**

```vue
<!-- ✅ Tailwind dark mode -->
<div class="bg-white dark:bg-gray-800 text-gray-900 dark:text-white">
  Content
</div>

<!-- ❌ Don't create custom CSS for dark mode -->
<div class="themed-container">Content</div>
<style>
.themed-container {
  background: white;
  color: #1f2937;
}
.dark .themed-container {
  background: #1f2937;
  color: white;
}
</style>
```

**5. Use PrimeVue Component Slots**

```vue
<!-- ✅ Use component slots for customization -->
<Card>
  <template #header>
    <div class="flex items-center justify-between p-4">
      <h3 class="text-lg font-semibold">Card Title</h3>
      <Button icon="pi pi-cog" text rounded />
    </div>
  </template>
  <template #content>
    Card content
  </template>
</Card>

<!-- ❌ Don't recreate card with custom CSS -->
<div class="custom-card">
  <div class="custom-header">
    <h3>Card Title</h3>
  </div>
  <div class="custom-content">Card content</div>
</div>
```

#### Component Styling Checklist

Before creating any component, verify:

- [ ] Can I use an existing PrimeVue component?
- [ ] Can I achieve the layout with Tailwind utilities?
- [ ] Can I use PrimeVue props (severity, size, variant) for styling?
- [ ] Can I use component slots for customization?
- [ ] Have I avoided creating custom CSS classes?
- [ ] Am I using Tailwind dark mode classes for theme support?
- [ ] Have I checked the UX Agent documentation for design patterns?

**Remember**: If you find yourself writing custom CSS, step back and find the Tailwind/PrimeVue solution. Our goal is rapid development with consistency, not custom styling.

### 5. TypeScript Configuration

#### Type Safety Standards

- Enable strict mode in tsconfig
- Define interfaces for all API responses
- Create type definitions for domain models
- Use typed composables
- Define component props with TypeScript
- Avoid `any` type - use `unknown` when needed

#### Type Organization

Types are organized by domain in separate files:

**types/api.ts** - Generic API types and responses
- ApiResponse<T>
- PaginatedResponse<T>
- ApiError

**types/auth.ts** - Authentication types
- User
- LoginCredentials
- AuthToken

**types/billing.ts** - Billing domain types
- Invoice
- Customer
- Payment
- Subscription

**types/inventory.ts** - Inventory domain types
- Product
- Warehouse
- StockMovement

**types/tenant.ts** - Multi-tenant types
- Tenant
- TenantContext
- TenantSettings

#### Type Definitions

```typescript
// types/billing.ts
export interface Invoice {
  id: string
  invoiceNumber: string
  customerId: string
  totalAmount: number
  status: InvoiceStatus
  createdAt: string
}

export type InvoiceStatus = 'draft' | 'sent' | 'paid' | 'overdue'
```

#### Component Props Typing

```typescript
<script setup lang="ts">
interface Props {
  invoice: Invoice
  readonly?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  readonly: false
})
</script>
```

### 6. State Management with Pinia

#### Store Organization

- Create feature-based stores (auth, tenant, invoices, products)
- Use `defineStore` with setup syntax
- Implement computed getters
- Use actions for async operations
- Persist auth and tenant state to localStorage

#### Available Stores

The project includes the following Pinia stores:

**auth.ts** - Authentication state management
- Manages user authentication tokens
- Stores current user information
- Handles login/logout operations
- Persisted to localStorage

**tenant.ts** - Multi-tenant context management
- Manages current tenant selection
- Stores available tenants
- Handles tenant switching
- Persisted to localStorage

**ui.ts** - UI state management
- Manages breadcrumb navigation
- Stores UI preferences
- Handles global UI state
- Sidebar/menu state

#### Store Examples

```typescript
// stores/auth.ts
export const useAuthStore = defineStore(
  'auth',
  () => {
    const token = ref<string | null>(null)
    const user = ref<User | null>(null)

    const isAuthenticated = computed(() => !!token.value)

    async function login(credentials: LoginCredentials) {
      // API call
    }

    function logout() {
      token.value = null
      user.value = null
    }

    return { token, user, isAuthenticated, login, logout }
  },
  {
    persist: true,
  },
)
```

### 6. API Integration & Composables

#### useApi Composable

```typescript
// composables/useApi.ts
export const useApi = () => {
  const authStore = useAuthStore();
  const tenantStore = useTenantStore();

  const apiFetch = $fetch.create({
    baseURL: "/api/v1",
    headers: {
      Authorization: `Bearer ${authStore.token}`,
      "X-Tenant-Id": tenantStore.currentTenantId,
    },
    onResponseError({ response }) {
      if (response.status === 401) {
        authStore.logout();
        navigateTo("/login");
      }
    },
  });

  return { apiFetch };
};
```

#### API Service Patterns

- Create typed API service functions
- Use composables for API calls
- Handle errors consistently
- Show loading states with PrimeVue ProgressBar/Skeleton
- Display errors with PrimeVue Toast

### 6.1. Available Composables

The project includes the following composables that should be reused across the application:

#### useApi

Provides a configured API client with authentication and tenant context.

```typescript
// composables/useApi.ts
const { apiFetch } = useApi()
const data = await apiFetch('/endpoint')
```

#### useNotification

Provides toast notification helpers for consistent user feedback.

```typescript
// composables/useNotification.ts
const toast = useNotification()

toast.showSuccess(summary, detail)
toast.showError(summary, detail)
toast.showWarning(summary, detail)
toast.showInfo(summary, detail)
```

#### useTheme

Manages dark/light theme state and provides theme utilities.

```typescript
// composables/useTheme.ts
const { isDark, toggleTheme, setTheme } = useTheme()
```

#### useFormatters

Provides formatting utilities for dates, currency, and numbers.

```typescript
// composables/useFormatters.ts
const { formatCurrency, formatDate, formatNumber } = useFormatters()

const formatted = formatCurrency(1234.56) // "$1,234.56"
const date = formatDate(new Date()) // locale-aware formatting
```

#### useWarehouse (Entity Composable Pattern Example)

**All entity-specific composables should follow this pattern**. This is the standard approach for CRUD operations in the application.

```typescript
// composables/useWarehouse.ts
import type { Warehouse } from '~/types/inventory'

export function useWarehouse() {
  const config = useRuntimeConfig()
  const toast = useNotification()
  const { t } = useI18n()

  // Get all warehouses for current tenant
  async function getAllWarehouses() {
    try {
      const response = await $fetch<{ data: Warehouse[], success: boolean }>(
        '/warehouses',
        {
          baseURL: config.public.apiBase,
          credentials: 'include',
        },
      )
      return response.data
    }
    catch (error) {
      toast.showError(
        t('messages.error_load'),
        error instanceof Error ? error.message : 'Failed to load warehouses',
      )
      throw error
    }
  }

  // Get warehouse by ID
  async function getWarehouseById(id: string) {
    try {
      const response = await $fetch<{ data: Warehouse, success: boolean }>(
        `/warehouses/${id}`,
        {
          baseURL: config.public.apiBase,
          credentials: 'include',
        },
      )
      return response.data
    }
    catch (error) {
      toast.showError(
        t('messages.error_load'),
        error instanceof Error ? error.message : 'Failed to load warehouse',
      )
      throw error
    }
  }

  // Create new warehouse
  async function createWarehouse(warehouse: Partial<Warehouse>) {
    try {
      const response = await $fetch<{ data: Warehouse, success: boolean }>(
        '/warehouses',
        {
          method: 'POST',
          baseURL: config.public.apiBase,
          credentials: 'include',
          body: warehouse,
        },
      )
      toast.showSuccess(
        t('warehouses.title'),
        t('warehouses.created_successfully'),
      )
      return response.data
    }
    catch (error) {
      toast.showError(
        t('messages.error_save'),
        error instanceof Error ? error.message : 'Failed to create warehouse',
      )
      throw error
    }
  }

  // Update existing warehouse
  async function updateWarehouse(id: string, warehouse: Partial<Warehouse>) {
    try {
      const response = await $fetch<{ data: Warehouse, success: boolean }>(
        `/warehouses/${id}`,
        {
          method: 'PUT',
          baseURL: config.public.apiBase,
          credentials: 'include',
          body: { ...warehouse, id },
        },
      )
      toast.showSuccess(
        t('warehouses.title'),
        t('warehouses.updated_successfully'),
      )
      return response.data
    }
    catch (error) {
      toast.showError(
        t('messages.error_save'),
        error instanceof Error ? error.message : 'Failed to update warehouse',
      )
      throw error
    }
  }

  // Delete warehouse (soft delete)
  async function deleteWarehouse(id: string) {
    try {
      await $fetch(`/warehouses/${id}`, {
        method: 'DELETE',
        baseURL: config.public.apiBase,
        credentials: 'include',
      })
      toast.showSuccess(
        t('warehouses.title'),
        t('warehouses.deleted_successfully'),
      )
    }
    catch (error) {
      toast.showError(
        t('messages.error_delete'),
        error instanceof Error ? error.message : 'Failed to delete warehouse',
      )
      throw error
    }
  }

  return {
    getAllWarehouses,
    getWarehouseById,
    createWarehouse,
    updateWarehouse,
    deleteWarehouse,
  }
}
```

**Key Patterns for Entity Composables**:
- Always use runtime config for `baseURL` (e.g., `config.public.apiBase`)
- **CRITICAL**: Never include `/api/v1` in endpoint paths - it's already in `baseURL`
- Include `credentials: 'include'` for cookie-based auth
- Wrap all calls in try/catch with toast notifications
- Use i18n for all user-facing messages
- Return typed data from API responses (`response.data`)
- Export individual methods (not one giant object)
- Follow naming convention: `getAll{Entity}`, `get{Entity}ById`, `create{Entity}`, `update{Entity}`, `delete{Entity}`

### 6.2. Available Shared Components

The project includes reusable shared components that should be used instead of recreating similar functionality:

#### ThemeSwitcher

Dropdown component for selecting theme (system/light/dark).

```vue
<ThemeSwitcher />
```

- Uses PrimeVue Select component
- Icon-only on mobile, icon + text on larger screens
- Automatically syncs with color mode

#### LanguageSwitcher

Dropdown component for language selection.

```vue
<LanguageSwitcher />
```

- Uses PrimeVue Select component
- Icon-only on mobile, icon + text on larger screens
- Persists language preference

#### PageHeader

Consistent page header with title, description, and action slot.

```vue
<PageHeader
  :title="$t('pages.invoices.title')"
  :description="$t('pages.invoices.description')"
>
  <template #actions>
    <Button :label="$t('actions.create')" icon="pi pi-plus" />
  </template>
</PageHeader>
```

#### StatCard

Reusable card for displaying statistics and metrics.

```vue
<StatCard
  :title="$t('dashboard.revenue')"
  :value="formatCurrency(revenue)"
  icon="pi pi-dollar"
  trend="+12%"
  trend-up
/>
```

#### LoadingState

Skeleton/spinner component for loading states.

```vue
<LoadingState :loading="isLoading">
  <template #default>
    <!-- Your content here -->
  </template>
</LoadingState>
```

#### EmptyState

Component for displaying empty states with icon, message, and action.

```vue
<EmptyState
  icon="pi pi-inbox"
  :title="$t('common.no_data')"
  :description="$t('common.no_data_description')"
>
  <template #action>
    <Button :label="$t('actions.create')" />
  </template>
</EmptyState>
```

#### DataTableActions

Action buttons component for DataTable rows (view, edit, delete).

```vue
<DataTableActions
  @view="handleView"
  @edit="handleEdit"
  @delete="handleDelete"
  :show-view="true"
  :show-edit="true"
  :show-delete="true"
/>
```

**Remember**: Always check for existing shared components before creating new ones. Reusing existing components ensures consistency and reduces maintenance.

### 6.3. Available Utilities

The project includes utility functions organized in the `utils/` directory:

#### constants.ts

Application-wide constants and configuration values.

```typescript
// utils/constants.ts
export const APP_NAME = 'Billing & Inventory'
export const API_TIMEOUT = 30000
export const DEFAULT_PAGE_SIZE = 10
export const MAX_FILE_SIZE = 5 * 1024 * 1024 // 5MB
```

#### helpers.ts

General utility functions for common operations.

```typescript
// utils/helpers.ts
export function debounce(fn: Function, delay: number) { ... }
export function truncate(text: string, length: number) { ... }
export function downloadFile(blob: Blob, filename: string) { ... }
```

#### status.ts

Status-related utilities for billing, inventory, and orders.

```typescript
// utils/status.ts
export function getInvoiceStatusSeverity(status: string): 'success' | 'warning' | 'danger' | 'info' { ... }
export function getStockStatusColor(quantity: number, minQuantity: number): string { ... }
```

#### validators.ts

Custom validation functions and rules.

```typescript
// utils/validators.ts
export const isValidEmail = (email: string): boolean => { ... }
export const isValidPhone = (phone: string): boolean => { ... }
export const isPositiveNumber = (value: number): boolean => { ... }
```

**Usage Example**:
```typescript
import { getInvoiceStatusSeverity } from '~/utils/status'
import { debounce } from '~/utils/helpers'

const severity = getInvoiceStatusSeverity(invoice.status)
const debouncedSearch = debounce(handleSearch, 300)
```

### 7. Authentication & Authorization

#### Authentication Flow

- Login page with PrimeVue Card and InputText components
- Store JWT token in Pinia + localStorage
- Auto-attach token to all API requests
- Implement token refresh mechanism
- Redirect to login on 401 responses

#### Auth Middleware

```typescript
// middleware/auth.ts
export default defineNuxtRouteMiddleware((to, from) => {
  const authStore = useAuthStore();

  if (!authStore.isAuthenticated && to.path !== "/login") {
    return navigateTo("/login");
  }
});
```

#### Protected Routes

- Apply auth middleware to protected pages
- Check user permissions for specific actions
- Hide/disable UI elements based on permissions
- Use v-if with permission checks

### 8. Multi-Tenant Context

#### Tenant Selection

- Implement tenant selector in layout (PrimeVue Dropdown)
- Store current tenant in Pinia
- Include tenant ID in all API requests (X-Tenant-Id header)
- Clear tenant-specific data on tenant switch
- Prevent cross-tenant data access in UI

#### Tenant Middleware

```typescript
// middleware/tenant.ts
export default defineNuxtRouteMiddleware((to, from) => {
  const tenantStore = useTenantStore();

  if (!tenantStore.currentTenantId) {
    return navigateTo("/select-tenant");
  }
});
```

### 9. Form Handling & Validation

#### PrimeVue Form Components

- Use PrimeVue form components exclusively
- Leverage built-in validation states (invalid, error)
- Use FloatLabel for clean form layouts
- Implement FormGroup for consistent spacing
- Use Fieldset for logical grouping

#### Validation Strategy

```typescript
// Use Vuelidate or Zod for validation
import { useVuelidate } from "@vuelidate/core";
import { required, email, minLength } from "@vuelidate/validators";

const rules = {
  email: { required, email },
  password: { required, minLength: minLength(8) },
};

const v$ = useVuelidate(rules, formData);
```

#### Form Submission

- Validate before submission
- Show loading state on submit button
- Display success message with Toast
- Handle errors with InlineMessage or Toast
- Reset form after successful submission

### 10. Data Tables & Lists

#### DataTable Configuration

- Use PrimeVue DataTable for all tabular data
- Enable built-in features (no custom implementations):
  - Pagination with Paginator
  - Sorting (sortable columns)
  - Filtering (filterDisplay, filters)
  - Column resizing (resizableColumns)
  - Row selection (selection, selectionMode)
  - Export (CSV export feature)
- Use lazy loading for large datasets
- Implement server-side pagination/sorting/filtering

#### DataTable Example

```typescript
<DataTable
  :value="invoices"
  :paginator="true"
  :rows="10"
  :loading="loading"
  filterDisplay="row"
  stripedRows
  responsiveLayout="scroll"
>
  <Column field="invoiceNumber" header="Invoice #" sortable />
  <Column field="totalAmount" header="Amount" sortable>
    <template #body="{ data }">
      {{ formatCurrency(data.totalAmount) }}
    </template>
  </Column>
  <Column header="Actions">
    <template #body="{ data }">
      <Button icon="pi pi-eye" @click="viewInvoice(data.id)" />
    </template>
  </Column>
</DataTable>
```

### 11. Layout & Navigation

#### Available Layouts

The project includes two main layouts:

**default.vue** - Authenticated application layout
- Menubar with navigation
- Tenant selector dropdown
- Language and theme switchers
- User menu with profile/settings/logout
- Breadcrumb navigation
- Responsive with mobile sidebar

**auth.vue** - Authentication layout
- Minimal layout for login/register pages
- Centered content container
- Theme and language switchers in top-right
- Animated background (optional)
- Toast for notifications

#### Layout Structure

- Use PrimeVue Menubar for top navigation
- Use PrimeVue Sidebar for mobile menu
- Use PrimeVue Breadcrumb for navigation context
- Implement responsive layouts with Tailwind grid/flex
- Use PrimeVue Card for content containers
- Include theme toggle and language switcher in navigation

#### Default Layout Example with Theme & Language Support

```vue
<script setup lang="ts">
const { t } = useI18n()
const { isDark } = useTheme()

const menuItems = computed(() => [
  { label: t('nav.dashboard'), icon: 'pi pi-home', to: '/' },
  { label: t('nav.billing'), icon: 'pi pi-dollar', to: '/billing' },
  { label: t('nav.inventory'), icon: 'pi pi-box', to: '/inventory' },
])
</script>

<template>
  <div class="min-h-screen" :class="isDark ? 'dark bg-gray-900' : 'bg-gray-50'">
    <Menubar :model="menuItems">
      <template #end>
        <div class="flex items-center gap-2">
          <!-- Language Switcher -->
          <LanguageSwitcher />

          <!-- Theme Switcher -->
          <ThemeSwitcher />

          <!-- User Menu -->
          <Button icon="pi pi-user" text rounded @click="toggleUserMenu" />
        </div>
      </template>
    </Menubar>

    <div class="max-w-7xl mx-auto px-4 py-6">
      <Breadcrumb :home="home" :model="breadcrumbItems" class="mb-4" />
      <slot />
    </div>

    <Toast position="top-right" />
  </div>
</template>
```

#### Auth Layout Example

```vue
<template>
  <div class="min-h-screen flex items-center justify-center relative">
    <!-- Theme & Language Switchers - Fixed Top Right -->
    <div class="fixed top-4 right-4 z-[99999] grid grid-cols-2 md:flex md:flex-row items-center gap-2">
      <LanguageSwitcher />
      <ThemeSwitcher />
    </div>

    <!-- Content Container -->
    <div class="w-full max-w-md p-6 relative z-10">
      <slot />
    </div>

    <!-- Toast for notifications -->
    <Toast position="top-right" :pt="{ root: { style: 'top: 5rem' } }" />
  </div>
</template>
```

          <!-- User Menu -->
          <Button icon="pi pi-user" text rounded @click="toggleUserMenu" />
        </div>
      </template>
    </Menubar>

    <div class="max-w-7xl mx-auto px-4 py-6">
      <Breadcrumb :home="home" :model="breadcrumbItems" class="mb-4" />
      <slot />
    </div>
  </div>
</template>
```

### 13. User Feedback & Notifications

#### Toast Notifications

- Use PrimeVue Toast for all notifications
- Define useToast composable for consistent usage
- Severity levels: success, info, warn, error
- Auto-dismiss timers (3-5 seconds)
- Position: top-right (default)

#### Confirm Dialogs

- Use ConfirmDialog for destructive actions
- Use ConfirmPopup for quick confirmations
- Provide clear action labels (Delete, Cancel, etc.)

#### Loading States

- Use ProgressBar for page-level loading
- Use Skeleton for content placeholders
- Use loading prop on buttons during async operations
- Use overlay loading with BlockUI for forms

### 13. Responsive Design

#### Breakpoint Strategy

- Mobile-first approach with Tailwind
- Use PrimeVue responsive components
- Test on mobile, tablet, desktop viewports
- Use responsive utilities: `hidden md:block`, `flex-col md:flex-row`
- Use PrimeVue's responsiveLayout on DataTable

#### Mobile Optimizations

- Use Sidebar for mobile navigation
- Stack forms vertically on mobile
- Use PrimeVue's responsive grid system
- Implement touch-friendly button sizes
- Use responsive DataTable layouts

### 14. Performance Optimization

#### Component Optimization

- Use lazy loading for routes: `component: () => import()`
- Implement virtual scrolling for long lists (PrimeVue VirtualScroller)
- Use v-show vs v-if appropriately
- Debounce search inputs
- Optimize images with Nuxt Image

#### Data Fetching

- Use `useFetch` and `useAsyncData` with proper keys
- Implement client-side caching
- Use lazy loading for non-critical data
- Implement infinite scroll with PrimeVue VirtualScroller
- Paginate large datasets

### 15. Internationalization (i18n)

#### Multi-language Support Strategy

- Support multiple languages for global SaaS reach
- Use @nuxtjs/i18n module for Nuxt 3
- Configure PrimeVue locale for component translations
- Implement tenant-specific language preferences
- Persist user language selection

#### Supported Languages

The project currently supports:
- **English** (en) - Default
- **Español** (es) - Spanish
- **Français** (fr) - French
- **Deutsch** (de) - German

Translation files are located in `i18n/locales/` directory.

#### i18n Module Setup

```typescript
// nuxt.config.ts
export default defineNuxtConfig({
  modules: ['@nuxtjs/i18n'],
  i18n: {
    locales: [
      { code: 'en', iso: 'en-US', file: 'en.json', name: 'English' },
      { code: 'es', iso: 'es-ES', file: 'es.json', name: 'Español' },
      { code: 'fr', iso: 'fr-FR', file: 'fr.json', name: 'Français' },
      { code: 'de', iso: 'de-DE', file: 'de.json', name: 'Deutsch' },
    ],
    defaultLocale: 'en',
    lazy: true,
    langDir: 'i18n/locales/',
    strategy: 'no_prefix', // or 'prefix_and_default'
    detectBrowserLanguage: {
      useCookie: true,
      cookieKey: 'i18n_redirected',
      redirectOn: 'root',
    },
  },
})
```

#### Translation File Structure

```json
// i18n/locales/en.json
{
  "common": {
    "save": "Save",
    "cancel": "Cancel",
    "delete": "Delete",
    "edit": "Edit",
    "search": "Search",
    "loading": "Loading...",
    "theme_system": "System",
    "theme_light": "Light",
    "theme_dark": "Dark"
  },
  "nav": {
    "dashboard": "Dashboard",
    "billing": "Billing",
    "inventory": "Inventory",
    "invoices": "Invoices",
    "products": "Products"
  },
  "billing": {
    "invoices": "Invoices",
    "invoice_number": "Invoice Number",
    "total_amount": "Total Amount",
    "status": "Status",
    "create_invoice": "Create Invoice"
  },
  "inventory": {
    "products": "Products",
    "stock": "Stock",
    "sku": "SKU",
    "warehouse": "Warehouse"
  },
  "auth": {
    "login": "Login",
    "logout": "Logout",
    "email": "Email",
    "password": "Password",
    "welcome_back": "Welcome Back",
    "demo_credentials": "Demo Credentials"
  },
  "messages": {
    "success_save": "Saved successfully",
    "error_load": "Failed to load data",
    "confirm_delete": "Are you sure you want to delete this item?"
  },
  "validation": {
    "email_required": "Email is required",
    "email_invalid": "Email is invalid",
    "password_required": "Password is required",
    "password_min_length": "Password must be at least 6 characters"
  }
}
```

#### PrimeVue Locale Integration

```typescript
// plugins/primevue.ts
import PrimeVue from "primevue/config";
import en from "primevue/resources/locale/en.json";
import es from "primevue/resources/locale/es.json";
import fr from "primevue/resources/locale/fr.json";
import de from "primevue/resources/locale/de.json";

export default defineNuxtPlugin((nuxtApp) => {
  const i18n = nuxtApp.$i18n;

  const locales = { en, es, fr, de };

  nuxtApp.vueApp.use(PrimeVue, {
    locale: locales[i18n.locale.value] || locales.en,
  });

  // Watch for locale changes
  watch(
    () => i18n.locale.value,
    (newLocale) => {
      nuxtApp.vueApp.config.globalProperties.$primevue.config.locale =
        locales[newLocale] || locales.en;
    },
  );
});
```

#### Language Switcher Component

```vue
<script setup lang="ts">
const { locale, locales, setLocale } = useI18n();

const availableLocales = computed(() =>
  locales.value.map((l) => ({
    label: l.name,
    value: l.code,
  })),
);

const currentLocale = computed({
  get: () => locale.value,
  set: (value) => setLocale(value),
});
</script>

<template>
  <Dropdown
    v-model="currentLocale"
    :options="availableLocales"
    optionLabel="label"
    optionValue="value"
    placeholder="Select Language"
  >
    <template #value="{ value }">
      <i class="pi pi-globe mr-2" />
      {{ availableLocales.find((l) => l.value === value)?.label }}
    </template>
  </Dropdown>
</template>
```

#### Using Translations in Components

```vue
<script setup lang="ts">
const { t } = useI18n();

// In template: {{ t('billing.invoices') }}
// In script: const title = t('billing.create_invoice')
</script>

<template>
  <div>
    <h1>{{ t("billing.invoices") }}</h1>
    <Button :label="t('common.save')" @click="save" />
    <DataTable :value="invoices">
      <Column field="invoiceNumber" :header="t('billing.invoice_number')" />
      <Column field="totalAmount" :header="t('billing.total_amount')" />
    </DataTable>
  </div>
</template>
```

#### Date and Number Formatting

```typescript
// composables/useFormatters.ts
export const useFormatters = () => {
  const { locale } = useI18n();

  const formatCurrency = (amount: number, currency = "USD") => {
    return new Intl.NumberFormat(locale.value, {
      style: "currency",
      currency,
    }).format(amount);
  };

  const formatDate = (
    date: string | Date,
    format: "short" | "long" = "short",
  ) => {
    const options: Intl.DateTimeFormatOptions =
      format === "short"
        ? { year: "numeric", month: "2-digit", day: "2-digit" }
        : { year: "numeric", month: "long", day: "numeric" };

    return new Intl.DateTimeFormat(locale.value, options).format(
      new Date(date),
    );
  };

  const formatNumber = (num: number) => {
    return new Intl.NumberFormat(locale.value).format(num);
  };

  return { formatCurrency, formatDate, formatNumber };
};
```

#### Best Practices for i18n

- Keep translation keys organized by feature/module
- Use nested objects for better organization
- Avoid hardcoded text in templates
- Use pluralization features: `$t('items', { count: n })`
- Test all languages during development
- Provide fallback translations
- Use parameters in translations: `$t('welcome', { name: userName })`
- Keep translations in sync across all language files
- Consider RTL languages if needed in future

### 16. Charts & Analytics

#### PrimeVue Chart Component

- Use for dashboards and reports
- Chart types: Line, Bar, Pie, Doughnut
- Use Chart.js configuration
- Implement responsive charts
- Use PrimeVue color palette

## Deliverables

When invoked, this agent will produce:

1. **Nuxt 3 Configuration**
   - nuxt.config.ts with PrimeVue, Tailwind, TypeScript
   - Plugin configurations (PrimeVue, i18n, color mode)
   - Module setups (@nuxtjs/i18n, @nuxtjs/color-mode)
   - Dark/Light theme setup
   - i18n configuration

2. **Component Templates**
   - Reusable PrimeVue-based components
   - Layout components
   - Form components
   - Data display components

3. **Composables**
   - useApi, useAuth, useTenant, useToast
   - Custom typed composables

4. **Store Definitions**
   - Pinia stores for auth, tenant, UI state
   - Typed actions and getters

5. **Page Templates**
   - Login, dashboard, list, detail, form pages
   - Using PrimeVue components
   - Theme-aware layouts
   - Internationalized content

6. **TypeScript Definitions**
   - API response types
   - Domain model interfaces
   - Component prop types

7. **Styling Guide**
   - Tailwind utility usage
   - PrimeVue theme usage (Light/Dark Teal)
   - Dark mode implementation
   - Responsive patterns

8. **i18n Implementation**
   - Translation files structure
   - Language switcher component
   - Date/number formatting utilities
   - PrimeVue locale integration

## Code Standards

### Naming Conventions

- Components: PascalCase (InvoiceList.vue)
- Composables: camelCase with 'use' prefix (useInvoiceApi.ts)
- Stores: camelCase with Store suffix (invoiceStore.ts)
- Types/Interfaces: PascalCase (Invoice, InvoiceStatus)
- Constants: UPPER_SNAKE_CASE

### Component Structure

```vue
<script setup lang="ts">
// Imports
// Props/Emits definitions
// Composables
// Reactive state
// Computed properties
// Methods
// Lifecycle hooks
</script>

<template>
  <!-- Template using PrimeVue components -->
</template>

<style scoped>
/* Minimal scoped styles - prefer Tailwind utilities */
</style>
```

### File Organization

- One component per file
- Group related components in feature folders
- Keep composables focused and single-purpose
- Separate API types from component types

## Best Practices

### DO:

✅ Use PrimeVue default components without customization
✅ Apply Tailwind for layout and spacing utilities
✅ Type everything with TypeScript
✅ Use composables for reusable logic
✅ Implement proper error handling with Toast
✅ Use Pinia for state management
✅ Follow Nuxt 3 auto-import conventions
✅ Implement responsive design with mobile-first approach
✅ Use PrimeVue built-in features (sorting, filtering, pagination)
✅ Keep components small and focused
✅ Support both light and dark themes
✅ Provide internationalization for all user-facing text
✅ Test in both theme modes
✅ Format dates/numbers according to user locale

### DON'T:

❌ Create custom components when PrimeVue has them
❌ Override PrimeVue styles extensively
❌ Build custom theme - use Lara Light/Dark Teal defaults
❌ Use Options API - use Composition API only
❌ Ignore TypeScript errors
❌ Mix state management approaches
❌ Create complex custom CSS - use Tailwind utilities
❌ Reinvent features that PrimeVue provides
❌ Skip responsive testing
❌ Use inline styles
❌ Hardcode text strings - use i18n translations
❌ Forget to test dark mode appearance
❌ Use fixed colors that don't adapt to theme

## Quick Start Templates

### Page Template (Invoice List)

```vue
<script setup lang="ts">
import type { Invoice } from "~/types/models";

definePageMeta({
  middleware: ["auth", "tenant"],
});

const { apiFetch } = useApi();
const toast = useToast();

const invoices = ref<Invoice[]>([]);
const loading = ref(false);

const loadInvoices = async () => {
  loading.value = true;
  try {
    invoices.value = await apiFetch("/invoices");
  } catch (error) {
    toast.add({
      severity: "error",
      summary: "Error",
      detail: "Failed to load invoices",
    });
  } finally {
    loading.value = false;
  }
};

onMounted(() => loadInvoices());
</script>

<template>
  <div class="container mx-auto px-4 py-6">
    <div class="flex justify-between items-center mb-6">
      <h1 class="text-3xl font-bold">Invoices</h1>
      <Button
        label="New Invoice"
        icon="pi pi-plus"
        @click="navigateTo('/invoices/new')"
      />
    </div>

    <Card>
      <template #content>
        <DataTable
          :value="invoices"
          :loading="loading"
          :paginator="true"
          :rows="10"
          stripedRows
        >
          <Column field="invoiceNumber" header="Invoice #" sortable />
          <Column field="totalAmount" header="Amount" sortable />
          <Column field="status" header="Status">
            <template #body="{ data }">
              <Tag
                :value="data.status"
                :severity="getStatusSeverity(data.status)"
              />
            </template>
          </Column>
        </DataTable>
      </template>
    </Card>
  </div>
</template>
```

## Constraints and Boundaries

### In Scope

- Nuxt 3 + TypeScript frontend implementation
- PrimeVue component integration (default Teal theme)
- Tailwind CSS for layouts and utilities
- State management with Pinia
- API integration and authentication
- Form handling and validation
- Responsive design
- Multi-tenant UI context
- Data tables and visualizations
- User notifications and feedback

### Out of Scope

- Backend development (delegate to Backend Agent)
- Custom theme creation (use default Teal)
- Complex CSS animations (use PrimeVue defaults)
- Backend API design (delegate to Backend/Architecture Agent)
- Infrastructure and deployment (delegate to DevOps Agent)
- Heavy UI customization (prioritize speed with defaults)

## Integration with Other Agents

- **Project Architecture Agent**: Receives frontend architecture guidance
- **Backend Agent**: Consumes API contracts and endpoints
- **Testing Agent**: Provides testable component structure
- **DevOps Agent**: Receives build and deployment requirements

## Version

- Created: February 6, 2026
- Framework Version: Nuxt 3, PrimeVue 3+, Tailwind CSS 3+
- Last Updated: February 6, 2026
