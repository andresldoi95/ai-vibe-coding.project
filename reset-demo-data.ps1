#!/usr/bin/env pwsh
# Reset Database and Seed Demo Data Script
# This script drops the database, recreates it, runs migrations, and seeds demo data

param(
    [switch]$Force
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Database Reset & Demo Data Seeder" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Confirm action
if (-not $Force) {
    Write-Host "WARNING: This will DELETE ALL DATA in the database!" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Demo credentials will be:" -ForegroundColor Green
    Write-Host "  Owner  : owner@demo.com  / password" -ForegroundColor White
    Write-Host "  Admin  : admin@demo.com  / password" -ForegroundColor White
    Write-Host "  Manager: manager@demo.com / password" -ForegroundColor White
    Write-Host "  User   : user@demo.com   / password" -ForegroundColor White
    Write-Host ""
    $confirm = Read-Host "Are you sure you want to continue? (yes/no)"

    if ($confirm -ne "yes") {
        Write-Host "Operation cancelled." -ForegroundColor Yellow
        exit 0
    }
}

Write-Host ""
Write-Host "Step 1: Stopping backend to close database connections..." -ForegroundColor Cyan
docker-compose stop backend

Write-Host ""
Write-Host "Step 2: Waiting for PostgreSQL to be ready..." -ForegroundColor Cyan
$dbReady = $false
$dbAttempts = 0
$maxDbAttempts = 30

while ($dbAttempts -lt $maxDbAttempts -and !$dbReady) {
    $dbAttempts++
    Write-Host "  Checking PostgreSQL availability (attempt $dbAttempts/$maxDbAttempts)..." -ForegroundColor Gray

    try {
        $result = docker-compose exec -T db psql -U postgres -c "SELECT 1" 2>&1
        if ($result -match "1 row") {
            $dbReady = $true
            Write-Host "  PostgreSQL is ready!" -ForegroundColor Green
        }
    }
    catch {
        Start-Sleep -Seconds 1
    }

    if (!$dbReady) {
        Start-Sleep -Seconds 1
    }
}

if (!$dbReady) {
    Write-Host ""
    Write-Host "ERROR: PostgreSQL did not become ready in time." -ForegroundColor Red
    Write-Host "Check logs with: docker logs saas-postgres" -ForegroundColor Yellow
    exit 1
}

Write-Host ""
Write-Host "Step 3: Terminating all database connections..." -ForegroundColor Cyan
docker-compose exec -T db psql -U postgres -c "SELECT pg_terminate_backend(pid) FROM pg_stat_activity WHERE datname = 'saas' AND pid <> pg_backend_pid();"

Write-Host ""
Write-Host "Step 4: Dropping existing database..." -ForegroundColor Cyan
docker-compose exec -T db psql -U postgres -c "DROP DATABASE IF EXISTS saas;"

Write-Host ""
Write-Host "Step 5: Creating fresh database..." -ForegroundColor Cyan
docker-compose exec -T db psql -U postgres -c "CREATE DATABASE saas OWNER postgres;"

Write-Host ""
Write-Host "Step 6: Starting backend and applying migrations..." -ForegroundColor Cyan
docker-compose up -d backend

Write-Host ""
Write-Host "Waiting for backend to start and apply migrations..." -ForegroundColor Cyan
Start-Sleep -Seconds 10

# Check if backend is healthy
$maxAttempts = 30
$attempt = 0
$healthy = $false

while ($attempt -lt $maxAttempts -and !$healthy) {
    $attempt++
    Write-Host "  Checking backend health (attempt $attempt/$maxAttempts)..." -ForegroundColor Gray

    try {
        $response = Invoke-WebRequest -Uri "http://localhost:5000/swagger/index.html" -Method Get -TimeoutSec 2 -ErrorAction SilentlyContinue -UseBasicParsing
        if ($response.StatusCode -eq 200) {
            $healthy = $true
            Write-Host "  Backend is healthy!" -ForegroundColor Green
        }
    }
    catch {
        Start-Sleep -Seconds 2
    }
}

if (!$healthy) {
    Write-Host ""
    Write-Host "ERROR: Backend did not become healthy in time." -ForegroundColor Red
    Write-Host "Check logs with: docker logs saas-backend" -ForegroundColor Yellow
    exit 1
}

Write-Host ""
Write-Host "Step 7: Seeding demo data..." -ForegroundColor Cyan
try {
    $response = Invoke-WebRequest -Uri "http://localhost:5000/api/seed/demo" -Method Post -TimeoutSec 30 -UseBasicParsing

    if ($response.StatusCode -eq 200) {
        Write-Host ""
        Write-Host "========================================" -ForegroundColor Green
        Write-Host "  Demo Data Seeded Successfully!" -ForegroundColor Green
        Write-Host "========================================" -ForegroundColor Green
        Write-Host ""
        Write-Host "Demo Company:" -ForegroundColor Cyan
        Write-Host "  Name: Demo Company" -ForegroundColor White
        Write-Host "  Slug: demo-company" -ForegroundColor White
        Write-Host ""
        Write-Host "Demo Users (all passwords are 'password'):" -ForegroundColor Cyan
        Write-Host "  Owner  : owner@demo.com" -ForegroundColor White
        Write-Host "  Admin  : admin@demo.com" -ForegroundColor White
        Write-Host "  Manager: manager@demo.com" -ForegroundColor White
        Write-Host "  User   : user@demo.com" -ForegroundColor White
        Write-Host ""
        Write-Host "Sample Data:" -ForegroundColor Cyan
        Write-Host "  - 3 Warehouses created" -ForegroundColor White
        Write-Host ""
        Write-Host "Login: http://localhost:3000/login" -ForegroundColor Green
        Write-Host ""
    }
}
catch {
    Write-Host ""
    Write-Host "ERROR: Failed to seed demo data." -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    Write-Host ""
    Write-Host "The database has been reset but demo data was not seeded." -ForegroundColor Yellow
    Write-Host "You can still register a new company manually at: http://localhost:3000/register" -ForegroundColor Yellow
    exit 1
}
