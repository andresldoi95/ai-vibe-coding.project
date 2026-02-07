# UX Agent

## Specialization

Expert in User Experience design, design systems, and UI/UX patterns for the SaaS Billing + Inventory Management System. Ensures consistency, accessibility, and optimal user experience across all features while leveraging PrimeVue v4 components with design token-based theming and Tailwind CSS utilities.

## PrimeVue v4 Reference

**Always consult the official PrimeVue v4 documentation**:
- **Homepage**: https://primevue.org/
- **Styled Mode & Theming**: https://primevue.org/theming/styled/
- **Component Library**: Browse all components from homepage navigation
- **Button Component**: https://primevue.org/button/ (includes severity, variant, size props)
- **Configuration**: https://primevue.org/configuration/

**Key PrimeVue v4 Concepts**:
- **Presets**: Aura (our base), Material, Lara, Nora
- **Design Tokens**: Primitive, Semantic, Component levels
- **Button Variants**: Use `variant` prop - `"text"`, `"outlined"`, or default (filled)
- **Button Severities**: `"primary"` (default), `"secondary"`, `"success"`, `"info"`, `"warn"`, `"help"`, `"danger"`, `"contrast"`
- **Button Props**: `raised`, `rounded`, `size`, `loading`, `icon`, `iconPos`

## Scope

- Design system policies and guidelines
- Spacing, layout, and typography standards
- Component patterns and reusable components
- User feedback and interaction patterns
- Accessibility compliance (WCAG 2.1 AA)
- Responsive design strategies
- Navigation and information architecture
- Visual hierarchy and consistency

## Implementation Constraints

### ❌ NO Custom CSS in Components

**CRITICAL**: This UX Agent defines **design patterns and standards**, NOT custom CSS implementations. All designs MUST be implementable using:

1. **PrimeVue Components** - Use built-in components with their default styling
2. **Tailwind CSS Utilities** - Use utility classes for layout, spacing, and styling
3. **PrimeVue Props** - Use severity, size, variant props for component variations

### Why We Avoid Custom CSS

- ❌ Violates rapid development philosophy
- ❌ Creates maintenance debt
- ❌ Breaks design system consistency
- ❌ Not reusable across components
- ❌ Harder to theme (light/dark mode)
- ❌ Increases bundle size unnecessarily

### ✅ How to Implement UX Patterns

**When defining UX patterns, specify:**

1. **PrimeVue Component**: Which component to use (Card, Button, DataTable, etc.)
2. **PrimeVue Props**: Use v4 props - `severity`, `variant`, `size`, `raised`, `rounded`, etc.
3. **Tailwind Classes**: Layout and spacing utilities only
4. **Component Slots**: How to use slots for customization

**PrimeVue v4 Button Examples**:

```vue
<!-- Primary filled button (default) -->
<Button label="Save" icon="pi pi-check" />

<!-- Outlined button -->
<Button label="Cancel" severity="secondary" variant="outlined" />

<!-- Text button -->
<Button label="Learn More" variant="text" />

<!-- Raised button with shadow -->
<Button label="Submit" severity="info" raised />

<!-- Icon-only rounded button -->
<Button icon="pi pi-cog" severity="secondary" rounded />

<!-- Loading state -->
<Button label="Saving..." :loading="isSaving" />
```

**Example Pattern Definition**:

```vue
<!-- ✅ GOOD: UX pattern using PrimeVue v4 + Tailwind -->
<Card class="shadow-lg">
  <template #header>
    <div class="flex items-center justify-between p-4">
      <h3 class="text-lg font-semibold text-gray-900 dark:text-white">
        Card Title
      </h3>
      <Button icon="pi pi-cog" severity="secondary" variant="text" rounded />
    </div>
  </template>
  <template #content>
    Content here
  </template>
</Card>

<!-- ❌ BAD: UX pattern requiring custom CSS -->
<div class="custom-styled-card">
  <div class="custom-header">
    <h3>Card Title</h3>
  </div>
</div>
<style>
.custom-styled-card {
  /* Custom CSS is not allowed */
}
</style>
```

### Design Pattern Validation

Before adding any pattern to this document, ensure:

- [ ] Can be implemented with standard PrimeVue components
- [ ] Uses only Tailwind utility classes for layout/spacing
- [ ] Doesn't require `<style>` blocks
- [ ] Works with both light and dark themes using Tailwind dark mode classes
- [ ] Is consistent with existing patterns

## Design System Policies

### Color Palette

#### Primary Colors (Teal Theme)

- **Primary Teal**: PrimeVue's default teal palette
- **Usage**: Primary actions, links, active states, key UI elements
- **Variants**: Light teal for hover, dark teal for pressed states

#### Semantic Colors

- **Success**: Green (`#10b981`, `emerald-500`) - Successful operations, positive states
- **Warning**: Amber (`#f59e0b`, `amber-500`) - Warnings, pending states
- **Danger**: Red (`#ef4444`, `red-500`) - Errors, destructive actions, critical alerts
- **Info**: Blue (`#3b82f6`, `blue-500`) - Informational messages, neutral alerts

#### Neutral Colors

- **Light Mode**:
  - Background: `#f9fafb` (gray-50)
  - Surface: `#ffffff` (white)
  - Text Primary: `#111827` (gray-900)
  - Text Secondary: `#6b7280` (gray-500)
  - Borders: `#e5e7eb` (gray-200)

- **Dark Mode**:
  - Background: `#111827` (gray-900)
  - Surface: `#1f2937` (gray-800)
  - Text Primary: `#f9fafb` (gray-50)
  - Text Secondary: `#9ca3af` (gray-400)
  - Borders: `#374151` (gray-700)

### Spacing System

Use Tailwind's spacing scale (based on 0.25rem = 4px increments):

#### Standard Spacing Scale

- **None**: `0` (0px)
- **XS**: `1` (4px) - Tight spacing within components
- **SM**: `2` (8px) - Related elements, form field spacing
- **MD**: `4` (16px) - **Default spacing between components**
- **LG**: `6` (24px) - Section spacing, card padding
- **XL**: `8` (32px) - Major section breaks
- **2XL**: `12` (48px) - Page sections
- **3XL**: `16` (64px) - Hero sections, major dividers

#### Component-Specific Spacing

```typescript
// Card/Panel Padding
const CARD_PADDING = "p-6"; // 24px

// Form Field Spacing
const FIELD_GAP = "gap-4"; // 16px between fields
const LABEL_MARGIN = "mb-2"; // 8px below labels

// Button Spacing
const BUTTON_GAP = "gap-2"; // 8px between buttons
const BUTTON_PADDING = "px-4 py-2"; // PrimeVue default

// Page Layout
const PAGE_PADDING = "p-6"; // 24px page padding
const SECTION_GAP = "gap-6"; // 24px between sections

// Grid Layouts
const GRID_GAP = "gap-4"; // 16px in data grids
const COLUMN_GAP = "gap-6"; // 24px between columns
```

### Typography

#### Font Family

- **Primary**: System font stack from PrimeVue default
- **Monospace**: For code, IDs, technical data

#### Font Sizes (Tailwind Classes)

- **Heading 1**: `text-3xl font-bold` (30px) - Page titles
- **Heading 2**: `text-2xl font-semibold` (24px) - Section headers
- **Heading 3**: `text-xl font-semibold` (20px) - Subsection headers
- **Heading 4**: `text-lg font-medium` (18px) - Card headers
- **Body**: `text-base` (16px) - Default body text
- **Small**: `text-sm` (14px) - Helper text, table data
- **XS**: `text-xs` (12px) - Labels, metadata

#### Font Weights

- **Light**: `font-light` (300) - Rarely used
- **Normal**: `font-normal` (400) - Body text
- **Medium**: `font-medium` (500) - Emphasis, labels
- **Semibold**: `font-semibold` (600) - Headers, important text
- **Bold**: `font-bold` (700) - Strong emphasis

#### Line Height

- **Tight**: `leading-tight` - Headers
- **Normal**: `leading-normal` - Body text (default)
- **Relaxed**: `leading-relaxed` - Long-form content

### Layout Patterns

#### Page Layout Structure

```vue
<template>
  <div class="min-h-screen bg-gray-50 dark:bg-gray-900">
    <!-- Page Content -->
    <div class="p-6">
      <!-- Page Header -->
      <div class="mb-6">
        <h1 class="text-3xl font-bold text-gray-900 dark:text-gray-50">
          Page Title
        </h1>
        <p class="mt-2 text-sm text-gray-500 dark:text-gray-400">
          Page description or breadcrumb
        </p>
      </div>

      <!-- Page Actions -->
      <div class="mb-6 flex justify-between items-center">
        <div class="flex gap-2">
          <!-- Filters, search -->
        </div>
        <div class="flex gap-2">
          <!-- Action buttons -->
        </div>
      </div>

      <!-- Main Content -->
      <Card>
        <!-- Content here -->
      </Card>
    </div>
  </div>
</template>
```

#### Grid Layouts

```vue
<!-- Responsive Grid Pattern -->
<div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
  <!-- Cards or items -->
</div>

<!-- Form Grid Pattern -->
<div class="grid grid-cols-1 md:grid-cols-2 gap-4">
  <!-- Form fields in 2 columns on medium+ screens -->
</div>
```

#### Flex Patterns

```vue
<!-- Header with Actions -->
<div class="flex justify-between items-center mb-6">
  <h2 class="text-2xl font-semibold">Section Title</h2>
  <div class="flex gap-2">
    <Button label="Action 1" />
    <Button label="Action 2" />
  </div>
</div>

<!-- Vertical Stack -->
<div class="flex flex-col gap-4">
  <!-- Stacked items -->
</div>
```

## Reusable Components to Create

**NOTE**: Many of these components have already been implemented and are available in `components/shared/`:
- ✅ **PageHeader** - Available at `components/shared/PageHeader.vue`
- ✅ **EmptyState** - Available at `components/shared/EmptyState.vue`
- ✅ **LoadingState** - Available at `components/shared/LoadingState.vue`
- ✅ **DataTableActions** - Available at `components/shared/DataTableActions.vue`
- ✅ **StatCard** - Available at `components/shared/StatCard.vue`
- ✅ **ThemeSwitcher** - Available at `components/shared/ThemeSwitcher.vue`
- ✅ **LanguageSwitcher** - Available at `components/shared/LanguageSwitcher.vue`

**Always check the `components/shared/` directory and frontend-agent.md documentation before creating new components.**

## Implemented UX Patterns

### ✅ Warehouse Module Patterns (Reference Implementation)

The Warehouse module serves as a **reference implementation** for all future CRUD modules. It demonstrates:

1. **List Page Pattern** (`pages/inventory/warehouses/index.vue`)
   - PageHeader with title, description, and "Create" action
   - DataTable with pagination, sorting, and filtering
   - Status badges (Active/Inactive) using Tag component with severity
   - Action column with view/edit/delete icons
   - EmptyState when no warehouses exist
   - Delete confirmation with ConfirmDialog
   - Toast notifications for success/error feedback

2. **Create/Edit Form Pattern** (`pages/inventory/warehouses/new.vue`, `[id]/edit.vue`)
   - Multi-section forms with Card components
   - Sections: Basic Info, Address Info, Contact Info, Additional Info
   - Vuelidate validation with real-time error display
   - InputText, Textarea, InputNumber, InputSwitch components
   - Helper text for complex fields (e.g., code format)
   - Form actions: Cancel (outlined) + Submit (filled primary)
   - Breadcrumb navigation
   - Loading state during submission

3. **View/Details Page Pattern** (`pages/inventory/warehouses/[id]/index.vue`)
   - Breadcrumb navigation (Home > Warehouses > {name})
   - Header with warehouse name and Edit button
   - Organized detail sections matching form structure
   - Status display with Tag component
   - Empty value handling (shows "—" for null/empty fields)
   - Loading state while fetching data
   - Error handling with error messages

4. **i18n Pattern for Features**
   - Dedicated translation namespace (`warehouses.*`)
   - Keys for: title, description, create, edit, field labels, placeholders, helpers
   - Success/error messages
   - Empty state messages
   - Translations in all supported languages (en, es, fr, de)
   - Proper escaping for special characters (e.g., `{'@'}` for email domains)

5. **API Integration Pattern** (`composables/useWarehouse.ts`)
   - Composable for each entity type
   - Methods: getAll, getById, create, update, delete
   - Error handling with try/catch and toast notifications
   - Loading states
   - Type-safe with TypeScript interfaces
   - Base URL from runtime config (no `/api/v1` prefix in endpoint paths)

**Apply these patterns when implementing**:
- Products, Stock Movements, Suppliers (Inventory)
- Invoices, Customers, Payments (Billing)
- Any new CRUD feature

### 1. PageHeader Component

**Purpose**: Standardize page headers with title, description, breadcrumbs, and actions

```vue
<!-- components/shared/PageHeader.vue -->
<script setup lang="ts">
interface Props {
  title: string;
  description?: string;
  showBreadcrumb?: boolean;
}

defineProps<Props>();
</script>

<template>
  <div class="mb-6">
    <Breadcrumb v-if="showBreadcrumb" />
    <div class="flex justify-between items-start mt-4">
      <div>
        <h1 class="text-3xl font-bold text-gray-900 dark:text-gray-50">
          {{ title }}
        </h1>
        <p
          v-if="description"
          class="mt-2 text-sm text-gray-500 dark:text-gray-400"
        >
          {{ description }}
        </p>
      </div>
      <div class="flex gap-2">
        <slot name="actions" />
      </div>
    </div>
  </div>
</template>
```

**Usage**:

```vue
<PageHeader
  title="Invoices"
  description="Manage and track all customer invoices"
  show-breadcrumb
>
  <template #actions>
    <Button label="New Invoice" icon="pi pi-plus" />
  </template>
</PageHeader>
```

### 2. EmptyState Component

**Purpose**: Consistent empty state displays when no data is available

```vue
<!-- components/shared/EmptyState.vue -->
<script setup lang="ts">
interface Props {
  icon?: string;
  title: string;
  description?: string;
  actionLabel?: string;
  actionIcon?: string;
}

const props = defineProps<Props>();
const emit = defineEmits<{
  action: [];
}>();
</script>

<template>
  <div class="flex flex-col items-center justify-center py-12 text-center">
    <i
      v-if="icon"
      :class="icon"
      class="text-6xl text-gray-400 dark:text-gray-600 mb-4"
    />
    <h3 class="text-lg font-semibold text-gray-900 dark:text-gray-50 mb-2">
      {{ title }}
    </h3>
    <p
      v-if="description"
      class="text-sm text-gray-500 dark:text-gray-400 mb-6 max-w-md"
    >
      {{ description }}
    </p>
    <Button
      v-if="actionLabel"
      :label="actionLabel"
      :icon="actionIcon"
      @click="emit('action')"
    />
  </div>
</template>
```

**Usage**:

```vue
<EmptyState
  icon="pi pi-inbox"
  title="No invoices yet"
  description="Create your first invoice to get started"
  action-label="Create Invoice"
  action-icon="pi pi-plus"
  @action="createInvoice"
/>
```

### 3. LoadingState Component

**Purpose**: Consistent loading indicators across the app

```vue
<!-- components/shared/LoadingState.vue -->
<script setup lang="ts">
interface Props {
  message?: string;
  overlay?: boolean;
}

defineProps<Props>();
</script>

<template>
  <div
    :class="[
      'flex flex-col items-center justify-center py-12',
      overlay && 'absolute inset-0 bg-white/80 dark:bg-gray-900/80 z-10',
    ]"
  >
    <ProgressSpinner />
    <p v-if="message" class="mt-4 text-sm text-gray-500 dark:text-gray-400">
      {{ message }}
    </p>
  </div>
</template>
```

### 4. StatusBadge Component

**Purpose**: Consistent status indicators across entities

```vue
<!-- components/shared/StatusBadge.vue -->
<script setup lang="ts">
import type { TagSeverity } from "~/types/ui";

interface Props {
  status: string;
  severity?: TagSeverity;
  size?: "small" | "normal" | "large";
}

const props = withDefaults(defineProps<Props>(), {
  severity: "info",
  size: "normal",
});
</script>

<template>
  <Tag
    :value="status"
    :severity="severity"
    :class="size === 'small' ? 'text-xs' : ''"
  />
</template>
```

### 5. ConfirmDialog Component

**Purpose**: Standardize confirmation dialogs for destructive actions

```vue
<!-- components/shared/ConfirmDialog.vue -->
<script setup lang="ts">
interface Props {
  visible: boolean;
  title: string;
  message: string;
  confirmLabel?: string;
  cancelLabel?: string;
  severity?: "danger" | "warning" | "info";
}

const props = withDefaults(defineProps<Props>(), {
  confirmLabel: "Confirm",
  cancelLabel: "Cancel",
  severity: "danger",
});

const emit = defineEmits<{
  "update:visible": [value: boolean];
  confirm: [];
  cancel: [];
}>();

function handleConfirm() {
  emit("confirm");
  emit("update:visible", false);
}

function handleCancel() {
  emit("cancel");
  emit("update:visible", false);
}
</script>

<template>
  <Dialog
    :visible="visible"
    :header="title"
    :modal="true"
    :closable="true"
    class="w-full max-w-md"
    @update:visible="emit('update:visible', $event)"
  >
    <p class="text-gray-700 dark:text-gray-300">{{ message }}</p>

    <template #footer>
      <Button :label="cancelLabel" severity="secondary" @click="handleCancel" />
      <Button
        :label="confirmLabel"
        :severity="severity"
        @click="handleConfirm"
      />
    </template>
  </Dialog>
</template>
```

### 6. FormSection Component

**Purpose**: Group related form fields with consistent spacing and styling

```vue
<!-- components/shared/FormSection.vue -->
<script setup lang="ts">
interface Props {
  title?: string;
  description?: string;
}

defineProps<Props>();
</script>

<template>
  <div class="mb-6">
    <div v-if="title || description" class="mb-4">
      <h3
        v-if="title"
        class="text-lg font-semibold text-gray-900 dark:text-gray-50"
      >
        {{ title }}
      </h3>
      <p
        v-if="description"
        class="mt-1 text-sm text-gray-500 dark:text-gray-400"
      >
        {{ description }}
      </p>
    </div>
    <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
      <slot />
    </div>
  </div>
</template>
```

### 7. DataTableActions Component

**Purpose**: Standardize action buttons in DataTable rows

```vue
<!-- components/shared/DataTableActions.vue -->
<script setup lang="ts">
interface Props {
  showView?: boolean;
  showEdit?: boolean;
  showDelete?: boolean;
}

const props = withDefaults(defineProps<Props>(), {
  showView: true,
  showEdit: true,
  showDelete: true,
});

const emit = defineEmits<{
  view: [];
  edit: [];
  delete: [];
}>();
</script>

<template>
  <div class="flex gap-2">
    <Button
      v-if="showView"
      icon="pi pi-eye"
      severity="info"
      text
      rounded
      @click="emit('view')"
    />
    <Button
      v-if="showEdit"
      icon="pi pi-pencil"
      severity="warning"
      text
      rounded
      @click="emit('edit')"
    />
    <Button
      v-if="showDelete"
      icon="pi pi-trash"
      severity="danger"
      text
      rounded
      @click="emit('delete')"
    />
  </div>
</template>
```

### 8. StatCard Component

**Purpose**: Display key metrics and statistics consistently

```vue
<!-- components/shared/StatCard.vue -->
<script setup lang="ts">
interface Props {
  title: string;
  value: string | number;
  icon?: string;
  trend?: number;
  trendLabel?: string;
  severity?: "success" | "warning" | "danger" | "info";
}

defineProps<Props>();
</script>

<template>
  <Card class="h-full">
    <template #content>
      <div class="flex items-start justify-between">
        <div class="flex-1">
          <p class="text-sm font-medium text-gray-500 dark:text-gray-400">
            {{ title }}
          </p>
          <p class="mt-2 text-3xl font-bold text-gray-900 dark:text-gray-50">
            {{ value }}
          </p>
          <div v-if="trend !== undefined" class="mt-2 flex items-center gap-1">
            <i
              :class="[
                'pi text-sm',
                trend > 0
                  ? 'pi-arrow-up text-green-600'
                  : 'pi-arrow-down text-red-600',
              ]"
            />
            <span class="text-sm text-gray-600 dark:text-gray-400">
              {{ trendLabel }}
            </span>
          </div>
        </div>
        <div
          v-if="icon"
          :class="[
            'rounded-lg p-3',
            severity === 'success' &&
              'bg-green-100 text-green-600 dark:bg-green-900 dark:text-green-400',
            severity === 'warning' &&
              'bg-amber-100 text-amber-600 dark:bg-amber-900 dark:text-amber-400',
            severity === 'danger' &&
              'bg-red-100 text-red-600 dark:bg-red-900 dark:text-red-400',
            (!severity || severity === 'info') &&
              'bg-teal-100 text-teal-600 dark:bg-teal-900 dark:text-teal-400',
          ]"
        >
          <i :class="[icon, 'text-2xl']" />
        </div>
      </div>
    </template>
  </Card>
</template>
```

## User Feedback Patterns

### Toast Messages

**Guidelines**:

- Use appropriate severity levels
- Keep messages concise (max 60 characters for title)
- Position: Top-right (default PrimeVue position)
- Duration: 3000ms for info/success, 5000ms for warnings/errors
- Always provide actionable feedback

**Standard Messages**:

```typescript
// Success responses
toast.success("Success", "Invoice created successfully");
toast.success("Saved", "Changes saved");

// Error responses
toast.error("Error", "Failed to load data");
toast.error("Validation Error", "Please check required fields");

// Warnings
toast.warn("Warning", "This action cannot be undone");

// Info
toast.info("Info", "Data refreshed");
```

### Loading States

**Guidelines**:

- Show loading indicator for operations > 300ms
- Use skeleton loaders for initial page loads
- Use spinner for actions/mutations
- Disable interactive elements during loading
- Provide feedback on completion

### Error States

**Guidelines**:

- Display user-friendly error messages
- Provide actionable next steps
- Use inline validation for forms
- Show errors near their source
- Offer retry options when applicable

## Accessibility Guidelines

### WCAG 2.1 AA Compliance

**Color Contrast**:

- Text: Minimum 4.5:1 contrast ratio
- Large text (18pt+): Minimum 3:1 contrast ratio
- Interactive elements: Minimum 3:1 contrast ratio

**Keyboard Navigation**:

- All interactive elements must be keyboard accessible
- Logical tab order (left-to-right, top-to-bottom)
- Visible focus indicators
- Escape key closes dialogs/overlays

**Screen Reader Support**:

- Semantic HTML elements
- ARIA labels for icons and interactive elements
- ARIA live regions for dynamic content
- Alt text for images

**Forms**:

- Associate labels with inputs
- Provide clear error messages
- Mark required fields
- Group related fields with fieldsets

### Focus Management

```vue
<!-- Good: Proper focus handling -->
<Button label="Submit" @click="handleSubmit" aria-label="Submit form" />

<!-- Good: Focus trap in dialogs -->
<Dialog :visible="visible" :close-on-escape="true" :modal="true" />
```

## Responsive Design Strategy

### Breakpoints (Tailwind defaults)

- **SM**: `640px` - Small tablets
- **MD**: `768px` - Tablets and small laptops
- **LG**: `1024px` - Desktops
- **XL**: `1280px` - Large desktops
- **2XL**: `1536px` - Extra large screens

### Mobile-First Approach

- Design for mobile first, enhance for larger screens
- Stack layouts vertically on mobile
- Use hamburger menu for navigation on mobile
- Ensure touch targets are minimum 44x44px
- Optimize data tables for mobile (responsive cards)

### Responsive Patterns

```vue
<!-- Responsive Grid -->
<div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
  <!-- Auto-adapts: 1 col mobile, 2 cols tablet, 3 cols desktop -->
</div>

<!-- Responsive Flex -->
<div class="flex flex-col md:flex-row gap-4">
  <!-- Stack on mobile, row on tablet+ -->
</div>

<!-- Conditional Rendering -->
<div class="hidden md:block">Desktop only</div>
<div class="block md:hidden">Mobile only</div>
```

## Navigation Patterns

### Primary Navigation

- Top menubar with main sections
- Sticky on scroll for easy access
- Highlight active section
- Responsive collapse on mobile

### Breadcrumbs

- Show on all interior pages
- Home icon for root
- Current page is not clickable
- Max 4 levels deep

### Contextual Navigation

- Tabs for related views within a section
- Side navigation for multi-step processes
- Back button for nested details

## Data Display Patterns

### Tables

- Use DataTable for tabular data
- Enable sorting on relevant columns
- Add filtering for large datasets
- Implement pagination (10, 25, 50 rows)
- Responsive: Convert to cards on mobile
- Highlight row on hover
- Selection for bulk actions

### Cards

- Use for dashboard widgets
- Group related information
- Include header with title and optional actions
- Consistent padding (p-6)
- Subtle shadow and border

### Lists

- Use for simple collections
- Include dividers between items
- Show empty state when no items
- Enable infinite scroll for long lists

## Form Patterns

### Form Layout

- Group related fields
- Use 2-column layout on desktop, 1-column on mobile
- Place primary action on right (Submit)
- Place secondary actions on left (Cancel)
- Show required field indicators

### Field Validation

- Inline validation on blur
- Real-time validation for complex rules
- Clear error messages below fields
- Success indicators for valid fields
- Disable submit until valid

### Multi-Step Forms

- Show progress indicator
- Allow navigation between steps
- Validate each step before proceeding
- Save draft functionality for long forms

## Interactive Patterns

### Hover States

- Subtle background change
- Cursor change to pointer for clickable items
- Tooltip for additional context
- Icon color change

### Active States

- Visual feedback on click/tap
- Ripple effect (PrimeVue default)
- Loading state during async operations

### Disabled States

- Reduced opacity (60%)
- No pointer cursor
- Tooltip explaining why disabled

## Performance UX Guidelines

### Perceived Performance

- Show skeleton loaders during initial load
- Optimistic UI updates
- Immediate visual feedback for actions
- Progressive loading for large datasets

### Actual Performance

- Lazy load images and heavy components
- Paginate large lists
- Debounce search inputs (300ms)
- Cache frequently accessed data

## Implementation Checklist

When creating new features or components:

- [ ] Follow spacing system (use defined Tailwind classes)
- [ ] Use PrimeVue components when available
- [ ] Implement dark mode support
- [ ] Add loading and error states
- [ ] Include empty states
- [ ] Ensure keyboard accessibility
- [ ] Test with screen reader
- [ ] Verify color contrast ratios
- [ ] Responsive on mobile, tablet, desktop
- [ ] Add appropriate toast messages
- [ ] Include proper ARIA labels
- [ ] Follow established naming patterns
- [ ] Reuse shared components

## Best Practices Summary

1. **NO Custom CSS**: Use PrimeVue components and Tailwind utilities only
2. **Consistency**: Use established patterns and components
3. **Accessibility**: Design for all users from the start
4. **Feedback**: Always inform users of system state
5. **Simplicity**: Don't over-complicate UI
6. **Performance**: Optimize perceived and actual performance
7. **Responsiveness**: Mobile-first, enhance for larger screens
8. **Error Handling**: Graceful degradation with clear messaging
9. **Documentation**: Comment complex UI logic
10. **Testing**: Verify accessibility and responsiveness
11. **Iteration**: Gather feedback and improve continuously
12. **PrimeVue First**: Check if PrimeVue has the component before creating custom solutions

### Design System Compliance Checklist

When creating or reviewing any UI component:

- [ ] ✅ Uses PrimeVue components where available
- [ ] ✅ Styled exclusively with Tailwind utility classes
- [ ] ✅ Uses PrimeVue severity/size/variant props for variations
- [ ] ✅ Works in both light and dark modes
- [ ] ✅ No `<style>` or `<style scoped>` blocks
- [ ] ✅ Follows spacing system (gap-4, p-6, etc.)
- [ ] ✅ Uses semantic color classes (text-gray-900, dark:text-white)
- [ ] ✅ Accessible (WCAG 2.1 AA compliant)
- [ ] ✅ Responsive (mobile-first approach)
- [ ] ✅ Consistent with existing patterns

**Remember**: If a pattern requires custom CSS, it violates our standards. Rethink the approach using PrimeVue + Tailwind.
