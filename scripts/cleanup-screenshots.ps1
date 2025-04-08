# Cleanup Screenshots Script
# This script cleans up the E2E test screenshots to keep only the most recent and relevant ones

# Configuration
$screenshotsDir = "e2e/screenshots"
$maxDaysToKeep = 7  # Keep screenshots newer than this many days
$maxScreenshotsPerPrefix = 3  # Keep this many screenshots per prefix (e.g., "radio-tuner")
$maxTotalScreenshots = 50  # Maximum total screenshots to keep

# Create a backup directory with timestamp
$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$backupDir = "e2e/screenshots-backup-$timestamp"
Write-Host "Creating backup directory: $backupDir"
New-Item -Path $backupDir -ItemType Directory -Force | Out-Null
Copy-Item -Path "$screenshotsDir/*" -Destination $backupDir -Recurse

# Get all screenshots
$allScreenshots = Get-ChildItem -Path $screenshotsDir -Recurse -File -Include "*.png"
Write-Host "Found $($allScreenshots.Count) screenshots"

# 1. Remove screenshots older than $maxDaysToKeep days
$cutoffDate = (Get-Date).AddDays(-$maxDaysToKeep)
$oldScreenshots = $allScreenshots | Where-Object { $_.LastWriteTime -lt $cutoffDate }
Write-Host "Found $($oldScreenshots.Count) screenshots older than $maxDaysToKeep days"

if ($oldScreenshots.Count -gt 0) {
    Write-Host "Removing old screenshots..."
    $oldScreenshots | ForEach-Object {
        Write-Host "  Removing $($_.Name)"
        Remove-Item $_.FullName -Force
    }
}

# 2. Group screenshots by prefix and keep only the most recent ones
$remainingScreenshots = Get-ChildItem -Path $screenshotsDir -Recurse -File -Include "*.png"
$prefixGroups = $remainingScreenshots | Group-Object -Property {
    # Extract prefix (everything before the last hyphen or the whole name if no hyphen)
    if ($_.Name -match "^(.*?)(-\d+)?\.png$") {
        $matches[1]
    } else {
        $_.BaseName
    }
}

foreach ($group in $prefixGroups) {
    if ($group.Count -gt $maxScreenshotsPerPrefix) {
        Write-Host "Group '$($group.Name)' has $($group.Count) screenshots, keeping $maxScreenshotsPerPrefix"

        # Sort by last write time (newest first) and skip the first $maxScreenshotsPerPrefix
        $toRemove = $group.Group | Sort-Object LastWriteTime -Descending | Select-Object -Skip $maxScreenshotsPerPrefix

        foreach ($screenshot in $toRemove) {
            Write-Host "  Removing $($screenshot.Name)"
            Remove-Item $screenshot.FullName -Force
        }
    }
}

# 3. If we still have too many screenshots, keep only the most recent ones
$remainingScreenshots = Get-ChildItem -Path $screenshotsDir -Recurse -File -Include "*.png"
if ($remainingScreenshots.Count -gt $maxTotalScreenshots) {
    Write-Host "Still have $($remainingScreenshots.Count) screenshots, reducing to $maxTotalScreenshots"

    # Sort by last write time (newest first) and skip the first $maxTotalScreenshots
    $toRemove = $remainingScreenshots | Sort-Object LastWriteTime -Descending | Select-Object -Skip $maxTotalScreenshots

    foreach ($screenshot in $toRemove) {
        Write-Host "  Removing $($screenshot.Name)"
        Remove-Item $screenshot.FullName -Force
    }
}

# Count remaining screenshots
$remainingCount = (Get-ChildItem -Path $screenshotsDir -Recurse -File -Include "*.png").Count
Write-Host "Cleanup complete. $remainingCount screenshots remaining."
