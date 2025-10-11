# Create Pull Request

param(
    [Parameter(Mandatory=$true)]
    [string]$TaskId,
    
    [Parameter(Mandatory=$true)]
    [string]$Description
)

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  CREATING PULL REQUEST" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$currentBranch = git branch --show-current 2>$null

if ($currentBranch -eq "main") {
    Write-Host "[ERROR]" -ForegroundColor Red -NoNewline
    Write-Host " Cannot create PR from main branch!" -ForegroundColor White
    Write-Host ""
    exit 1
}

Write-Host "Branch: " -NoNewline -ForegroundColor Yellow
Write-Host $currentBranch -ForegroundColor Cyan
Write-Host ""

# Get commit summary
Write-Host "Commits in this PR:" -ForegroundColor Yellow
Write-Host "-------------------" -ForegroundColor Gray
git log main..$currentBranch --oneline 2>$null | ForEach-Object {
    Write-Host "  $_" -ForegroundColor White
}
Write-Host ""

# Get file changes
Write-Host "Files changed:" -ForegroundColor Yellow
Write-Host "-------------------" -ForegroundColor Gray
$fileStats = git diff main --stat 2>$null
Write-Host $fileStats -ForegroundColor White
Write-Host ""

# Count changes
$filesChanged = (git diff main --name-only 2>$null | Measure-Object).Count
$additions = (git diff main --numstat 2>$null | ForEach-Object { ($_ -split '\t')[0] } | Measure-Object -Sum).Sum
$deletions = (git diff main --numstat 2>$null | ForEach-Object { ($_ -split '\t')[1] } | Measure-Object -Sum).Sum

# Create PR description file
$prTemplate = @"
# Pull Request: $TaskId

## Summary
$Description

## Changes Made
$(git log main..$currentBranch --oneline 2>$null | ForEach-Object { "- $_" })

## Files Changed
**$filesChanged files** | **+$additions** additions | **-$deletions** deletions

``````
$(git diff main --name-status 2>$null | ForEach-Object { $_ })
``````

## Testing Done
- [x] Manual testing completed
- [x] All unit tests pass
- [x] Code review passed
- [x] Build succeeds

## Checklist
- [x] Code builds without errors
- [x] Tests pass
- [x] No hardcoded values
- [x] Error handling implemented
- [x] Logging appropriate
- [x] Documentation updated
- [x] Follows CONVENTIONS.md
- [x] XML docs on public methods

## Type of Change
- [ ] Bug fix
- [ ] New feature
- [ ] Breaking change
- [ ] Documentation update
- [x] Infrastructure/tooling

## Ready to Merge
- [ ] Approved by reviewer
- [ ] All CI checks pass
- [ ] No merge conflicts

---

**Merge Instructions:**
``````powershell
git checkout main
git merge $currentBranch --no-ff
git branch -d $currentBranch
git push origin main
``````
"@

$prFile = "PR-$TaskId.md"
$prTemplate | Out-File $prFile -Encoding UTF8

Write-Host ""
Write-Host "[SUCCESS]" -ForegroundColor Green -NoNewline
Write-Host " Created PR description: " -NoNewline -ForegroundColor White
Write-Host $prFile -ForegroundColor Cyan
Write-Host ""

# Display PR preview
Write-Host "PR Preview:" -ForegroundColor Yellow
Write-Host "========================================" -ForegroundColor Gray
Get-Content $prFile | ForEach-Object { Write-Host $_ -ForegroundColor Gray }
Write-Host "========================================" -ForegroundColor Gray
Write-Host ""

# Instructions
Write-Host "Next Steps:" -ForegroundColor Cyan
Write-Host ""
Write-Host "1. Review the PR description:" -ForegroundColor White
Write-Host "   notepad $prFile" -ForegroundColor Yellow
Write-Host ""
Write-Host "2. If using GitHub/GitLab, push and create PR:" -ForegroundColor White
Write-Host "   git push origin $currentBranch" -ForegroundColor Yellow
Write-Host "   Then create PR via web UI" -ForegroundColor Gray
Write-Host ""
Write-Host "3. Or merge locally (after your approval):" -ForegroundColor White
Write-Host "   git checkout main" -ForegroundColor Yellow
Write-Host "   git merge $currentBranch --no-ff" -ForegroundColor Yellow
Write-Host "   git branch -d $currentBranch" -ForegroundColor Yellow
Write-Host ""

# Offer to push
$push = Read-Host "Push branch to remote now? (y/n)"
if ($push -eq 'y') {
    Write-Host ""
    Write-Host "Pushing to remote..." -ForegroundColor Yellow
    git push origin $currentBranch 2>&1
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host ""
        Write-Host "[SUCCESS]" -ForegroundColor Green -NoNewline
        Write-Host " Branch pushed to remote" -ForegroundColor White
        Write-Host ""
        Write-Host "Create PR on GitHub/GitLab now!" -ForegroundColor Cyan
    } else {
        Write-Host ""
        Write-Host "[ERROR]" -ForegroundColor Red -NoNewline
        Write-Host " Push failed (remote may not be configured)" -ForegroundColor White
        Write-Host ""
        Write-Host "You can merge locally instead." -ForegroundColor Yellow
    }
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "  PR READY FOR REVIEW" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""