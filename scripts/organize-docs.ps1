# Move Documentation Files Script
# Run this script from the root of the slip-verification-system directory

# Move root-level docs to organized structure
Move-Item -Path "DEVOPS_QUICKSTART.md" -Destination "docs\devops\" -Force -ErrorAction SilentlyContinue
Move-Item -Path "DEVOPS_SUMMARY.md" -Destination "docs\devops\" -Force -ErrorAction SilentlyContinue
Move-Item -Path "DEPLOYMENT_RUNBOOK.md" -Destination "docs\devops\" -Force -ErrorAction SilentlyContinue

Move-Item -Path "ARCHITECTURE.md" -Destination "docs\architecture\" -Force -ErrorAction SilentlyContinue

Move-Item -Path "MONITORING_QUICKSTART.md" -Destination "docs\monitoring\" -Force -ErrorAction SilentlyContinue
Move-Item -Path "MONITORING_IMPLEMENTATION_SUMMARY.md" -Destination "docs\monitoring\" -Force -ErrorAction SilentlyContinue

Move-Item -Path "MESSAGE_QUEUE_IMPLEMENTATION.md" -Destination "docs\message-queue\" -Force -ErrorAction SilentlyContinue
Move-Item -Path "MESSAGE_QUEUE_FILE_STRUCTURE.md" -Destination "docs\message-queue\" -Force -ErrorAction SilentlyContinue

Move-Item -Path "NOTIFICATION_IMPLEMENTATION_SUMMARY.md" -Destination "docs\notification\" -Force -ErrorAction SilentlyContinue

Move-Item -Path "PERFORMANCE_IMPLEMENTATION_SUMMARY.md" -Destination "docs\performance\" -Force -ErrorAction SilentlyContinue
Move-Item -Path "PERFORMANCE_QUICK_REFERENCE.md" -Destination "docs\performance\" -Force -ErrorAction SilentlyContinue

Move-Item -Path "SIGNALR_IMPLEMENTATION_SUMMARY.md" -Destination "docs\signalr\" -Force -ErrorAction SilentlyContinue

Move-Item -Path "IMPLEMENTATION_SUMMARY.md" -Destination "docs\" -Force -ErrorAction SilentlyContinue

# Move files already in docs/ to subdirectories
Move-Item -Path "docs\API_DOCUMENTATION.md" -Destination "docs\api\" -Force -ErrorAction SilentlyContinue
Move-Item -Path "docs\ANGULAR_PERFORMANCE_EXAMPLES.md" -Destination "docs\performance\" -Force -ErrorAction SilentlyContinue
Move-Item -Path "docs\PERFORMANCE_OPTIMIZATION.md" -Destination "docs\performance\" -Force -ErrorAction SilentlyContinue
Move-Item -Path "docs\PERFORMANCE_QUICKSTART.md" -Destination "docs\performance\" -Force -ErrorAction SilentlyContinue
Move-Item -Path "docs\SIGNALR_QUICKSTART.md" -Destination "docs\signalr\" -Force -ErrorAction SilentlyContinue
Move-Item -Path "docs\SIGNALR_REALTIME_SYSTEM.md" -Destination "docs\signalr\" -Force -ErrorAction SilentlyContinue

# Delete root-level files that were already copied
Remove-Item -Path "SECURITY.md" -Force -ErrorAction SilentlyContinue 
Remove-Item -Path "QUICKSTART.md" -Force -ErrorAction SilentlyContinue

# Remove .gitkeep placeholder files
Get-ChildItem -Path "docs" -Recurse -Filter ".gitkeep" | Remove-Item -Force -ErrorAction SilentlyContinue

Write-Host "✅ Documentation reorganization complete!" -ForegroundColor Green
Write-Host ""
Write-Host "New structure:"
Get-ChildItem -Path "docs" -Directory | ForEach-Object { 
    $count = (Get-ChildItem -Path $_.FullName -Filter "*.md" -File).Count
    Write-Host "  - $($_.Name)/ ($count files)"
}
