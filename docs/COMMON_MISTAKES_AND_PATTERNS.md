# Common Mistakes and Patterns Guide

This document catalogs recurring mistakes and establishes patterns to avoid them in future development.

## Table of Contents
- [Form Development Patterns](#form-development-patterns)
- [i18n and Localization](#i18n-and-localization)
- [Component Usage](#component-usage)
- [Validation Patterns](#validation-patterns)
- [Pre-Implementation Checklist](#pre-implementation-checklist)

---

## Form Development Patterns

### ❌ MISTAKE: Using Non-Existent Components

**What Happened:**
- Used `<FormField>` component that doesn't exist in the project
- Created SRI Configuration form with non-existent component, causing all fields to fail rendering

**Why It's Wrong:**
- Project doesn't have a `FormField` abstraction component
- This causes silent failures where forms appear empty or broken

### ✅ CORRECT PATTERN: Use PrimeVue Components Directly

**Always follow the Warehouse implementation pattern:**

```vue
<!-- ✅ CORRECT: Standard form field pattern -->
<div class="flex flex-col gap-2">
  <label for="fieldName" class="font-semibold text-slate-700 dark:text-slate-200">
    {{ t('namespace.field_label') }} <!-- Add * for required fields -->
  </label>
  <InputText
    id="fieldName"
    v-model="formData.fieldName"
    :invalid="v$.fieldName.$error"
    :placeholder="t('namespace.field_placeholder')"
    @blur="v$.fieldName.$touch()"
  />
  <small v-if="v$.fieldName.$error" class="text-red-600 dark:text-red-400">
    {{ v$.fieldName.$errors[0].$message }}
  </small>
  <small v-else-if="helperText" class="text-slate-500 dark:text-slate-400">
    {{ t('namespace.field_helper') }}
  </small>
</div>

<!-- ❌ WRONG: Using FormField component -->
<FormField
  v-model="formData.fieldName"
  name="fieldName"
  :label="t('namespace.field_label')"
/>
```

**Reference Implementation:**
- See `frontend/pages/inventory/warehouses/new.vue` for complete form example
- See `frontend/pages/inventory/warehouses/[id]/edit.vue` for edit form pattern

---

## i18n and Localization

### ❌ MISTAKE: Incorrect @ Symbol Escaping

**What Happened:**
- Used `@@` to escape @ symbols in email placeholders
- Caused vue-i18n parse errors: "Invalid linked format"

**Why It's Wrong:**
- Project uses `{'@'}` format, not `@@` for escaping
- Different escaping methods cause incompatibility

### ✅ CORRECT PATTERN: Use `{'@'}` for Email Addresses

```json
{
  "email_placeholder": "contact{'@'}company.com",
  "warehouse_email_placeholder": "warehouse{'@'}company.com"
}
```

**Reference:**
- See `frontend/i18n/locales/en.json` - existing email placeholders
- Search for `{'@'}` to find all examples

### 📋 i18n Special Character Escaping Reference

| Character | How to Escape | Example |
|-----------|---------------|---------|
| `@` (email) | `{'@'}` | `user{'@'}domain.com` |
| `{` (literal) | `'{}'` | `{'{'}variable{'}'}` |
| `'` (apostrophe) | `\'` or use unicode | `don't` or `don\u0027t` |

**Before Adding i18n Keys:**
1. Check `docs/i18n-standards.md` for naming conventions
2. Search existing locale files for similar patterns
3. Verify special character escaping matches project standards

---

## Component Usage

### ❌ MISTAKE: Assuming Component Existence

**What Happened:**
- Assumed `FormField` component exists without checking
- Implemented entire feature using non-existent component

**Why It's Wrong:**
- Wastes development time
- Creates broken UI that requires complete rewrite

### ✅ CORRECT PATTERN: Verify Component Availability

**Before using a component:**

1. **Search for component definition:**
   ```bash
   # Search for component files
   git ls-files | grep -i "ComponentName.vue"

   # Or use VS Code search
   # Search: **/ComponentName.vue
   ```

2. **Check component imports in existing pages:**
   ```typescript
   // Look for how components are imported
   import ComponentName from '~/components/...'
   ```

3. **Reference similar implementations:**
   - Find a working page with similar functionality
   - Copy its component usage patterns

**Available Components Pattern (Based on Warehouse Implementation):**
- `Card` - Wrapper for content sections
- `PageHeader` - Page title and description
- `InputText` - Text input fields
- `Textarea` - Multi-line text input
- `Dropdown` - Select dropdowns
- `InputSwitch` - Toggle switches
- `Button` - Action buttons
- `DataTable` - Data tables
- `Tag` - Status badges
- `LoadingState` - Loading indicators

---

## Validation Patterns

### ❌ MISTAKE: Incomplete Vuelidate Rules

**What Happened:**
- Created form with `environment` and `accountingRequired` fields
- Did not include them in Vuelidate rules object
- Form always failed validation: "El formulario contiene errores"

**Why It's Wrong:**
- Vuelidate only validates fields defined in rules
- Missing fields cause validation to fail even when filled correctly

### ✅ CORRECT PATTERN: All Form Fields Must Be in Rules

```typescript
// ✅ CORRECT: All formData fields have corresponding rules
const formData = reactive({
  name: '',
  email: '',
  isActive: false,
  environment: SriEnvironment.Test,
})

const rules = computed(() => ({
  name: { required },
  email: { required, email },
  isActive: {},  // No validation, but must be present
  environment: { required },
}))
```

**Rule:**
- **Every field in `formData` MUST have an entry in `rules`**
- Even if no validation needed, use empty object: `fieldName: {}`
- Omitting a field from rules = validation always fails

**Boolean/Toggle Fields:**
```typescript
// ✅ Always include in rules, even with no validation
accountingRequired: {},
isActive: {},
isRiseRegime: {},
```

---

## Pre-Implementation Checklist

### Before Implementing Any New Feature

#### 1. ✅ Find Reference Implementation
- [ ] Search for similar existing feature in codebase
- [ ] Identify the canonical reference (see `AGENTS.md`)
- [ ] Review implementation approach

**Example:**
```bash
# For CRUD features, check Warehouse module
frontend/pages/inventory/warehouses/
backend/src/Application/Features/Warehouse/

# For forms, check
frontend/pages/inventory/warehouses/new.vue
```

#### 2. ✅ Verify Component Availability
- [ ] Check if components exist before using them
- [ ] Search existing pages for component import patterns
- [ ] Use only verified, existing components

#### 3. ✅ Check i18n Standards
- [ ] Review `docs/i18n-standards.md`
- [ ] Search locale files for similar translation patterns
- [ ] Verify special character escaping format

#### 4. ✅ Validation Rules Complete
- [ ] Every `formData` field has entry in `rules`
- [ ] Boolean fields included (even if empty rules)
- [ ] Test validation before declaring complete

#### 5. ✅ Backend-Frontend Alignment
- [ ] DTO properties match between backend and frontend
- [ ] All entity fields represented in forms (if applicable)
- [ ] Migration created for new database fields

---

## Common Error Messages and Solutions

### "Invalid linked format" in vue-i18n

**Cause:** Incorrect `@` symbol escaping in locale files

**Solution:**
```json
// ❌ WRONG
"email_placeholder": "user@example.com"
"email_placeholder": "user@@example.com"

// ✅ CORRECT
"email_placeholder": "user{'@'}example.com"
```

### "El formulario contiene errores" (Form Validation Always Fails)

**Cause:** Form fields missing from Vuelidate rules

**Solution:**
1. List all fields in `formData`
2. Ensure all have entries in `rules` computed property
3. Use `{}` for fields without specific validation

```typescript
// Add all fields, even boolean toggles
const rules = computed(() => ({
  // ... other rules
  booleanField: {},  // ✅ Must be present
  toggleField: {},   // ✅ Must be present
}))
```

### Form Fields Not Rendering

**Cause:** Using non-existent component (e.g., `FormField`)

**Solution:**
1. Remove custom component usage
2. Use standard PrimeVue components pattern
3. Reference `frontend/pages/inventory/warehouses/new.vue`

---

## Quick Reference: Form Field Template

Copy this template when creating new form fields:

```vue
<!-- Standard Text Input -->
<div class="flex flex-col gap-2">
  <label for="fieldId" class="font-semibold text-slate-700 dark:text-slate-200">
    {{ t('namespace.field_label') }} <span v-if="required" class="text-red-500">*</span>
  </label>
  <InputText
    id="fieldId"
    v-model="formData.fieldName"
    :invalid="v$.fieldName.$error"
    :placeholder="t('namespace.field_placeholder')"
    @blur="v$.fieldName.$touch()"
  />
  <small v-if="v$.fieldName.$error" class="text-red-600 dark:text-red-400">
    {{ v$.fieldName.$errors[0].$message }}
  </small>
  <small v-else class="text-slate-500 dark:text-slate-400">
    {{ t('namespace.field_helper') }}
  </small>
</div>

<!-- Dropdown -->
<div class="flex flex-col gap-2">
  <label for="dropdownId" class="font-semibold text-slate-700 dark:text-slate-200">
    {{ t('namespace.dropdown_label') }} *
  </label>
  <Dropdown
    id="dropdownId"
    v-model="formData.selectedValue"
    :options="options"
    option-label="label"
    option-value="value"
    :placeholder="t('namespace.dropdown_placeholder')"
    class="w-full"
  />
  <small v-if="v$.selectedValue.$error" class="text-red-600 dark:text-red-400">
    {{ v$.selectedValue.$errors[0].$message }}
  </small>
</div>

<!-- Toggle Switch -->
<div class="flex items-center gap-3">
  <InputSwitch
    id="toggleId"
    v-model="formData.toggleValue"
    :aria-label="t('namespace.toggle_label')"
  />
  <label for="toggleId" class="cursor-pointer text-sm font-medium text-slate-700 dark:text-slate-300">
    {{ t('namespace.toggle_label') }}
  </label>
</div>
```

---

## Related Documentation

- `docs/WAREHOUSE_IMPLEMENTATION_REFERENCE.md` - Complete CRUD reference
- `docs/i18n-standards.md` - Translation naming and patterns
- `docs/frontend-agent.md` - Frontend implementation standards
- `AGENTS.md` - Agent specializations and reference implementations

---

## Lessons Learned Log

### Issue: SRI Configuration Form (February 2026)

**Mistakes Made:**
1. ❌ Used non-existent `FormField` component
2. ❌ Used `@@` instead of `{'@'}` for email escaping
3. ❌ Omitted `environment` and `accountingRequired` from validation rules
4. ❌ Didn't check existing form implementations first

**What Should Have Been Done:**
1. ✅ Search for existing form implementations first
2. ✅ Copy patterns from `warehouse/new.vue`
3. ✅ Check all email placeholders for `{'@'}` pattern
4. ✅ Include ALL formData fields in validation rules
5. ✅ Test form rendering before moving to validation

**Time Wasted:** ~30 minutes debugging and fixing

**Prevention:** Follow this checklist before implementing forms

---

## Standard Development Workflow

### When Adding Any New Feature:

```
1. Read AGENTS.md
   ↓
2. Identify relevant agent/reference
   ↓
3. Read reference documentation (e.g., WAREHOUSE_IMPLEMENTATION_REFERENCE.md)
   ↓
4. Search codebase for similar examples
   ↓
5. Copy proven patterns
   ↓
6. Adapt to specific requirements
   ↓
7. Test thoroughly before declaring complete
```

### Never:
- ❌ Assume components exist without verification
- ❌ Create new patterns when existing ones work
- ❌ Skip checking reference implementations
- ❌ Use placeholder code markers in production
- ❌ Declare work complete without testing

### Always:
- ✅ Reference `WAREHOUSE_IMPLEMENTATION_REFERENCE.md` for CRUD features
- ✅ Check `i18n-standards.md` before adding translations
- ✅ Search for similar implementations first
- ✅ Test forms: render → validation → submission
- ✅ Include all formData fields in validation rules

---

## Maintenance

**This document should be updated whenever:**
- New recurring mistakes are identified
- Standard patterns change
- New components become available
- Better solutions are discovered

**Last Updated:** February 12, 2026
**Contributors:** GitHub Copilot, Development Team
