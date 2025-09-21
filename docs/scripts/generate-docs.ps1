# ============================================================================
# QuestNav Documentation Generator Script (PowerShell)
# ============================================================================
# This script automatically generates documentation for all QuestNav components:
# - Protocol Buffer documentation (HTML)
# - Java API documentation (Javadoc)
# - C# API documentation (DocFX)
#
# Prerequisites:
# - protoc (Protocol Buffer compiler)
# - protoc-gen-doc (Go package: github.com/pseudomuto/protoc-gen-doc/cmd/protoc-gen-doc)
# - Java 17+ and Gradle (for Java docs)
# - .NET SDK and DocFX (for C# docs)
#
# Usage: .\generate-docs.ps1 [-Clean] [-ProtoOnly] [-JavaOnly] [-CSharpOnly] [-Help]
# ============================================================================

[CmdletBinding()]
param(
    [switch]$Clean,
    [switch]$ProtoOnly,
    [switch]$JavaOnly,
    [switch]$CSharpOnly,
    [switch]$Help
)

# Script configuration
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$ProjectRoot = $ScriptDir
$DocsDir = Join-Path $ProjectRoot "docs\static\api"
$ProtoDir = Join-Path $ProjectRoot "protos"
$JavaDir = Join-Path $ProjectRoot "questnav-lib"
$UnityDir = Join-Path $ProjectRoot "unity"
$DocFXDir = Join-Path $UnityDir "DocFX"

# Output directories
$ProtoOutDir = Join-Path $DocsDir "proto"
$JavaOutDir = Join-Path $DocsDir "java"
$CSharpOutDir = Join-Path $DocsDir "csharp"

# Function to write colored output
function Write-ColorOutput {
    param(
        [string]$Message,
        [string]$Color = "White"
    )
    Write-Host $Message -ForegroundColor $Color
}

# Function to check if a command exists
function Test-Command {
    param(
        [string]$Command,
        [string]$Description
    )
    
    try {
        $null = Get-Command $Command -ErrorAction Stop
        Write-ColorOutput "[OK] $Description found" "Green"
        return $true
    }
    catch {
        Write-ColorOutput "[ERROR] $Description not found in PATH" "Red"
        Write-ColorOutput "[ERROR] Please install $Command and ensure it's available in your PATH" "Red"
        return $false
    }
}

# Function to show help
function Show-Help {
    Write-Host ""
    Write-ColorOutput "QuestNav Documentation Generator" "Cyan"
    Write-Host ""
    Write-Host "Usage: .\generate-docs.ps1 [OPTIONS]"
    Write-Host ""
    Write-Host "Options:"
    Write-Host "  -Clean        Clean existing documentation before generating new docs"
    Write-Host "  -ProtoOnly    Generate only Protocol Buffer documentation"
    Write-Host "  -JavaOnly     Generate only Java API documentation"
    Write-Host "  -CSharpOnly   Generate only C# API documentation"
    Write-Host "  -Help         Show this help message"
    Write-Host ""
    Write-Host "Prerequisites:"
    Write-Host "  - protoc (Protocol Buffer compiler)"
    Write-Host "  - protoc-gen-doc (install with: go install github.com/pseudomuto/protoc-gen-doc/cmd/protoc-gen-doc@latest)"
    Write-Host "  - Java 17+ and Gradle (for Java documentation)"
    Write-Host "  - .NET SDK and DocFX (for C# documentation)"
    Write-Host ""
    Write-Host "Examples:"
    Write-Host "  .\generate-docs.ps1                    Generate all documentation"
    Write-Host "  .\generate-docs.ps1 -Clean            Clean and generate all documentation"
    Write-Host "  .\generate-docs.ps1 -JavaOnly         Generate only Java documentation"
    Write-Host "  .\generate-docs.ps1 -Clean -ProtoOnly Clean and generate only Protocol Buffer docs"
    Write-Host ""
}

# Show help if requested
if ($Help) {
    Show-Help
    exit 0
}

# Show header
Write-Host ""
Write-ColorOutput "============================================================================" "Cyan"
Write-ColorOutput "QuestNav Documentation Generator" "Cyan"
Write-ColorOutput "============================================================================" "Cyan"
Write-Host ""

# Clean existing documentation if requested
if ($Clean) {
    Write-ColorOutput "[INFO] Cleaning existing documentation..." "Yellow"
    
    if (Test-Path $ProtoOutDir) { Remove-Item $ProtoOutDir -Recurse -Force }
    if (Test-Path $JavaOutDir) { Remove-Item $JavaOutDir -Recurse -Force }
    if (Test-Path $CSharpOutDir) { Remove-Item $CSharpOutDir -Recurse -Force }
    
    Write-ColorOutput "[INFO] Cleanup completed." "Green"
    Write-Host ""
}

# Create output directories
Write-ColorOutput "[INFO] Creating output directories..." "Yellow"
if (!(Test-Path $DocsDir)) { New-Item -ItemType Directory -Path $DocsDir -Force | Out-Null }
if (!(Test-Path $ProtoOutDir)) { New-Item -ItemType Directory -Path $ProtoOutDir -Force | Out-Null }
if (!(Test-Path $JavaOutDir)) { New-Item -ItemType Directory -Path $JavaOutDir -Force | Out-Null }
if (!(Test-Path $CSharpOutDir)) { New-Item -ItemType Directory -Path $CSharpOutDir -Force | Out-Null }

# Check prerequisites
Write-ColorOutput "[INFO] Checking prerequisites..." "Yellow"
$prerequisitesFailed = $false

if (!(Test-Command "protoc" "Protocol Buffer compiler (protoc)")) { $prerequisitesFailed = $true }
if (!(Test-Command "protoc-gen-doc" "Protocol Buffer documentation generator (protoc-gen-doc)")) { $prerequisitesFailed = $true }
if (!(Test-Command "java" "Java Runtime Environment")) { $prerequisitesFailed = $true }
if (!(Test-Command "docfx" "DocFX documentation generator")) { $prerequisitesFailed = $true }

if ($prerequisitesFailed) {
    Write-ColorOutput "[ERROR] Some prerequisites are missing. Please install them and try again." "Red"
    exit 1
}

Write-ColorOutput "[INFO] All prerequisites found." "Green"
Write-Host ""

try {
    # Generate Protocol Buffer Documentation
    if (!$JavaOnly -and !$CSharpOnly) {
        Write-ColorOutput "============================================================================" "Cyan"
        Write-ColorOutput "Generating Protocol Buffer Documentation" "Cyan"
        Write-ColorOutput "============================================================================" "Cyan"
        Write-Host ""
        
        Write-ColorOutput "[INFO] Generating protobuf documentation..." "Yellow"
        Set-Location $ProjectRoot
        
        $protoResult = & protoc --doc_out="$ProtoOutDir" --doc_opt=html,index.html -I=protos protos/*.proto
        if ($LASTEXITCODE -ne 0) {
            throw "Failed to generate protocol buffer documentation"
        }
        
        Write-ColorOutput "[SUCCESS] Protocol buffer documentation generated successfully" "Green"
        Write-ColorOutput "[INFO] Output: $ProtoOutDir\index.html" "Cyan"
        Write-Host ""
    }

    # Generate Java Documentation
    if (!$ProtoOnly -and !$CSharpOnly) {
        Write-ColorOutput "============================================================================" "Cyan"
        Write-ColorOutput "Generating Java API Documentation" "Cyan"
        Write-ColorOutput "============================================================================" "Cyan"
        Write-Host ""
        
        Write-ColorOutput "[INFO] Generating Java documentation with Gradle..." "Yellow"
        Set-Location $JavaDir
        
        if ($IsWindows -or $env:OS -eq "Windows_NT") {
            $gradleResult = & .\gradlew.bat javadoc
        } else {
            $gradleResult = & ./gradlew javadoc
        }
        
        if ($LASTEXITCODE -ne 0) {
            throw "Failed to generate Java documentation"
        }
        
        Write-ColorOutput "[INFO] Copying Java documentation to output directory..." "Yellow"
        Set-Location $ProjectRoot
        
        $javadocSource = Join-Path $JavaDir "build\docs\javadoc\*"
        Copy-Item -Path $javadocSource -Destination $JavaOutDir -Recurse -Force
        
        Write-ColorOutput "[SUCCESS] Java documentation generated successfully" "Green"
        Write-ColorOutput "[INFO] Output: $JavaOutDir\index.html" "Cyan"
        Write-Host ""
    }

    # Generate C# Documentation
    if (!$ProtoOnly -and !$JavaOnly) {
        Write-ColorOutput "============================================================================" "Cyan"
        Write-ColorOutput "Generating C# API Documentation" "Cyan"
        Write-ColorOutput "============================================================================" "Cyan"
        Write-Host ""
        
        Write-ColorOutput "[INFO] Generating C# documentation with DocFX..." "Yellow"
        Set-Location $DocFXDir
        
        $docfxResult = & docfx docfx.json
        if ($LASTEXITCODE -ne 0) {
            throw "Failed to generate C# documentation"
        }
        
        Write-ColorOutput "[SUCCESS] C# documentation generated successfully" "Green"
        Write-ColorOutput "[INFO] Output: $CSharpOutDir\index.html" "Cyan"
        Write-Host ""
    }

    # Final summary
    Write-ColorOutput "============================================================================" "Cyan"
    Write-ColorOutput "Documentation Generation Complete" "Cyan"
    Write-ColorOutput "============================================================================" "Cyan"
    Write-Host ""
    Write-ColorOutput "Generated documentation is available at:" "Green"
    Write-Host ""

    if (!$JavaOnly -and !$CSharpOnly) {
        Write-ColorOutput "  Protocol Buffers: $ProtoOutDir\index.html" "Cyan"
    }

    if (!$ProtoOnly -and !$CSharpOnly) {
        Write-ColorOutput "  Java API:        $JavaOutDir\index.html" "Cyan"
    }

    if (!$ProtoOnly -and !$JavaOnly) {
        Write-ColorOutput "  C# API:          $CSharpOutDir\index.html" "Cyan"
    }

    Write-Host ""
    Write-ColorOutput "[SUCCESS] All documentation generated successfully!" "Green"
}
catch {
    Write-ColorOutput "[ERROR] $($_.Exception.Message)" "Red"
    Set-Location $ProjectRoot
    exit 1
}
finally {
    Set-Location $ProjectRoot
}
