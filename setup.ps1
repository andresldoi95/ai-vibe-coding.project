# Setup script for Windows (PowerShell)
# Run this script to initialize the frontend project

Write-Host "🚀 SaaS Billing & Inventory - Frontend Setup" -ForegroundColor Cyan
Write-Host "=============================================" -ForegroundColor Cyan
Write-Host ""

# Check Node.js version
Write-Host "Checking Node.js version..." -ForegroundColor Yellow
$nodeVersion = node --version 2>$null
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Node.js is not installed!" -ForegroundColor Red
    Write-Host "Please install Node.js 20+ from https://nodejs.org/" -ForegroundColor Red
    exit 1
}

Write-Host "✅ Node.js version: $nodeVersion" -ForegroundColor Green

# Navigate to frontend directory
Set-Location -Path "frontend"

# Check if node_modules exists
if (Test-Path "node_modules") {
    Write-Host "📦 node_modules already exists" -ForegroundColor Yellow
    $response = Read-Host "Do you want to reinstall dependencies? (y/N)"
    if ($response -eq "y" -or $response -eq "Y") {
        Write-Host "Removing node_modules..." -ForegroundColor Yellow
        Remove-Item -Path "node_modules" -Recurse -Force
        Write-Host "Installing dependencies..." -ForegroundColor Yellow
        npm install
    }
} else {
    Write-Host "📦 Installing dependencies..." -ForegroundColor Yellow
    npm install
    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ Failed to install dependencies!" -ForegroundColor Red
        exit 1
    }
    Write-Host "✅ Dependencies installed successfully" -ForegroundColor Green
}

# Create .env file if it doesn't exist
if (-not (Test-Path ".env")) {
    Write-Host "📝 Creating .env file..." -ForegroundColor Yellow
    Copy-Item ".env.example" ".env"
    Write-Host "✅ .env file created" -ForegroundColor Green
} else {
    Write-Host "✅ .env file already exists" -ForegroundColor Green
}

Write-Host ""
Write-Host "🎉 Setup complete!" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "  1. Review and update the .env file if needed" -ForegroundColor White
Write-Host "  2. Run 'npm run dev' to start the development server" -ForegroundColor White
Write-Host "  3. Open http://localhost:3000 in your browser" -ForegroundColor White
Write-Host ""
Write-Host "Or use Docker:" -ForegroundColor Cyan
Write-Host "  cd .." -ForegroundColor White
Write-Host "  docker-compose up" -ForegroundColor White
Write-Host ""

# Ask if user wants to start dev server
$startDev = Read-Host "Start development server now? (y/N)"
if ($startDev -eq "y" -or $startDev -eq "Y") {
    Write-Host ""
    Write-Host "🚀 Starting development server..." -ForegroundColor Green
    Write-Host ""
    npm run dev
}
