# Utils

This directory contains utility functions and helpers.

## Structure

Reusable utility functions:
- `validation.ts` - Common validation functions
- `encryption.ts` - Password hashing, encryption utilities
- `jwt.ts` - JWT token generation and verification
- `sriUtils.ts` - SRI access key generation, XML utilities
- `dateUtils.ts` - Date formatting and manipulation
- `numberUtils.ts` - Number formatting (currency, decimals)
- `asyncHandler.ts` - Async error handler wrapper
- `responseFormatter.ts` - Standardized API responses

## Guidelines

- Keep functions pure when possible
- Export individual functions
- Include comprehensive JSDoc comments
- Write unit tests for utilities
- Handle edge cases
