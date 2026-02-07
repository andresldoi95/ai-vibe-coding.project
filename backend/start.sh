#!/bin/bash

# Build the backend container
echo "Building backend Docker image..."
docker-compose build backend

# Start database if not running
echo "Starting database..."
docker-compose up -d db

# Wait for database to be ready
echo "Waiting for database to be ready..."
sleep 10

# Run migrations inside the container
echo "Running database migrations..."
docker-compose run --rm backend dotnet ef database update --project /src/src/Infrastructure --startup-project /src/src/Api

# Start all services
echo "Starting all services..."
docker-compose up -d

echo ""
echo "Backend API is running at http://localhost:5000"
echo "Swagger UI: http://localhost:5000/swagger"
echo ""
echo "View logs: docker logs -f saas-backend"
