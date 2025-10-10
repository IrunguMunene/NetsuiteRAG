# Update task status in PROJECT_MASTER.md

param(
    [Parameter(Mandatory=$true)]
    [string]$TaskId,
    
    [ValidateSet('Started', 'In Progress', 'Complete', 'Blocked')]
    [string]$Status = 'Complete',
    
    [string]$Notes = ''
)

$masterFile = "PROJECT_MASTER.md"
$timestamp = Get-Date -Format "yyyy-MM-dd HH:mm"

if (-not (Test-Path $masterFile)) {
    Write-Host "ERROR: PROJECT_MASTER.md not found!" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "=== Updating Task Status ===" -ForegroundColor Cyan
Write-Host "Task: $TaskId" -ForegroundColor Yellow
Write-Host "Status: $Status" -ForegroundColor Yellow
Write-Host ""

$content = Get-Content $masterFile -Raw

# Update task checkbox if Complete
if ($Status -eq 'Complete') {
    $content = $content -replace "(\[ \] $TaskId)", "[x] $TaskId"
    Write-Host "✓ Marked $TaskId as complete" -ForegroundColor Green
}

# Add to session history
$historyEntry = "`n$timestamp : $TaskId - $Status"
if ($Notes) {
    $historyEntry += " - $Notes"
}

# Find the Session History section and add entry
if ($content -match '## Session History') {
    $content = $content -replace '(## Session History\s*\n)', "`$1$historyEntry`n"
} else {
    $content += "`n## Session History$historyEntry`n"
}

# Update last updated timestamp
$content = $content -replace '\*\*Last Updated:\*\* .*', "**Last Updated:** $timestamp"

# Save
$content | Out-File $masterFile -Encoding UTF8 -NoNewline

Write-Host "✓ Updated PROJECT_MASTER.md" -ForegroundColor Green
Write-Host ""