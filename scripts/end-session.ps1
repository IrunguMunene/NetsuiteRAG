# End of session automation - saves progress and commits

param(
    [Parameter(Mandatory=$true)]
    [string]$SessionFile,
    
    [Parameter(Mandatory=$true)]
    [string]$Summary,
    
    [string]$TaskId = "",
    
    [ValidateSet('Complete', 'In Progress')]
    [string]$TaskStatus = 'In Progress'
)

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  END OF SESSION PROCEDURE" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Step 1: Update session file with progress
Write-Host "Step 1: Updating session file..." -ForegroundColor Yellow

$sessionContent = Get-Content $SessionFile -Raw
$timestamp = Get-Date -Format "yyyy-MM-dd HH:mm"

$progressNote = @"

---

## Session End Summary

**Date:** $timestamp
**Status:** $TaskStatus

### What Was Completed:
$Summary

### Files Created/Modified:
$(git status --short | Out-String)

### Next Steps:
[To be determined in next session]

---
"@

$sessionContent += $progressNote
$sessionContent | Out-File $SessionFile -Encoding UTF8

Write-Host "[SUCCESS] Updated $SessionFile" -ForegroundColor Green
Write-Host ""

# Step 2: Update PROJECT_MASTER if task provided
if ($TaskId) {
    Write-Host "Step 2: Updating PROJECT_MASTER.md..." -ForegroundColor Yellow
    & ".\scripts\update-task.ps1" -TaskId $TaskId -Status $TaskStatus -Notes "Session ended"
    Write-Host ""
}

# Step 3: Git add all changes
Write-Host "Step 3: Staging files for git..." -ForegroundColor Yellow
git add .
$stagedFiles = git diff --cached --name-only
Write-Host "Staged files:" -ForegroundColor Gray
$stagedFiles | ForEach-Object { Write-Host "  $_" -ForegroundColor Gray }
Write-Host ""

# Step 4: Git commit
Write-Host "Step 4: Committing to git..." -ForegroundColor Yellow
$commitMessage = if ($TaskId) { "$TaskId : $Summary" } else { "Session progress: $Summary" }
git commit -m $commitMessage

if ($LASTEXITCODE -eq 0) {
    Write-Host "[SUCCESS] Committed successfully" -ForegroundColor Green
} else {
    Write-Host "[INFO] Commit failed (maybe no changes?)" -ForegroundColor Yellow
}
Write-Host ""

# Step 5: Show summary
Write-Host "========================================" -ForegroundColor Green
Write-Host "  SESSION SAVED SUCCESSFULLY" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Summary:" -ForegroundColor Cyan
Write-Host "  Session file updated: $SessionFile" -ForegroundColor White
if ($TaskId) {
    Write-Host "  Task $TaskId marked as: $TaskStatus" -ForegroundColor White
}
Write-Host "  Changes committed to git" -ForegroundColor White
Write-Host ""
Write-Host "When you return:" -ForegroundColor Cyan
Write-Host "  1. Run: .\scripts\check-environment.ps1" -ForegroundColor White
Write-Host "  2. Run: claude" -ForegroundColor White
Write-Host "  3. Tell Claude: 'Read PROJECT_MASTER.md and $SessionFile, continue where we left off'" -ForegroundColor White
Write-Host ""