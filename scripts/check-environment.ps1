# Check that all required services are running

Write-Host ""
Write-Host "=== NetSuite RAG Environment Check ===" -ForegroundColor Cyan
Write-Host "$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')" -ForegroundColor Gray
Write-Host ""

$allGood = $true

function Test-ServicePort {
    param(
        [string]$Service,
        [int]$Port
    )
    
    try {
        $test = Test-NetConnection -ComputerName localhost -Port $Port -WarningAction SilentlyContinue -ErrorAction SilentlyContinue
        if ($test.TcpTestSucceeded) {
            Write-Host "  " -NoNewline
            Write-Host "[OK]" -ForegroundColor Green -NoNewline
            Write-Host " $Service (localhost:$Port)" -ForegroundColor White
            return $true
        }
        else {
            Write-Host "  " -NoNewline
            Write-Host "[FAIL]" -ForegroundColor Red -NoNewline
            Write-Host " $Service (localhost:$Port) - NOT RESPONDING" -ForegroundColor White
            return $false
        }
    }
    catch {
        Write-Host "  " -NoNewline
        Write-Host "[FAIL]" -ForegroundColor Red -NoNewline
        Write-Host " $Service (localhost:$Port) - ERROR" -ForegroundColor White
        return $false
    }
}

Write-Host "Checking Services:" -ForegroundColor Yellow
Write-Host ""

$pgOk = Test-ServicePort -Service "PostgreSQL" -Port 5432
$redisOk = Test-ServicePort -Service "Redis" -Port 6379
$qdrantOk = Test-ServicePort -Service "Qdrant" -Port 6333
$ollamaOk = Test-ServicePort -Service "Ollama" -Port 11434

$allGood = $pgOk -and $redisOk -and $qdrantOk -and $ollamaOk

Write-Host ""
Write-Host "Checking Tools:" -ForegroundColor Yellow
Write-Host ""

try {
    $dotnetVersion = dotnet --version
    Write-Host "  " -NoNewline
    Write-Host "[OK]" -ForegroundColor Green -NoNewline
    Write-Host " .NET SDK ($dotnetVersion)" -ForegroundColor White
}
catch {
    Write-Host "  " -NoNewline
    Write-Host "[FAIL]" -ForegroundColor Red -NoNewline
    Write-Host " .NET SDK - NOT INSTALLED" -ForegroundColor White
    $allGood = $false
}

try {
    $nodeVersion = node --version
    Write-Host "  " -NoNewline
    Write-Host "[OK]" -ForegroundColor Green -NoNewline
    Write-Host " Node.js ($nodeVersion)" -ForegroundColor White
}
catch {
    Write-Host "  " -NoNewline
    Write-Host "[FAIL]" -ForegroundColor Red -NoNewline
    Write-Host " Node.js - NOT INSTALLED" -ForegroundColor White
    $allGood = $false
}

try {
    $gitVersion = git --version
    Write-Host "  " -NoNewline
    Write-Host "[OK]" -ForegroundColor Green -NoNewline
    Write-Host " Git ($gitVersion)" -ForegroundColor White
}
catch {
    Write-Host "  " -NoNewline
    Write-Host "[FAIL]" -ForegroundColor Red -NoNewline
    Write-Host " Git - NOT INSTALLED" -ForegroundColor White
    $allGood = $false
}

Write-Host ""

if ($allGood) {
    Write-Host "======================================" -ForegroundColor Green
    Write-Host "  ALL SYSTEMS READY!" -ForegroundColor Green
    Write-Host "======================================" -ForegroundColor Green
}
else {
    Write-Host "======================================" -ForegroundColor Red
    Write-Host "  ISSUES DETECTED" -ForegroundColor Red
    Write-Host "======================================" -ForegroundColor Red
    Write-Host ""
    Write-Host "Please start any missing services before development." -ForegroundColor Yellow
}

Write-Host ""