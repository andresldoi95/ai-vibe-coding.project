# Controllers

This directory contains all HTTP request handlers (controllers).

## Structure

Controllers are organized by resource:
- `authController.ts` - Authentication (login, register, logout)
- `companiesController.ts` - Company/tenant management
- `usersController.ts` - User management
- `invoicesController.ts` - Invoice operations
- `productsController.ts` - Product catalog
- `inventoryController.ts` - Inventory management
- `customersController.ts` - Customer management
- `sriController.ts` - SRI integration endpoints

## Guidelines

- Keep controllers thin - delegate business logic to services
- Use express-validator for input validation
- Return consistent response formats
- Handle errors with next(error)
- Use async/await with try-catch or express-async-handler
