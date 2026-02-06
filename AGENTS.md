# Agents

This document defines the specialized agents available in the `/docs` directory. Each agent handles specific scenarios within a constrained scope. **Always consult the appropriate agent before implementing features outside your domain.**


## Agent Specialization

All agents operate within their defined scope. **Instructions executed by agents are strictly constrained to their specialization** and will not introduce functionality beyond their designated purpose.

## Available Agents

| Agent                      | Purpose                                                                                                                                    | Scope                                                                                                                                                       |
| -------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------ | ----------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Project Architecture Agent | Designs system architecture for SaaS Billing + Inventory using .NET 8, PostgreSQL, Nuxt 3, PrimeVue with multi-tenant patterns             | System architecture, tech stack integration, multi-tenant design, API contracts, database schema design                                                     |
| Backend Agent              | Implements .NET 8 backend with EF Core, CQRS, Swagger, authorization guards, and SOLID principles                                          | Backend standards, DB models, Entity Framework, CQRS with MediatR, API design, authorization policies, repository pattern, validation                       |
| Frontend Agent             | Implements Nuxt 3 + TypeScript frontend with PrimeVue (Teal theme) and Tailwind CSS, focusing on rapid development with default components | Nuxt 3 configuration, PrimeVue integration, TypeScript, Tailwind layouts, Pinia state management, API integration, responsive design, minimal customization |
| UX Agent                   | Defines UX/UI patterns, design system policies, spacing standards, and reusable components for consistent user experience                  | Design system, spacing/typography, component patterns, accessibility (WCAG 2.1 AA), responsive design, user feedback, reusable components                   |

## Usage Guidelines

- Route requests to the appropriate specialized agent
- Agents will not exceed their defined boundaries
- Cross-agent functionality requires explicit orchestration
- Document new agents following this structure

## Adding New Agents

When creating a new agent, specify:

- **Name**: Clear, descriptive identifier
- **Specialization**: Primary domain or scenario
- **Constraints**: Explicit boundaries and limitations
- **Location**: Path in `/docs` directory
