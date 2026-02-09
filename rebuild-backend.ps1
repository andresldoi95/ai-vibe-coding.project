# Stop and remove backend container
Write-Host "Stopping backend container..."
docker stop saas-backend | Out-Null
docker rm saas-backend | Out-Null

# Rebuild image
Write-Host "Rebuilding backend image..."
docker-compose build --no-cache backend

# Start backend
Write-Host "Starting backend container..."
docker-compose up -d backend

# Show logs
Write-Host "`nBackend logs:"
docker logs saas-backend --tail 20
