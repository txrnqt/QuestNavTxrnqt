# QuestNav DocFX Build Script (PowerShell)
# This script generates C# API documentation and integrates it with the Docusaurus site

$ErrorActionPreference = "Stop"

Write-Host "Building QuestNav C# API Documentation" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan

# Check if DocFX is installed
try {
    docfx --version | Out-Null
} catch {
    Write-Host "DocFX not found. Installing..." -ForegroundColor Red
    dotnet tool install -g docfx
}

# Check if we're in the right directory
if (-not (Test-Path "docfx.json")) {
    Write-Host "Error: docfx.json not found. Please run this script from unity/DocFX directory" -ForegroundColor Red
    exit 1
}

Write-Host "Generating API metadata from C# project..." -ForegroundColor Yellow
docfx metadata --logLevel Info

Write-Host "Building documentation site..." -ForegroundColor Yellow
docfx build --logLevel Info

# Verify output
if (Test-Path "../../docs/static/api/csharp") {
    $apiFiles = (Get-ChildItem -Path "../../docs/static/api/csharp/api" -Filter "*.html" -Recurse).Count
    Write-Host "Documentation built successfully!" -ForegroundColor Green
    Write-Host "   Generated $apiFiles API pages" -ForegroundColor Green
    Write-Host "   Output: docs/static/api/csharp/" -ForegroundColor Green
    Write-Host ""
    Write-Host "To preview the documentation:" -ForegroundColor Cyan
    Write-Host "   1. Run 'npm start' in the docs/ directory" -ForegroundColor White
    Write-Host "   2. Navigate to http://localhost:3000/api/csharp/" -ForegroundColor White
    Write-Host ""
    Write-Host "The documentation includes:" -ForegroundColor Cyan
    Write-Host "   • QuestNav.Core - Main application logic" -ForegroundColor White
    Write-Host "   • QuestNav.Commands - Command processing" -ForegroundColor White
    Write-Host "   • QuestNav.Network - NetworkTables communication" -ForegroundColor White
    Write-Host "   • QuestNav.UI - User interface management" -ForegroundColor White
    Write-Host "   • QuestNav.Utils - Utility functions" -ForegroundColor White
    Write-Host "   • QuestNav.Native.NTCore - Native NetworkTables" -ForegroundColor White
    Write-Host "   • QuestNav.Protos.Generated - Protocol buffers" -ForegroundColor White
} else {
    Write-Host "Error: Documentation output not found at docs/static/api/csharp/" -ForegroundColor Red
    exit 1
}
