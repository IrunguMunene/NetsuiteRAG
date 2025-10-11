# Comprehensive code review script

param(
    [Parameter(Mandatory=$true)]
    [string]$TaskId
)

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  CODE REVIEW: $TaskId" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$allPassed = $true

# Check 1: Build
Write-Host "1. Building solution..." -ForegroundColor Yellow
$buildOutput = dotnet build --no-restore 2>&1
if ($LASTEXITCODE -eq 0) {
    Write-Host "   " -NoNewline
    Write-Host "[PASS]" -ForegroundColor Green -NoNewline
    Write-Host " Build successful" -ForegroundColor White
} else {
    Write-Host "   " -NoNewline
    Write-Host "[FAIL]" -ForegroundColor Red -NoNewline
    Write-Host " Build failed" -ForegroundColor White
    Write-Host ""
    Write-Host "Build Output:" -ForegroundColor Red
    Write-Host $buildOutput -ForegroundColor Gray
    $allPassed = $false
}

# Check 2: Tests
Write-Host ""
Write-Host "2. Running tests..." -ForegroundColor Yellow
if (Test-Path "tests") {
    $testOutput = dotnet test --no-build 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "   " -NoNewline
        Write-Host "[PASS]" -ForegroundColor Green -NoNewline
        Write-Host " All tests passed" -ForegroundColor White
    } else {
        Write-Host "   " -NoNewline
        Write-Host "[FAIL]" -ForegroundColor Red -NoNewline
        Write-Host " Tests failed" -ForegroundColor White
        Write-Host ""
        Write-Host "Test Output:" -ForegroundColor Red
        Write-Host $testOutput -ForegroundColor Gray
        $allPassed = $false
    }
} else {
    Write-Host "   " -NoNewline
    Write-Host "[WARN]" -ForegroundColor Yellow -NoNewline
    Write-Host " No tests found" -ForegroundColor White
}

# Check 3: Code quality checks
Write-Host ""
Write-Host "3. Code quality checks..." -ForegroundColor Yellow

# Find all .cs files changed
$changedFiles = git diff main --name-only --diff-filter=ACMR 2>$null | Where-Object { $_ -like "*.cs" }

if ($changedFiles) {
    $issues = @()
    
    foreach ($file in $changedFiles) {
        if (Test-Path $file) {
            $content = Get-Content $file -Raw
            
            # Check for commented code
            if ($content -match '//\s*(public|private|protected|internal|class|void)') {
                $issues += "  - $file : Contains commented-out code"
            }
            
            # Check for TODO
            if ($content -match '//\s*TODO') {
                $issues += "  - $file : Contains unresolved TODO"
            }
            
            # Check for Console.WriteLine (should use ILogger)
            if ($content -match 'Console\.WriteLine') {
                $issues += "  - $file : Uses Console.WriteLine (use ILogger instead)"
            }
            
            # Check for hardcoded strings that look like config
            if ($content -match '"(http://|https://|Server=|Password=|ConnectionString)') {
                $issues += "  - $file : Possible hardcoded connection string/URL"
            }
            
            # Check for empty catch blocks
            if ($content -match 'catch[^{]*\{\s*\}') {
                $issues += "  - $file : Empty catch block found"
            }
        }
    }
    
    if ($issues.Count -eq 0) {
        Write-Host "   " -NoNewline
        Write-Host "[PASS]" -ForegroundColor Green -NoNewline
        Write-Host " No code quality issues found" -ForegroundColor White
    } else {
        Write-Host "   " -NoNewline
        Write-Host "[FAIL]" -ForegroundColor Red -NoNewline
        Write-Host " Code quality issues found:" -ForegroundColor White
        $issues | ForEach-Object { Write-Host $_ -ForegroundColor Red }
        $allPassed = $false
    }
} else {
    Write-Host "   " -NoNewline
    Write-Host "[INFO]" -ForegroundColor Cyan -NoNewline
    Write-Host " No C# files changed" -ForegroundColor White
}

# Check 4: XML documentation
Write-Host ""
Write-Host "4. XML documentation check..." -ForegroundColor Yellow
$missingDocs = @()

if ($changedFiles) {
    foreach ($file in $changedFiles) {
        if (Test-Path $file) {
            $content = Get-Content $file
            
            # Find public methods without XML docs
            for ($i = 0; $i -lt $content.Count; $i++) {
                if ($content[$i] -match 'public\s+(\w+\??)\s+(\w+)\s*\(') {
                    $methodName = $matches[2]
                    # Check if previous line(s) have ///
                    $hasDoc = $false
                    for ($j = $i - 1; $j -ge [Math]::Max(0, $i - 5); $j--) {
                        if ($content[$j] -match '///') {
                            $hasDoc = $true
                            break
                        }
                        if ($content[$j] -match '\S' -and $content[$j] -notmatch '^\s*\[') {
                            break
                        }
                    }
                    
                    if (-not $hasDoc -and $methodName -ne 'Main') {
                        $missingDocs += "  - $file : Method '$methodName' missing XML documentation"
                    }
                }
            }
        }
    }
}

if ($missingDocs.Count -eq 0) {
    Write-Host "   " -NoNewline
    Write-Host "[PASS]" -ForegroundColor Green -NoNewline
    Write-Host " All public methods documented" -ForegroundColor White
} else {
    Write-Host "   " -NoNewline
    Write-Host "[WARN]" -ForegroundColor Yellow -NoNewline
    Write-Host " Missing documentation (not blocking):" -ForegroundColor White
    $missingDocs | Select-Object -First 10 | ForEach-Object { Write-Host $_ -ForegroundColor Yellow }
    if ($missingDocs.Count -gt 10) {
        Write-Host "  ... and $($missingDocs.Count - 10) more" -ForegroundColor Yellow
    }
}

# Check 5: Git status
Write-Host ""
Write-Host "5. Git status..." -ForegroundColor Yellow
$gitStatus = git status --short 2>$null
if ($gitStatus) {
    Write-Host "   " -NoNewline
    Write-Host "[FAIL]" -ForegroundColor Red -NoNewline
    Write-Host " Uncommitted changes found:" -ForegroundColor White
    git status --short | ForEach-Object { Write-Host "     $_" -ForegroundColor Yellow }
    Write-Host ""
    Write-Host "   Commit these before creating PR" -ForegroundColor Yellow
    $allPassed = $false
} else {
    Write-Host "   " -NoNewline
    Write-Host "[PASS]" -ForegroundColor Green -NoNewline
    Write-Host " All changes committed" -ForegroundColor White
}

# Check 6: Branch check
Write-Host ""
Write-Host "6. Branch verification..." -ForegroundColor Yellow
$currentBranch = git branch --show-current 2>$null
if ($currentBranch -eq "main") {
    Write-Host "   " -NoNewline
    Write-Host "[FAIL]" -ForegroundColor Red -NoNewline
    Write-Host " Still on main branch!" -ForegroundColor White
    $allPassed = $false
} elseif ($currentBranch -like "feature/$TaskId*") {
    Write-Host "   " -NoNewline
    Write-Host "[PASS]" -ForegroundColor Green -NoNewline
    Write-Host " On correct feature branch: $currentBranch" -ForegroundColor White
} else {
    Write-Host "   " -NoNewline
    Write-Host "[WARN]" -ForegroundColor Yellow -NoNewline
    Write-Host " On branch: $currentBranch" -ForegroundColor White
    Write-Host "   Expected: feature/$TaskId-*" -ForegroundColor Yellow
}

# Summary
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
if ($allPassed) {
    Write-Host "  CODE REVIEW: PASSED" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "All checks passed! Ready to create Pull Request." -ForegroundColor Green
    Write-Host ""
    exit 0
} else {
    Write-Host "  CODE REVIEW: FAILED" -ForegroundColor Red
    Write-Host "========================================" -ForegroundColor Red
    Write-Host ""
    Write-Host "Please fix the issues above before creating PR." -ForegroundColor Yellow
    Write-Host ""
    exit 1
}