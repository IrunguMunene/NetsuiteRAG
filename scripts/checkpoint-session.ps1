# Checkpoint current session state

param(
    [Parameter(Mandatory=$true)]
    [string]$TaskId,
    
    [Parameter(Mandatory=$true)]
    [string]$CurrentStep,
    
    [Parameter(Mandatory=$true)]
    [string]$Status,
    
    [string]$Notes = ""
)

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  CHECKPOINT: $TaskId" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
$branch = git branch --show-current 2>$null

# Find or create session file
$sessionFiles = Get-ChildItem "sessions" -Filter "*$TaskId*.md" | Sort-Object LastWriteTime -Descending
if ($sessionFiles) {
    $sessionFile = $sessionFiles[0].FullName
} else {
    Write-Host "[ERROR] No session file found for $TaskId" -ForegroundColor Red
    exit 1
}

Write-Host "Session file: $sessionFile" -ForegroundColor Yellow
Write-Host "Current step: $CurrentStep" -ForegroundColor Yellow
Write-Host "Status: $Status" -ForegroundColor Yellow
Write-Host ""

# Create checkpoint marker
$checkpointMarker = @"

---

## CHECKPOINT: $timestamp

**Step:** $CurrentStep  
**Status:** $Status  
**Branch:** $branch  
**Notes:** $Notes

### To Resume:
1. Ensure you're on branch: ``$branch``
2. Read this session file to see progress
3. Continue from: $CurrentStep

### What Was Completed:
[Claude: List what was done before this checkpoint]

### What's Next:
[Claude: List remaining steps]

---

"@

# Append checkpoint to session file
Add-Content -Path $sessionFile -Value $checkpointMarker

Write-Host "✓ Checkpoint saved to session file" -ForegroundColor Green

# Commit current state
$uncommitted = git status --short
if ($uncommitted) {
    git add .
    git commit -m "[$TaskId] Checkpoint: $CurrentStep - $Status"
    Write-Host "✓ Changes committed to git" -ForegroundColor Green
} else {
    Write-Host "✓ No uncommitted changes" -ForegroundColor Green
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "  CHECKPOINT COMPLETE" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Resume with:" -ForegroundColor Yellow
Write-Host "  .\scripts\start-session.ps1" -ForegroundColor Cyan
Write-Host "  claude" -ForegroundColor Cyan
Write-Host "  'Read .clinerules and resume from checkpoint'" -ForegroundColor Cyan
Write-Host ""