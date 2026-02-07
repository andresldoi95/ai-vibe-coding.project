# Build the backend container
Write-Host "Building backend Docker image..." -ForegroundColor Green
docker-compose build backend

# Start database if not running
Write-Host "Starting database..." -ForegroundColor Green
docker-compose up -d db

# Wait for database to be ready
Write-Host "Waiting for database to be ready..." -ForegroundColor Yellow
Start-Sleep -Seconds 10

# Run migrations inside the container
Write-Host "Running database migrations..." -ForegroundColor Green
docker-compose run --rm backend dotnet ef database update --project /src/src/Infrastructure --startup-project /src/src/Api

# Start all services
Write-Host "Starting all services..." -ForegroundColor Green
docker-compose up -d

Write-Host "`nBackend API is running at http://localhost:5000" -ForegroundColor Cyan
Write-Host "Swagger UI: http://localhost:5000/swagger" -ForegroundColor Cyan
Write-Host "`nView logs: docker logs -f saas-backend" -ForegroundColor Yellow
