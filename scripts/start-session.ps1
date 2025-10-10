# Start a development session - checks environment and prepares

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  STARTING DEVELOPMENT SESSION" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Check environment
Write-Host "Checking environment..." -ForegroundColor Yellow
& ".\scripts\check-environment.ps1"

if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "Fix environment issues before starting!" -ForegroundColor Red
    exit 1
}

# Show current status
Write-Host ""
Write-Host "Current Status:" -ForegroundColor Cyan
$master = Get-Content "PROJECT_MASTER.md" -Raw
if ($master -match '\*\*Next Task:\*\* ([^\n]+)') {
    Write-Host "  Next Task: $($matches[1])" -ForegroundColor White
}

# Find latest session
$latestSession = Get-ChildItem sessions -Filter "session-*.md" | Sort-Object Name -Descending | Select-Object -First 1
if ($latestSession) {
    Write-Host "  Latest Session: $($latestSession.Name)" -ForegroundColor White
}

Write-Host ""
Write-Host "Ready to start Claude Code!" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "  1. Run: claude" -ForegroundColor White
Write-Host "  2. Give Claude the continuation instructions" -ForegroundColor White
Write-Host ""