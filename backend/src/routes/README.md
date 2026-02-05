# Routes

This directory contains Express route definitions.

## Structure

Routes map HTTP endpoints to controllers:
- `authRoutes.ts` - /api/v1/auth/*
- `companiesRoutes.ts` - /api/v1/companies/*
- `usersRoutes.ts` - /api/v1/users/*
- `invoicesRoutes.ts` - /api/v1/invoices/*
- `productsRoutes.ts` - /api/v1/products/*
- `inventoryRoutes.ts` - /api/v1/inventory/*
- `customersRoutes.ts` - /api/v1/customers/*
- `reportsRoutes.ts` - /api/v1/reports/*
- `sriRoutes.ts` - /api/v1/sri/*

## Guidelines

- Use Express Router
- Apply middleware at route level (auth, validation)
- Group related endpoints
- Use HTTP methods correctly (GET, POST, PUT, PATCH, DELETE)
- Include validation middleware before controller
- Document routes with comments
