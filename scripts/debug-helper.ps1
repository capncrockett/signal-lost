# Debug Helper Script
# This script provides utilities for debugging the Signal Lost game

# Configuration
$gameUrl = "http://localhost:5173"
$devServerCommand = "npm run dev"
$testCommand = "npm test -- --watch"
$e2eTestCommand = "npm run test:e2e"
$visualTestCommand = "npm run test:visual"

# Function to start the development server
function Start-DevServer {
    Write-Host "Starting development server..." -ForegroundColor Cyan
    Start-Process powershell -ArgumentList "-Command $devServerCommand"
    
    # Wait for server to start
    Write-Host "Waiting for server to start..." -ForegroundColor Yellow
    Start-Sleep -Seconds 3
    
    # Open browser
    Write-Host "Opening browser at $gameUrl" -ForegroundColor Green
    Start-Process $gameUrl
}

# Function to run tests in watch mode
function Start-TestWatcher {
    Write-Host "Starting test watcher..." -ForegroundColor Cyan
    Start-Process powershell -ArgumentList "-Command $testCommand"
}

# Function to run E2E tests
function Run-E2ETests {
    Write-Host "Running E2E tests..." -ForegroundColor Cyan
    Invoke-Expression $e2eTestCommand
}

# Function to run visual tests
function Run-VisualTests {
    Write-Host "Running visual tests..." -ForegroundColor Cyan
    Invoke-Expression $visualTestCommand
}

# Function to clean up screenshots
function Clean-Screenshots {
    Write-Host "Cleaning up screenshots..." -ForegroundColor Cyan
    Invoke-Expression "npm run cleanup:screenshots"
}

# Function to check for TypeScript errors
function Check-TypeScript {
    Write-Host "Checking for TypeScript errors..." -ForegroundColor Cyan
    Invoke-Expression "npm run type-check"
}

# Function to run linting
function Run-Linting {
    Write-Host "Running linting..." -ForegroundColor Cyan
    Invoke-Expression "npm run lint"
}

# Function to run all checks
function Run-AllChecks {
    Write-Host "Running all checks..." -ForegroundColor Cyan
    Invoke-Expression "npm run check-all"
}

# Function to start a complete development environment
function Start-DevEnvironment {
    Write-Host "Starting complete development environment..." -ForegroundColor Magenta
    
    # Start dev server in a new window
    Start-DevServer
    
    # Start test watcher in a new window
    Start-TestWatcher
    
    # Run initial TypeScript check
    Check-TypeScript
    
    Write-Host "Development environment is ready!" -ForegroundColor Green
    Write-Host "- Game is running at $gameUrl" -ForegroundColor Green
    Write-Host "- Tests are running in watch mode" -ForegroundColor Green
}

# Function to check other agent's work
function Check-OtherAgentWork {
    param (
        [string]$agentType
    )
    
    $otherAgent = if ($agentType -eq "alpha") { "beta" } else { "alpha" }
    Write-Host "Checking $otherAgent agent's work..." -ForegroundColor Cyan
    
    # Fetch latest
    git fetch origin
    
    # List other agent's branches
    $branches = git branch -r | Select-String "feature/$otherAgent/"
    
    foreach ($branch in $branches) {
        Write-Host "Checking branch: $branch" -ForegroundColor Yellow
        git checkout $branch
        npm run check-all
    }
}

# Function to start contract validation
function Start-ContractValidation {
    Write-Host "Running contract validation..." -ForegroundColor Cyan
    npm run dev:validate-contracts
}

# Display menu
function Show-Menu {
    Clear-Host
    Write-Host "=== Signal Lost Debug Helper ===" -ForegroundColor Magenta
    Write-Host
    Write-Host "1. Start development server" -ForegroundColor Cyan
    Write-Host "2. Start test watcher" -ForegroundColor Cyan
    Write-Host "3. Run E2E tests" -ForegroundColor Cyan
    Write-Host "4. Run visual tests" -ForegroundColor Cyan
    Write-Host "5. Clean up screenshots" -ForegroundColor Cyan
    Write-Host "6. Check for TypeScript errors" -ForegroundColor Cyan
    Write-Host "7. Run linting" -ForegroundColor Cyan
    Write-Host "8. Run all checks" -ForegroundColor Cyan
    Write-Host "9. Start complete development environment" -ForegroundColor Green
    Write-Host "10. Check other agent's work" -ForegroundColor Cyan
    Write-Host "11. Start contract validation" -ForegroundColor Cyan
    Write-Host
    Write-Host "0. Exit" -ForegroundColor Red
    Write-Host
    
    $choice = Read-Host "Enter your choice"
    
    switch ($choice) {
        "1" { Start-DevServer; pause; Show-Menu }
        "2" { Start-TestWatcher; pause; Show-Menu }
        "3" { Run-E2ETests; pause; Show-Menu }
        "4" { Run-VisualTests; pause; Show-Menu }
        "5" { Clean-Screenshots; pause; Show-Menu }
        "6" { Check-TypeScript; pause; Show-Menu }
        "7" { Run-Linting; pause; Show-Menu }
        "8" { Run-AllChecks; pause; Show-Menu }
        "9" { Start-DevEnvironment; pause; Show-Menu }
        "10" {
            $agentType = Read-Host "Enter agent type (alpha/beta):"
            Check-OtherAgentWork -agentType $agentType
            pause
            Show-Menu
        }
        "11" { Start-ContractValidation; pause; Show-Menu }
        "0" { return }
        default { Write-Host "Invalid choice. Press any key to continue..."; pause; Show-Menu }
    }
}

# Start the menu
Show-Menu
