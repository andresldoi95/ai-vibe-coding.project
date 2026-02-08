# Email Template Compilation Script
# Compiles MJML templates to HTML

Write-Host "Compiling MJML Email Templates..." -ForegroundColor Cyan

# Check if Node.js is installed
if (-not (Get-Command node -ErrorAction SilentlyContinue)) {
    Write-Host "ERROR: Node.js is not installed. Please install Node.js first." -ForegroundColor Red
    exit 1
}

# Navigate to email templates directory
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $scriptPath

# Check if node_modules exists, if not install dependencies
if (-not (Test-Path "node_modules")) {
    Write-Host "Installing MJML dependencies..." -ForegroundColor Yellow
    npm install
}

# Compile MJML templates
Write-Host "Compiling templates from src/ to dist/..." -ForegroundColor Yellow
npm run build

# Check if compilation was successful
if ($LASTEXITCODE -eq 0) {
    Write-Host "Email templates compiled successfully!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Compiled templates:" -ForegroundColor Cyan
    Get-ChildItem dist/*.html | ForEach-Object { Write-Host "  - $($_.Name)" -ForegroundColor White }
} else {
    Write-Host "ERROR: Compilation failed. Please check MJML syntax." -ForegroundColor Red
    exit 1
}
