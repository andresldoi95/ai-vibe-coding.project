# Multi-stage Dockerfile for Nuxt 3 application

# Stage 1: Dependencies
FROM node:20-alpine AS deps
WORKDIR /app

# Copy package files
COPY package.json package-lock.json* ./

# Install dependencies
RUN npm ci

# Stage 2: Builder
FROM node:20-alpine AS builder
WORKDIR /app

# Copy dependencies from deps stage
COPY --from=deps /app/node_modules ./node_modules

# Copy application code
COPY . .

# Build arguments for runtime config
ARG NUXT_PUBLIC_API_BASE
ENV NUXT_PUBLIC_API_BASE=$NUXT_PUBLIC_API_BASE

# Build the application
RUN npm run build

# Stage 3: Runner
FROM node:20-alpine AS runner
WORKDIR /app

# Set NODE_ENV to production
ENV NODE_ENV=production

# Copy built application from builder
COPY --from=builder /app/.output ./.output
COPY --from=builder /app/package.json ./package.json

# Create a non-root user
RUN addgroup --system --gid 1001 nodejs && \
    adduser --system --uid 1001 nuxtjs && \
    chown -R nuxtjs:nodejs /app

USER nuxtjs

# Expose port
EXPOSE 3000

# Set host to listen on all interfaces
ENV HOST=0.0.0.0
ENV PORT=3000

# Start the application
CMD ["node", ".output/server/index.mjs"]
