FROM node:20-alpine

# Install wget for healthcheck
RUN apk add --no-cache wget

WORKDIR /app

# Copy package files
COPY package*.json ./

# Install dependencies
RUN npm install

# Copy source code
COPY . .

# Create necessary directories
RUN mkdir -p logs storage/xml storage/pdf storage/certificates

# Expose port
EXPOSE 3000

# Start application (development mode)
CMD ["npm", "run", "dev"]
