# Agents

This document defines the specialized agents available for the SaaS Billing + Inventory Management System. Each agent handles specific scenarios within a constrained scope.

## Agent Structure

Agents are organized in two locations:

1. **`.github/agents/`** - GitHub Copilot agent definitions (`.agent.md` files with YAML frontmatter)
   - Concise prompts optimized for GitHub Copilot
   - Configured with appropriate tools and model settings
   - Quick reference for agent capabilities

2. **`docs/`** - Detailed agent instructions and reference documentation
   - Comprehensive implementation guidelines
   - Code examples and patterns
   - Technical specifications and best practices

**Always consult the appropriate agent before implementing features outside your domain.**

## Benefits of This Structure

### ✅ Optimized for GitHub Copilot
- Agent files in `.github/agents/` are automatically discovered by GitHub Copilot
- YAML frontmatter enables proper configuration (model, tools, description)
- Concise prompts reduce context size and improve response quality

### ✅ Separation of Concerns
- **Agent files**: Focused, actionable prompts for immediate use
- **Documentation files**: Comprehensive reference for deep dives
- Developers can choose appropriate level of detail

### ✅ Improved Discoverability
- All agents visible in GitHub Copilot interface
- Clear naming convention (Agent Name.agent.md)
- Centralized catalog in AGENTS.md

### ✅ Better Performance
- Smaller agent prompts = faster agent initialization
- Reduced token usage per interaction
- Detailed docs loaded only when needed

### ✅ Maintainability
- Single source of truth for each agent
- Easy to update without breaking references
- Clear separation between prompt and documentation

## Code Generation Guidelines

Before generating any code, **always**:
1. **Review common mistakes** (`docs/COMMON_MISTAKES_AND_PATTERNS.md`) to avoid recurring errors
2. Review relevant `/docs` files to understand established patterns and guidelines
3. Consult the appropriate specialized agent for your domain
4. Reference the canonical implementation examples (e.g., `WAREHOUSE_IMPLEMENTATION_REFERENCE.md`)
5. **Check i18n standards** (`docs/i18n-standards.md`) before adding any translation keys
6. **Verify component existence** before using them (search for component files first)
7. **Update seed data** in `backend/src/Api/Controllers/SeedController.cs` after adding new entities or migrations
8. **Run `reset-demo-data.ps1`** after database schema changes to verify demo data compatibility
9. Ensure your implementation aligns with project standards and conventions
10. Never generate code without reviewing the documentation first



## Agent Specialization

All agents operate within their defined scope. **Instructions executed by agents are strictly constrained to their specialization** and will not introduce functionality beyond their designated purpose.

## Available Agents

| Agent                         | Agent File | Documentation | Purpose                                                                                                                                    |
| ----------------------------- | ---------- | ------------- | ------------------------------------------------------------------------------------------------------------------------------------------ |
| Project Architecture Agent    | [.github/agents/](/.github/agents/Project%20Architecture%20Agent.agent.md) | [docs/](docs/project-architecture-agent.md) | Designs system architecture for SaaS Billing + Inventory using .NET 8, PostgreSQL, Nuxt 3, PrimeVue with multi-tenant patterns |
| Backend Agent                 | [.github/agents/](/.github/agents/Backend%20Agent.agent.md) | [docs/](docs/backend-agent.md) | Implements .NET 8 backend with EF Core, CQRS, Swagger, authorization guards, and SOLID principles |
| Backend Unit Testing Agent    | [.github/agents/](/.github/agents/Backend%20Unit%20Testing%20Agent.agent.md) | [docs/](docs/backend-unit-testing-agent.md) | Implements comprehensive unit tests for backend using xUnit, FluentAssertions, and Moq following Warehouse test patterns |
| Frontend Agent                | [.github/agents/](/.github/agents/Frontend%20Agent.agent.md) | [docs/](docs/frontend-agent.md) | Implements Nuxt 3 + TypeScript frontend with PrimeVue (Teal theme) and Tailwind CSS, focusing on rapid development with default components |
| UX Agent                      | [.github/agents/](/.github/agents/UX%20Agent.agent.md) | [docs/](docs/ux-agent.md) | Defines UX/UI patterns, design system policies, spacing standards, and reusable components for consistent user experience |
| Auth Agent                    | [.github/agents/](/.github/agents/Auth%20Agent.agent.md) | [docs/](docs/auth-agent.md) | Handles authentication, authorization, role-based access control (RBAC), and multi-tenant security with user-company associations |
| Email Agent                   | [.github/agents/](/.github/agents/Email%20Agent.agent.md) | [docs/](docs/email-agent.md) | Implements email service with Mailpit testing, transactional emails, templates, and SMTP configuration with MailKit |

### Legacy Agent

| Agent                         | Agent File | Purpose                                                                                                                                    |
| ----------------------------- | ---------- | ------------------------------------------------------------------------------------------------------------------------------------------ |
| Billing Project Agent         | [.github/agents/](/.github/agents/Billing%20Project%20Agent.agent.md) | Agent generator for Billing and Inventory Project System (meta-agent for creating other agents) |

## Usage Guidelines

### For GitHub Copilot Users

1. **Invoke agents via GitHub Copilot**: Use `@agent-name` to invoke specialized agents
2. **Agents are discoverable**: All agents in `.github/agents/` are auto-discovered
3. **Consult agent documentation**: Reference `docs/` for detailed implementation guidelines
4. **Follow agent constraints**: Each agent has defined boundaries and specializations

### For Manual Development

1. **Route requests to appropriate agent**: Check agent table for specialization
2. **Review detailed documentation**: Read corresponding `docs/*.md` file
3. **Follow reference implementations**: Use `WAREHOUSE_IMPLEMENTATION_REFERENCE.md` as template
4. **Maintain agent boundaries**: Don't exceed defined scope

### Agent Workflow

```
User Request → Select Agent → Review Agent File (.github/agents/)
                ↓
        Read Documentation (docs/)
                ↓
        Follow Patterns & Standards
                ↓
        Implement Feature
```

### Cross-Agent Coordination

- Agents will not exceed their defined boundaries
- Cross-agent functionality requires explicit orchestration
- Example: Frontend Agent + Backend Agent for full-stack feature
- Example: UX Agent defines patterns → Frontend Agent implements

## Reference Documentation

### Common Mistakes and Patterns Guide

The Common Mistakes document catalogs **recurring errors and establishes patterns** to prevent them in future development. This is the first document to consult before implementing any feature.

- ✅ **Form Development Patterns**: Standard form field structure using PrimeVue components
- ✅ **i18n Special Characters**: Proper escaping for `@`, `{`, and other special characters
- ✅ **Component Verification**: How to check component existence before use
- ✅ **Validation Patterns**: Complete Vuelidate rules for all form fields
- ✅ **Pre-Implementation Checklist**: Step-by-step verification before coding
- ✅ **Lessons Learned**: Historical mistakes and how to avoid them

**Location**: [`docs/COMMON_MISTAKES_AND_PATTERNS.md`](docs/COMMON_MISTAKES_AND_PATTERNS.md)

**Always consult this before**:
- Implementing any new form or CRUD feature
- Using components you're not familiar with
- Adding i18n translations
- Starting any feature without a reference implementation

### Warehouse Module (Complete Implementation Reference)

The Warehouse module serves as the **canonical reference implementation** for all CRUD features in this project. It demonstrates:

- ✅ **Backend**: Entity, CQRS commands/queries, repository, EF configuration, controller, migration
- ✅ **Frontend**: TypeScript types, composable, list/create/edit/view pages, i18n translations
- ✅ **UX Patterns**: DataTable, multi-section forms, breadcrumbs, status badges, confirmations, toasts

**Location**: [`docs/WAREHOUSE_IMPLEMENTATION_REFERENCE.md`](docs/WAREHOUSE_IMPLEMENTATION_REFERENCE.md)

**Use this as a template when implementing**:
- New inventory entities (Products, Stock Movements, Suppliers)
- Billing entities (Invoices, Customers, Payments)
- Any CRUD module across the application

### i18n Standards (Translation & Localization Reference)

The i18n standards document defines **naming conventions, patterns, and best practices** for all translations in the project. It prevents common locale mistakes and ensures consistency.

- ✅ **Naming Conventions**: snake_case keys, camelCase namespaces, consistent patterns
- ✅ **CRUD Patterns**: Standard structure for all CRUD modules (based on Warehouse)
- ✅ **Message Patterns**: Success, error, confirmation, and validation messages
- ✅ **Form Field Patterns**: Labels, placeholders, helpers, hints
- ✅ **Common Keys**: Reusable translations from `common` namespace
- ✅ **Interpolation**: Variable substitution guidelines
- ✅ **Common Mistakes**: What to avoid and how to fix

**Location**: [`docs/i18n-standards.md`](docs/i18n-standards.md)

**Always consult this before**:
- Adding any new translation keys
- Creating form labels or validation messages
- Implementing new CRUD pages
- Writing success/error messages
- Working with locale files (`frontend/i18n/locales/*.json`)

## Implementation Status

### Completed Modules
- ✅ **Authentication**: User registration, login, JWT tokens, refresh tokens
- ✅ **Multi-tenancy**: Tenant management, schema-per-tenant isolation
- ✅ **Inventory - Warehouses**: Full CRUD with reference implementation (see above)
- ✅ **Backend Unit Tests**: Comprehensive test coverage for Warehouses feature (73 tests: 14 Domain + 59 Application)

## Adding New Agents

When creating a new agent:

### 1. Create Agent File (`.github/agents/Agent Name.agent.md`)

Required frontmatter:
```yaml
---
name: Agent Name
description: Brief description of agent purpose
argument-hint: How to use this agent and when
model: Claude Sonnet 4.5 (copilot)
tools: ['read', 'edit', 'search', 'web', 'bash']
---
```

Agent content should include:
- **Role**: Clear definition of agent's expertise
- **Responsibilities**: Core tasks the agent handles
- **Standards**: Implementation patterns and constraints
- **Reference**: Link to detailed documentation in `docs/`
- **Usage**: When to invoke this agent

### 2. Create Documentation File (`docs/agent-name.md`)

Detailed documentation should include:
- **Specialization**: Domain expertise
- **Tech Stack**: Technologies and tools
- **Implementation Standards**: Code patterns, examples
- **Best Practices**: Guidelines and conventions
- **Examples**: Comprehensive code samples
- **Constraints**: Boundaries and limitations

### 3. Update AGENTS.md

Add entry to the agent table with:
- Agent name
- Link to `.github/agents/` file
- Link to `docs/` documentation
- Brief purpose description

### Template Files

- **Agent File Template**: See existing agents in `.github/agents/`
- **Documentation Template**: See existing docs in `docs/`
