# Test Email Templates Script
# Sends test emails to verify MJML templates in Mailpit

Write-Host "Testing Email Templates..." -ForegroundColor Cyan
Write-Host ""

$baseUrl = "http://localhost:5000/api/v1/testemail"
$mailpitUrl = "http://localhost:8025"

# Test 1: Password Reset Email
Write-Host "1. Sending Password Reset Email..." -ForegroundColor Yellow
$passwordResetBody = @{
    email = "test@example.com"
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri "$baseUrl/send-password-reset" -Method Post -Body $passwordResetBody -ContentType "application/json"
    Write-Host "   SUCCESS: $($response.message)" -ForegroundColor Green
} catch {
    Write-Host "   ERROR: $($_.Exception.Message)" -ForegroundColor Red
}

Start-Sleep -Seconds 1

# Test 2: Welcome Email
Write-Host "2. Sending Welcome Email..." -ForegroundColor Yellow
$welcomeBody = @{
    email = "newuser@example.com"
    userName = "John Doe"
    tenantId = [Guid]::Empty.ToString()
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri "$baseUrl/send-welcome" -Method Post -Body $welcomeBody -ContentType "application/json"
    Write-Host "   SUCCESS: $($response.message)" -ForegroundColor Green
} catch {
    Write-Host "   ERROR: $($_.Exception.Message)" -ForegroundColor Red
}

Start-Sleep -Seconds 1

# Test 3: User Invitation Email
Write-Host "3. Sending User Invitation Email..." -ForegroundColor Yellow
$invitationBody = @{
    email = "invited@example.com"
    companyName = "Acme Corporation"
    tenantId = [Guid]::Empty.ToString()
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri "$baseUrl/send-invitation" -Method Post -Body $invitationBody -ContentType "application/json"
    Write-Host "   SUCCESS: $($response.message)" -ForegroundColor Green
} catch {
    Write-Host "   ERROR: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "All test emails sent!" -ForegroundColor Green
Write-Host "Open Mailpit to view: $mailpitUrl" -ForegroundColor Cyan
Write-Host ""

# Offer to open Mailpit in browser
$openBrowser = Read-Host "Open Mailpit in browser? (Y/N)"
if ($openBrowser -eq "Y" -or $openBrowser -eq "y") {
    Start-Process $mailpitUrl
}
