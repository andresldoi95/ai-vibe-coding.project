# i18n Standards and Conventions

**Purpose**: This document defines the internationalization (i18n) standards, naming conventions, and patterns used across the SaaS Billing + Inventory Management System to ensure consistency and prevent translation errors.

## Table of Contents

1. [General Principles](#general-principles)
2. [File Structure](#file-structure)
3. [Naming Conventions](#naming-conventions)
4. [CRUD Module Pattern](#crud-module-pattern)
5. [Form Field Patterns](#form-field-patterns)
6. [Message Patterns](#message-patterns)
7. [Validation Patterns](#validation-patterns)
8. [Common Reusable Keys](#common-reusable-keys)
9. [Interpolation Guidelines](#interpolation-guidelines)
10. [Pluralization Rules](#pluralization-rules)
11. [Best Practices](#best-practices)
12. [Common Mistakes to Avoid](#common-mistakes-to-avoid)

---

## General Principles

### ✅ DO
- Use **snake_case** for translation keys (e.g., `created_successfully`, `unit_price`)
- Keep keys **lowercase** except for acronyms (e.g., `export_excel`, `sri_configuration`)
- Be **descriptive** and **specific** to the context
- Reuse **common keys** from the `common` namespace when possible
- Follow **established patterns** for similar features

### ❌ DON'T
- Mix camelCase, PascalCase, or kebab-case in keys
- Create duplicate keys that already exist in `common`
- Use overly generic keys that lack context
- Hardcode text in components - always use i18n keys

---

## File Structure

### Supported Locales

```
frontend/i18n/locales/
├── en.json  # English (primary)
├── es.json  # Spanish
├── fr.json  # French
└── de.json  # German
```

### Top-Level Namespaces

```json
{
  "app": {},              // Application-level metadata
  "common": {},           // Shared/reusable translations
  "nav": {},              // Navigation menu items
  "auth": {},             // Authentication/authorization
  "validation": {},       // Validation messages (global)
  "messages": {},         // Generic success/error messages
  "roles": {},            // Roles & permissions

  // Feature modules (CRUD entities)
  "warehouses": {},
  "products": {},
  "customers": {},
  "invoices": {},
  "stock_movements": {},
  "establishments": {},
  "emissionPoints": {},   // Note: camelCase for multi-word entities
  "taxRates": {},
  "sriConfiguration": {},
  "invoiceConfig": {},

  // Domain-specific namespaces
  "sri": {}               // SRI-specific enums and labels
}
```

---

## Naming Conventions

### Namespace Naming

| Pattern | Example | Usage |
|---------|---------|-------|
| **Singular lowercase** | `"product": {}` | Single-word entity names |
| **camelCase** | `"emissionPoints": {}` | Multi-word entity names |
| **snake_case keys** | `"unit_price": "..."` | All keys within namespaces |

### Key Naming Patterns

```json
{
  "feature": {
    // Meta information
    "title": "Feature Title",
    "description": "Feature description",

    // Actions (verbs)
    "create": "Create Feature",
    "edit": "Edit Feature",
    "view": "View Feature",
    "delete": "Delete Feature",
    "search": "Search features...",
    "export": "Export",

    // Descriptions for actions
    "create_description": "Add a new feature",
    "edit_description": "Update feature information",
    "view_description": "View feature details",
    "empty_description": "No features found. Create your first feature.",

    // Field names (nouns)
    "field_name": "Field Label",
    "field_name_placeholder": "Placeholder text",
    "field_name_helper": "Helper text below field",
    "field_name_hint": "Tooltip or hint text",

    // Status/state labels
    "is_active": "Active feature",
    "status": "Status",
    "status_draft": "Draft",
    "status_active": "Active",

    // Messages
    "created_successfully": "Feature created successfully",
    "updated_successfully": "Feature updated successfully",
    "deleted_successfully": "Feature deleted successfully",
    "confirm_delete": "Are you sure you want to delete {name}?",

    // Errors
    "create_error": "Failed to create feature",
    "update_error": "Failed to update feature",
    "delete_error": "Failed to delete feature",
    "load_error": "Failed to load feature"
  }
}
```

---

## CRUD Module Pattern

Every CRUD feature should follow this **canonical structure** (based on Warehouse implementation):

```json
{
  "moduleName": {
    // ========================================
    // 1. MODULE METADATA
    // ========================================
    "title": "Module Title (Plural)",
    "description": "Brief description of what this module manages",

    // ========================================
    // 2. PAGE TITLES & DESCRIPTIONS
    // ========================================
    "create": "Create Module",
    "edit": "Edit Module",
    "view": "View Module",
    "create_description": "Add a new item to your database",
    "edit_description": "Update item information and details",
    "view_description": "View item details",
    "empty_description": "Create your first item to get started",

    // ========================================
    // 3. FORM SECTIONS
    // ========================================
    "basic_info": "Basic Information",
    "address_info": "Address Information",
    "contact_info": "Contact Information",
    "additional_info": "Additional Information",
    "pricing": "Pricing",
    "classification": "Classification",

    // ========================================
    // 4. FORM FIELDS
    // ========================================
    // Pattern: field_name, field_name_placeholder, field_name_helper/hint
    "name": "Item Name",
    "name_placeholder": "Enter item name",
    "code": "Item Code",
    "code_placeholder": "ITEM-001",
    "code_helper": "Uppercase letters, numbers, and hyphens only",
    "description": "Description",
    "description_placeholder": "Detailed description",
    "is_active": "Active item",

    // ========================================
    // 5. SEARCH & FILTERS
    // ========================================
    "search": "Search items...",
    "search_placeholder": "Search by name, code, or description",
    "filters": "Filters",
    "apply_filters": "Apply Filters",
    "reset_filters": "Reset Filters",
    "all_items": "All Items",
    "active_items": "Active",
    "inactive_items": "Inactive",

    // ========================================
    // 6. DATA TABLE COLUMNS
    // ========================================
    "created_at": "Created At",
    "updated_at": "Updated At",
    "status": "Status",
    "actions": "Actions",

    // ========================================
    // 7. SUCCESS MESSAGES
    // ========================================
    "created_successfully": "Item created successfully",
    "updated_successfully": "Item updated successfully",
    "deleted_successfully": "Item deleted successfully",

    // ========================================
    // 8. ERROR MESSAGES
    // ========================================
    "create_error": "Failed to create item",
    "update_error": "Failed to update item",
    "delete_error": "Failed to delete item",
    "load_error": "Failed to load item",

    // ========================================
    // 9. CONFIRMATIONS
    // ========================================
    "confirm_delete": "Are you sure you want to delete {name}?",
    "delete_title": "Delete Item",

    // ========================================
    // 10. EXPORT (if applicable)
    // ========================================
    "export": "Export",
    "export_data": "Export Data",
    "export_dialog_title": "Export Items",
    "export_description": "Export data in your preferred format",
    "export_format": "Export Format",
    "export_excel": "Excel (.xlsx)",
    "export_csv": "CSV (.csv)",
    "exporting": "Exporting...",
    "export_success": "Export completed successfully",
    "export_error": "Export failed"
  }
}
```

---

## Form Field Patterns

### Standard Field Structure

Every form field should have:

```json
{
  "field_name": "Field Label",                    // Display label
  "field_name_placeholder": "Placeholder text",   // Input placeholder
  "field_name_helper": "Helper text",             // Inline help (below field)
  "field_name_hint": "Tooltip or hint"            // Tooltip text (optional)
}
```

### Examples

```json
{
  "email": "Email Address",
  "email_placeholder": "you@example.com",
  "email_helper": "We'll never share your email with anyone",

  "warehouse_code": "Warehouse Code",
  "warehouse_code_placeholder": "WH-001",
  "warehouse_code_helper": "Uppercase letters, numbers, and hyphens only",

  "unit_price": "Unit Price",
  "unit_price_placeholder": "99.99",
  "unit_price_hint": "Price per unit in USD"
}
```

### Common Field Patterns

#### Boolean/Checkbox Fields
```json
{
  "is_active": "Active warehouse",
  "is_default": "Set as default",
  "require_approval": "Require approval"
}
```

#### Select/Dropdown Fields
```json
{
  "warehouse": "Warehouse",
  "select_warehouse": "Select a warehouse",
  "no_warehouses": "No warehouses available"
}
```

#### Date Fields
```json
{
  "start_date": "Start Date",
  "select_date": "Select date",
  "date_range": "Date Range",
  "from_date": "From Date",
  "to_date": "To Date"
}
```

---

## Message Patterns

### Success Messages

**Pattern**: `{action}_successfully`

```json
{
  "created_successfully": "Item created successfully",
  "updated_successfully": "Item updated successfully",
  "deleted_successfully": "Item deleted successfully",
  "saved_successfully": "Changes saved successfully",
  "sent_successfully": "Email sent successfully",
  "uploaded_successfully": "File uploaded successfully"
}
```

### Error Messages

**Pattern**: `{action}_error`

```json
{
  "create_error": "Failed to create item",
  "update_error": "Failed to update item",
  "delete_error": "Failed to delete item",
  "load_error": "Failed to load data",
  "upload_error": "Failed to upload file",
  "send_error": "Failed to send email"
}
```

### Confirmation Messages

**Pattern**: `confirm_{action}` with interpolation

```json
{
  "confirm_delete": "Are you sure you want to delete {name}?",
  "confirm_cancel": "Are you sure you want to cancel? Unsaved changes will be lost.",
  "confirm_submit": "Are you sure you want to submit this form?"
}
```

### Generic Messages (Reusable)

Use the `messages` namespace for generic messages:

```json
{
  "messages": {
    "success_save": "Saved successfully",
    "success_create": "Created successfully",
    "success_update": "Updated successfully",
    "success_delete": "Deleted successfully",
    "error_load": "Failed to load data",
    "error_save": "Failed to save",
    "confirm_delete": "Are you sure you want to delete this item?",
    "operation_cancelled": "Operation cancelled"
  }
}
```

---

## Validation Patterns

### Global Validation Messages

Located in the `validation` namespace:

```json
{
  "validation": {
    // Required fields
    "required": "This field is required",
    "email_required": "Email is required",
    "password_required": "Password is required",

    // Format validation
    "email_invalid": "Please enter a valid email address",
    "invalid_field": "This field is invalid",
    "invalid_form": "Form contains errors",

    // Length validation
    "min_length": "{field} must be at least {min} characters",
    "max_length": "{field} cannot exceed {max} characters",

    // Pattern validation
    "warehouse_code_format": "Warehouse code can only contain uppercase letters, numbers, and hyphens",
    "product_code_format": "Product code can only contain uppercase letters, numbers, and hyphens",
    "sku_format": "SKU can only contain uppercase letters, numbers, and hyphens",
    "establishment_code_format": "Establishment code must be exactly 3 digits (001-999)",
    "ruc_format": "RUC must be exactly 13 digits",
    "cedula_format": "Cédula must be exactly 10 digits",

    // Password validation
    "password_min_length": "Password must be at least 8 characters",
    "password_uppercase": "Password must contain at least one uppercase letter",
    "password_lowercase": "Password must contain at least one lowercase letter",
    "password_number": "Password must contain at least one number",
    "password_match": "Passwords must match",
    "passwordMismatch": "Passwords do not match",

    // Custom validations
    "initial_warehouse_required": "Warehouse is required when initial quantity is specified",
    "insufficient_permissions": "Insufficient permissions",
    "contact_admin": "You do not have permission to perform this action. Contact your administrator."
  }
}
```

### Field-Specific Validation

For complex fields, use feature-specific validation keys:

```json
{
  "products": {
    "code_helper": "Uppercase letters, numbers, and hyphens only. Must be unique.",
    "sku_helper": "Stock Keeping Unit. Must be unique.",
    "initial_warehouse_hint": "Required when initial quantity is set"
  }
}
```

---

## Common Reusable Keys

### From `common` Namespace

Always check if a key exists in `common` before creating a feature-specific one:

```json
{
  "common": {
    // Actions
    "save": "Save",
    "cancel": "Cancel",
    "delete": "Delete",
    "edit": "Edit",
    "create": "Create",
    "view": "View",
    "search": "Search",
    "close": "Close",
    "confirm": "Confirm",
    "back": "Back",
    "next": "Next",
    "previous": "Previous",
    "select": "Select",
    "export": "Export",
    "select_all": "Select All",
    "deselect_all": "Deselect All",
    "save_changes": "Save Changes",

    // States
    "loading": "Loading...",
    "sending": "Sending...",
    "processing": "Processing...",
    "exporting": "Exporting...",
    "no_data": "No data available",
    "active": "Active",
    "inactive": "Inactive",
    "valid": "Valid",
    "expired": "Expired",

    // General fields
    "status": "Status",
    "actions": "Actions",
    "description": "Description",
    "email": "Email",
    "phone": "Phone",
    "type": "Type",
    "created_at": "Created At",
    "updated_at": "Updated At",

    // Address fields
    "street_address": "Street Address",
    "city": "City",
    "state": "State / Province",
    "postal_code": "Postal Code",
    "country": "Country",

    // Sections
    "basic_info": "Basic Information",
    "audit_info": "Audit Information",

    // Filters
    "filters": "Filters",

    // Export
    "export_format": "Export Format",
    "export_description": "Export data in your preferred format",

    // Confirmations
    "confirm_delete": "Are you sure you want to delete this item?",
    "confirm_delete_item": "Are you sure you want to delete {name}?",

    // Misc
    "yes": "Yes",
    "no": "No",
    "not_specified": "Not specified"
  }
}
```

### Usage in Components

❌ **WRONG**:
```vue
<Button label="Save" />  <!-- Hardcoded -->
```

✅ **CORRECT**:
```vue
<Button :label="$t('common.save')" />
```

---

## Interpolation Guidelines

### Variable Interpolation

Use **curly braces** `{variable}` for dynamic values:

```json
{
  "confirm_delete": "Are you sure you want to delete {name}?",
  "items_count": "{count} item(s) found",
  "expires_in": "Expires in {days} day(s)",
  "min_length": "{field} must be at least {min} characters"
}
```

### Usage in Components

```typescript
// Single variable
$t('warehouses.confirm_delete', { name: warehouse.name })

// Multiple variables
$t('validation.min_length', { field: 'Password', min: 8 })

// In templates
{{ $t('products.items_count', { count: products.length }) }}
```

### Special Characters in Placeholders

For special characters like `@`, use **single quotes** `{'}`:

```json
{
  "email_placeholder": "you{'@'}example.com"
}
```

This prevents parsing errors in i18n libraries.

---

## Pluralization Rules

### Without nuxt-i18n Pluralization

Use **parentheses** for optional plural forms:

```json
{
  "items_count": "{count} item(s)",
  "users": "{count} user(s)",
  "invoices_issued_count": "{count} invoice(s)",
  "emission_points_count": "{count} emission point(s)"
}
```

### With nuxt-i18n Pluralization (Advanced)

If pluralization is enabled, use pipe-separated forms:

```json
{
  "items_count": "no items | {count} item | {count} items"
}
```

**Current project uses parentheses pattern**, not pipe separators.

---

## Best Practices

### 1. Consistency is Key

✅ **DO**: Follow established patterns from `warehouses`, `products`, `stock_movements`

❌ **DON'T**: Invent new patterns for similar features

### 2. Check Before Creating

Before adding a new key:

1. Search `common` namespace for reusable keys
2. Check similar features (e.g., `warehouses` for CRUD patterns)
3. Validate naming against this document

### 3. Maintain All Locales

When adding new keys:

- Add to **all locale files** (`en.json`, `es.json`, `fr.json`, `de.json`)
- Use English as the source, then translate
- Keep structure identical across all locales

### 4. Meaningful Context

❌ **BAD**: `"text1": "Click here"`

✅ **GOOD**: `"submit_invoice_button": "Submit Invoice"`

### 5. Avoid Hardcoded Text

**NEVER** hardcode user-facing text in components:

❌ **WRONG**:
```vue
<h1>Warehouses</h1>
<Button label="Create" />
```

✅ **CORRECT**:
```vue
<h1>{{ $t('warehouses.title') }}</h1>
<Button :label="$t('common.create')" />
```

### 6. Section Headers

For multi-section forms, use `_info` suffix:

```json
{
  "basic_info": "Basic Information",
  "address_info": "Address Information",
  "contact_info": "Contact Information",
  "additional_info": "Additional Information",
  "pricing_info": "Pricing Information"
}
```

### 7. Helper Text Naming

- `_placeholder`: Placeholder text inside input
- `_helper`: Help text below input (always visible)
- `_hint`: Tooltip or contextual help (may be hidden)

```json
{
  "code": "Code",
  "code_placeholder": "PROD-001",
  "code_helper": "Must be unique and uppercase",
  "code_hint": "Used for internal tracking"
}
```

---

## Common Mistakes to Avoid

### ❌ Mistake 1: Mixing Case Styles

```json
// WRONG
{
  "userName": "User Name",
  "user-email": "Email",
  "User_Phone": "Phone"
}

// CORRECT
{
  "user_name": "User Name",
  "user_email": "Email",
  "user_phone": "Phone"
}
```

### ❌ Mistake 2: Duplicating Common Keys

```json
// WRONG - "save" already exists in common
{
  "warehouses": {
    "save": "Save",
    "cancel": "Cancel"
  }
}

// CORRECT - Use common namespace
// In component: $t('common.save'), $t('common.cancel')
```

### ❌ Mistake 3: Inconsistent Success Messages

```json
// WRONG - Different patterns
{
  "created": "Warehouse created",
  "warehouse_updated_message": "Successfully updated warehouse",
  "deleteSuccess": "Deleted"
}

// CORRECT - Consistent pattern
{
  "created_successfully": "Warehouse created successfully",
  "updated_successfully": "Warehouse updated successfully",
  "deleted_successfully": "Warehouse deleted successfully"
}
```

### ❌ Mistake 4: Missing Placeholder/Helper Keys

```json
// WRONG - Field without placeholder/helper
{
  "email": "Email Address"
}

// CORRECT - Complete field definition
{
  "email": "Email Address",
  "email_placeholder": "you@example.com",
  "email_helper": "We'll never share your email"
}
```

### ❌ Mistake 5: Hardcoding Locale-Specific Values

```json
// WRONG - Hardcoding currency/units
{
  "price": "Price: $99.99",
  "weight": "Weight: 5 kg"
}

// CORRECT - Separate number from label
{
  "price": "Price",
  "weight": "Weight",
  "currency_usd": "USD",
  "unit_kg": "kg"
}
```

### ❌ Mistake 6: Overly Generic Keys

```json
// WRONG - Too generic
{
  "name": "Name",
  "code": "Code"
}

// CORRECT - Context-specific where needed
{
  "warehouse_name": "Warehouse Name",
  "warehouse_code": "Warehouse Code"
}

// OR use in specific namespace
{
  "warehouses": {
    "name": "Warehouse Name",
    "code": "Warehouse Code"
  }
}
```

### ❌ Mistake 7: Inconsistent Empty States

```json
// WRONG - Different patterns
{
  "no_data": "No warehouses",
  "empty_message": "There are no items to display"
}

// CORRECT - Consistent pattern
{
  "empty_description": "Create your first warehouse to get started",
  "no_records": "No warehouses found"
}
```

---

## Quick Reference: CRUD Checklist

When implementing a new CRUD feature, ensure you have:

- [ ] `title` - Module title (plural)
- [ ] `description` - Brief description
- [ ] `create`, `edit`, `view` - Action labels
- [ ] `create_description`, `edit_description`, `view_description` - Action descriptions
- [ ] `empty_description` - Empty state message
- [ ] Form section headers (`basic_info`, `address_info`, etc.)
- [ ] All form fields with `_placeholder` and `_helper`/`_hint`
- [ ] Search and filter keys
- [ ] `created_successfully`, `updated_successfully`, `deleted_successfully`
- [ ] `create_error`, `update_error`, `delete_error`, `load_error`
- [ ] `confirm_delete` with `{name}` interpolation
- [ ] Export keys (if applicable)
- [ ] **All keys added to ALL locale files** (en, es, fr, de)

---

## Examples from Canonical Implementations

### Warehouse (Complete Reference)

See [`frontend/i18n/locales/en.json`](../frontend/i18n/locales/en.json) - `warehouses` namespace (lines ~150-220)

Key highlights:
- Complete CRUD pattern implementation
- Multi-section form (`basic_info`, `address_info`, `contact_info`, `additional_info`)
- Export functionality keys
- Consistent message patterns

### Product (With Initial Stock)

See [`frontend/i18n/locales/en.json`](../frontend/i18n/locales/en.json) - `products` namespace (lines ~230-300)

Key highlights:
- Complex form with pricing, classification, physical properties
- Readonly field hints (`current_stock_level_readonly_hint`)
- Filter-specific keys (`price_range`, `low_stock`)

### Stock Movements (Transfer + Cost)

See [`frontend/i18n/locales/en.json`](../frontend/i18n/locales/en.json) - `stock_movements` namespace (lines ~560-600)

Key highlights:
- Movement types (IN, OUT, TRANSFER, ADJUSTMENT)
- Source/destination warehouse patterns
- Cost calculation helpers
- Date range exports

### Invoices (Complex Entity)

See [`frontend/i18n/locales/en.json`](../frontend/i18n/locales/en.json) - `invoices` namespace (lines ~750-820)

Key highlights:
- Multiple status labels
- Line items section
- SRI integration keys
- Status change workflow

---

## Tools and Validation

### VS Code i18n Extensions (Recommended)

- **i18n Ally**: Inline translation preview and management
- **Localization Editor**: Edit all locales side-by-side

### Translation Key Validation

Before committing:

1. **Check key exists in all locales**:
   ```bash
   # Search for key in all locale files
   grep -r "new_key" frontend/i18n/locales/
   ```

2. **Validate JSON syntax**:
   ```bash
   # Parse JSON to check for syntax errors
   jq empty frontend/i18n/locales/en.json
   ```

3. **Check for missing interpolations**:
   - If `en.json` has `{name}`, ensure all translations maintain `{name}`

---

## References

- **Warehouse Implementation**: [`docs/WAREHOUSE_IMPLEMENTATION_REFERENCE.md`](./WAREHOUSE_IMPLEMENTATION_REFERENCE.md)
- **Frontend Agent**: [`docs/frontend-agent.md`](./frontend-agent.md)
- **Locale Files**: [`frontend/i18n/locales/`](../frontend/i18n/locales/)

---

## Changelog

| Date | Change | Author |
|------|--------|--------|
| 2026-02-11 | Initial i18n standards document created | System |

---

**Remember**: When in doubt, reference the **Warehouse module** as the canonical example for i18n patterns. Always maintain consistency with established conventions!
