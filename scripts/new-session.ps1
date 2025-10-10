# Create a new session file for the next task

param(
    [Parameter(Mandatory=$true)]
    [string]$TaskId,
    
    [Parameter(Mandatory=$true)]
    [string]$Description,
    
    [string]$Phase = "Phase 0"
)

$sessionNumber = (Get-ChildItem sessions -Filter "session-*.md" | Measure-Object).Count + 1
$sessionNumber = $sessionNumber.ToString("000")
$filename = "sessions\session-$sessionNumber-$TaskId-$($Description -replace ' ','-' -replace '[^a-zA-Z0-9-]','').md"

Write-Host ""
Write-Host "=== Creating New Session File ===" -ForegroundColor Cyan
Write-Host "Session: $sessionNumber" -ForegroundColor Yellow
Write-Host "Task: $TaskId" -ForegroundColor Yellow
Write-Host "File: $filename" -ForegroundColor Yellow
Write-Host ""

# Get last session summary
$lastSession = Get-ChildItem sessions -Filter "session-*.md" | Sort-Object Name -Descending | Select-Object -First 1
$lastSessionSummary = "First session"
if ($lastSession) {
    $lastSessionSummary = "See: $($lastSession.Name)"
}

# Create session content from template
$template = Get-Content "session-templates\feature-session-template.md" -Raw

# Replace placeholders
$template = $template -replace '\[TASK_DESCRIPTION\]', $Description
$template = $template -replace '\[TODAY''S_DATE\]', (Get-Date -Format "yyyy-MM-dd")
$template = $template -replace '\[TASK_ID\]', $TaskId
$template = $template -replace '\[PHASE_NUMBER\]', $Phase
$template = $template -replace '\[SUMMARY_OF_LAST_SESSION or "This is the first session"\]', $lastSessionSummary
$template = $template -replace '\[WHAT_NEEDS_TO_BE_DONE\]', $Description

# Save
$template | Out-File $filename -Encoding UTF8

Write-Host "✓ Created: $filename" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "1. Edit the file to add specific task details" -ForegroundColor White
Write-Host "2. Start Claude Code and reference this session file" -ForegroundColor White
Write-Host ""

return $filename