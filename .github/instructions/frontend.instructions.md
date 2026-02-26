---
applyTo: "frontend/**"
---

# Frontend Development Instructions

You are an expert in Nuxt 3 + TypeScript frontend development for the SaaS Billing + Inventory Management System.
Consolidates: **Frontend Agent**, **UX Agent**.

---

## Tech Stack

- **Framework**: Nuxt 3 + TypeScript (strict mode — `any` is **forbidden**)
- **UI Library**: PrimeVue v4 with Aura preset, Teal primary color
- **CSS**: Tailwind CSS utilities only — ❌ No `<style>` / `<style scoped>` blocks (except `assets/styles/main.css`)
- **State**: Pinia (auth, tenant, ui stores — `auth` and `tenant` are persisted)
- **HTTP**: `useApi()` composable → `apiFetch` — the **only** correct way to call the API
- **Validation**: Vuelidate + `@vuelidate/validators`
- **i18n**: `@nuxtjs/i18n` — English, Español, Français, Deutsch
- **Icons**: PrimeIcons

---

## CRITICAL Rules (Breaking These Causes Runtime Crashes)

| ❌ WRONG | ✅ CORRECT |
|---|---|
| `useAuth()` | Does not exist — use `useApi()` |
| `apiFetch('/api/v1/warehouses')` | `apiFetch('/warehouses')` — `/api/v1` is already in baseURL |
| `$fetch(...)` without headers | `const { apiFetch } = useApi()` |
| `can('create', 'products')` | `can.createProduct()` — `can` is an object with methods |
| `<Breadcrumb>` directly in pages | `uiStore.setBreadcrumbs([...])` in `onMounted` |
| `any` type | Use proper types from `~/types/` |
| Custom CSS for layout | Tailwind utilities |

---

## API Usage

### `useApi()` — Always Use This
```typescript
const { apiFetch } = useApi()

// Automatically adds: Authorization header, X-Tenant-Id header, correct baseURL
const response = await apiFetch<ApiResponse<Warehouse[]>>('/warehouses')
return response.data
```

### Composable Template (follow `useWarehouse` pattern)
```typescript
import type { YourType, YourFormData } from '~/types/your-module'
import type { ApiResponse } from '~/types/api'

export function useYourService() {
  const { apiFetch } = useApi()
  const toast = useNotification()
  const { t } = useI18n()

  const getAll = async (): Promise<YourType[]> => {
    try {
      const response = await apiFetch<ApiResponse<YourType[]>>('/your-resource')
      return response.data
    } catch (error) {
      toast.showError(t('messages.error_load'), error instanceof Error ? error.message : 'Failed')
      throw error
    }
  }
  return { getAll, getById, create, update, remove }
}
```

### API Response Structure
```typescript
interface ApiResponse<T> { data: T; message?: string; success: boolean }
// Always access: response.data
```

---

## Styling Rules

**No custom CSS. Use PrimeVue + Tailwind only.**

| Use Case | Tool |
|---|---|
| Layout / spacing / responsive | Tailwind utilities |
| UI components | PrimeVue built-in |
| Colors / status variants | PrimeVue `severity` prop |
| Dark mode | Tailwind `dark:` classes |
| Never | `<style scoped>`, inline styles |

```vue
<!-- ❌ BAD -->
<style scoped>.my-card { padding: 24px; }</style>

<!-- ✅ GOOD -->
<Card><template #content><div class="p-6">...</div></template></Card>
```

---

## Component Checklist (before creating anything custom)

1. Check `components/shared/` — already implemented:
   - `PageHeader.vue` — title, description, `#actions` slot
   - `EmptyState.vue` — icon, title, description, action button
   - `LoadingState.vue` — spinner / skeleton
   - `DataTableActions.vue` — view / edit / delete row buttons
   - `StatCard.vue` — metric with trend
   - `ThemeSwitcher.vue`, `LanguageSwitcher.vue`

2. Check PrimeVue components — prefer built-in over custom recreations

---

## PrimeVue Standards

**Button:**
```vue
<Button label="Save" icon="pi pi-check" />
<Button label="Cancel" severity="secondary" variant="outlined" />
<Button label="Delete" severity="danger" />
<Button label="Saving..." :loading="isSaving" />
```

**Severity values:** `primary` (default), `secondary`, `success`, `info`, `warn`, `help`, `danger`, `contrast`
**Variant values:** default (filled), `outlined`, `text`

**DataTable (standard pattern):**
```vue
<DataTable :value="items" :paginator="true" :rows="10" :loading="loading"
  filterDisplay="row" stripedRows responsiveLayout="scroll">
  <Column field="name" header="Name" sortable />
  <Column header="Actions">
    <template #body="{ data }">
      <DataTableActions @view="..." @edit="..." @delete="..." />
    </template>
  </Column>
</DataTable>
```

---

## Spacing System (Tailwind scale)

| Token | Class | px |
|---|---|---|
| Card padding | `p-6` | 24px |
| Field gap | `gap-4` | 16px |
| Label margin | `mb-2` | 8px |
| Button gap | `gap-2` | 8px |
| Page padding | `p-6` | 24px |
| Section gap | `gap-6` | 24px |

---

## Typography

- H1 (page titles): `text-3xl font-bold`
- H2 (section headers): `text-2xl font-semibold`
- H4 (card headers): `text-lg font-medium`
- Body: `text-base`
- Helper text: `text-sm text-gray-500 dark:text-gray-400`

---

## Page Layout Pattern

```vue
<template>
  <div class="min-h-screen bg-gray-50 dark:bg-gray-900">
    <div class="p-6">
      <div class="mb-6 flex justify-between items-center">
        <div>
          <h1 class="text-3xl font-bold text-gray-900 dark:text-gray-50">{{ t('page.title') }}</h1>
          <p class="mt-2 text-sm text-gray-500 dark:text-gray-400">{{ t('page.description') }}</p>
        </div>
        <Button :label="t('common.create')" icon="pi pi-plus" @click="router.push('/new')" />
      </div>
      <Card><!-- content --></Card>
    </div>
  </div>
</template>

<script setup lang="ts">
const { t } = useI18n()
const uiStore = useUIStore()

onMounted(() => {
  uiStore.setBreadcrumbs([
    { label: t('nav.section'), to: '/section' },
    { label: t('page.title') }, // No 'to' for current page
  ])
})
</script>
```

---

## Form Pattern

```vue
<form @submit.prevent="handleSubmit">
  <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
    <div class="flex flex-col gap-2">
      <label for="name" class="font-medium">{{ t('fields.name') }} <span class="text-red-500">*</span></label>
      <InputText id="name" v-model="form.name" :class="{ 'p-invalid': v$.name.$error }" />
      <small v-if="v$.name.$error" class="p-error">{{ v$.name.$errors[0].$message }}</small>
    </div>
  </div>
  <div class="flex justify-end gap-2 mt-6">
    <Button :label="t('common.cancel')" severity="secondary" variant="outlined" @click="router.back()" />
    <Button :label="t('common.save')" icon="pi pi-check" type="submit" :loading="saving" />
  </div>
</form>
```

---

## Permissions (`can` object — NOT a function)

```vue
<Button v-if="can.createWarehouse()" label="New" />
<Button v-if="can.editProduct()" label="Edit" />
<Button v-if="can.deleteProduct()" severity="danger" label="Delete" />
```

Available: `viewWarehouses`, `createWarehouse`, `editWarehouse`, `deleteWarehouse`, `viewProducts`, `createProduct`, `editProduct`, `deleteProduct`, `viewStock`, `createStock`, `viewCustomers`, `createCustomer`, `editCustomer`, `deleteCustomer`, `viewUsers`, `createUser`, `editUser`, `deleteUser`, `inviteUser`, `removeUser`, `viewRoles`, `manageRoles`, `viewTenants`, `createTenant`, `editTenant`, `deleteTenant`

---

## i18n Standards

- All user-facing text must go through `t('key')` — no hardcoded strings
- Add keys to all 4 locale files: `en.json`, `es.json`, `fr.json`, `de.json`
- Escape `@` with `{'@'}`, `{` with `{'{'}`
- Follow snake_case for keys, camelCase for namespaces
- Refer to `docs/i18n-standards.md` for naming conventions

---

## ESLint Rules (enforced by `@antfu/eslint-config`)

- Single quotes, no semicolons, trailing commas in multiline
- No `any` type
- Top-level declarations use `function` keyword
- `import.meta.client` instead of `process.client`
- Run after changes: `npm run lint:fix` → `npm run lint`

---

## Accessibility (WCAG 2.1 AA)

- Color contrast: text 4.5:1, interactive 3:1
- All interactive elements keyboard-accessible
- ARIA labels on icon-only buttons
- Required fields marked with `*` and `aria-required`
- Errors announced to screen readers

---

## After Changes

```powershell
docker-compose restart frontend
docker-compose ps frontend
```
Verify at http://localhost:3000 before marking work complete.
