@echo off
REM ============================================================================
REM QuestNav Documentation Generator Script
REM ============================================================================
REM This script automatically generates documentation for all QuestNav components:
REM - Protocol Buffer documentation (HTML)
REM - Java API documentation (Javadoc)
REM - C# API documentation (DocFX)
REM
REM Prerequisites:
REM - protoc (Protocol Buffer compiler)
REM - protoc-gen-doc (Go package: github.com/pseudomuto/protoc-gen-doc/cmd/protoc-gen-doc)
REM - Java 17+ and Gradle (for Java docs)
REM - .NET SDK and DocFX (for C# docs)
REM
REM Usage: generate-docs.bat [--clean] [--proto-only] [--java-only] [--csharp-only]
REM ============================================================================

setlocal enabledelayedexpansion

REM Script configuration
set "SCRIPT_DIR=%~dp0"
set "PROJECT_ROOT=%SCRIPT_DIR%"
set "DOCS_DIR=%PROJECT_ROOT%docs\static\api"
set "PROTO_DIR=%PROJECT_ROOT%protos"
set "JAVA_DIR=%PROJECT_ROOT%questnav-lib"
set "UNITY_DIR=%PROJECT_ROOT%unity"
set "DOCFX_DIR=%UNITY_DIR%\DocFX"

REM Output directories
set "PROTO_OUT_DIR=%DOCS_DIR%\proto"
set "JAVA_OUT_DIR=%DOCS_DIR%\java"
set "CSHARP_OUT_DIR=%DOCS_DIR%\csharp"

REM Parse command line arguments
set "CLEAN_FIRST=false"
set "PROTO_ONLY=false"
set "JAVA_ONLY=false"
set "CSHARP_ONLY=false"

:parse_args
if "%~1"=="" goto :args_done
if /i "%~1"=="--clean" set "CLEAN_FIRST=true"
if /i "%~1"=="--proto-only" set "PROTO_ONLY=true"
if /i "%~1"=="--java-only" set "JAVA_ONLY=true"
if /i "%~1"=="--csharp-only" set "CSHARP_ONLY=true"
if /i "%~1"=="--help" goto :show_help
if /i "%~1"=="-h" goto :show_help
shift
goto :parse_args
:args_done

REM Show header
echo.
echo ============================================================================
echo QuestNav Documentation Generator
echo ============================================================================
echo.

REM Clean existing documentation if requested
if "%CLEAN_FIRST%"=="true" (
    echo [INFO] Cleaning existing documentation...
    if exist "%PROTO_OUT_DIR%" rmdir /s /q "%PROTO_OUT_DIR%"
    if exist "%JAVA_OUT_DIR%" rmdir /s /q "%JAVA_OUT_DIR%"
    if exist "%CSHARP_OUT_DIR%" rmdir /s /q "%CSHARP_OUT_DIR%"
    echo [INFO] Cleanup completed.
    echo.
)

REM Create output directories
echo [INFO] Creating output directories...
if not exist "%DOCS_DIR%" mkdir "%DOCS_DIR%"
if not exist "%PROTO_OUT_DIR%" mkdir "%PROTO_OUT_DIR%"
if not exist "%JAVA_OUT_DIR%" mkdir "%JAVA_OUT_DIR%"
if not exist "%CSHARP_OUT_DIR%" mkdir "%CSHARP_OUT_DIR%"

REM Check prerequisites
echo [INFO] Checking prerequisites...
call :check_command protoc "Protocol Buffer compiler (protoc)"
if !errorlevel! neq 0 exit /b 1

call :check_command protoc-gen-doc "Protocol Buffer documentation generator (protoc-gen-doc)"
if !errorlevel! neq 0 exit /b 1

call :check_command java "Java Runtime Environment"
if !errorlevel! neq 0 exit /b 1

call :check_command docfx "DocFX documentation generator"
if !errorlevel! neq 0 exit /b 1

echo [INFO] All prerequisites found.
echo.

REM Generate Protocol Buffer Documentation
if "%JAVA_ONLY%"=="false" if "%CSHARP_ONLY%"=="false" (
    echo ============================================================================
    echo Generating Protocol Buffer Documentation
    echo ============================================================================
    echo.
    
    echo [INFO] Generating protobuf documentation...
    cd /d "%PROJECT_ROOT%"
    
    protoc --doc_out="%PROTO_OUT_DIR%" --doc_opt=html,index.html -I=protos protos/*.proto
    if !errorlevel! neq 0 (
        echo [ERROR] Failed to generate protocol buffer documentation
        exit /b 1
    )
    
    echo [SUCCESS] Protocol buffer documentation generated successfully
    echo [INFO] Output: %PROTO_OUT_DIR%\index.html
    echo.
)

REM Generate Java Documentation
if "%PROTO_ONLY%"=="false" if "%CSHARP_ONLY%"=="false" (
    echo ============================================================================
    echo Generating Java API Documentation
    echo ============================================================================
    echo.
    
    echo [INFO] Generating Java documentation with Gradle...
    cd /d "%JAVA_DIR%"
    
    call gradlew.bat javadoc
    if !errorlevel! neq 0 (
        echo [ERROR] Failed to generate Java documentation
        exit /b 1
    )
    
    echo [INFO] Copying Java documentation to output directory...
    cd /d "%PROJECT_ROOT%"
    xcopy /e /i /y "%JAVA_DIR%\build\docs\javadoc\*" "%JAVA_OUT_DIR%\"
    if !errorlevel! neq 0 (
        echo [ERROR] Failed to copy Java documentation
        exit /b 1
    )
    
    echo [SUCCESS] Java documentation generated successfully
    echo [INFO] Output: %JAVA_OUT_DIR%\index.html
    echo.
)

REM Generate C# Documentation
if "%PROTO_ONLY%"=="false" if "%JAVA_ONLY%"=="false" (
    echo ============================================================================
    echo Generating C# API Documentation
    echo ============================================================================
    echo.
    
    echo [INFO] Generating C# documentation with DocFX...
    cd /d "%DOCFX_DIR%"
    
    docfx docfx.json
    if !errorlevel! neq 0 (
        echo [ERROR] Failed to generate C# documentation
        exit /b 1
    )
    
    echo [SUCCESS] C# documentation generated successfully
    echo [INFO] Output: %CSHARP_OUT_DIR%\index.html
    echo.
)

REM Final summary
echo ============================================================================
echo Documentation Generation Complete
echo ============================================================================
echo.
echo Generated documentation is available at:

if "%JAVA_ONLY%"=="false" if "%CSHARP_ONLY%"=="false" (
    echo   Protocol Buffers: %PROTO_OUT_DIR%\index.html
)

if "%PROTO_ONLY%"=="false" if "%CSHARP_ONLY%"=="false" (
    echo   Java API:        %JAVA_OUT_DIR%\index.html
)

if "%PROTO_ONLY%"=="false" if "%JAVA_ONLY%"=="false" (
    echo   C# API:          %CSHARP_OUT_DIR%\index.html
)

echo.
echo [SUCCESS] All documentation generated successfully!
goto :end

REM Function to check if a command exists
:check_command
where %1 >nul 2>&1
if %errorlevel% neq 0 (
    echo [ERROR] %2 not found in PATH
    echo [ERROR] Please install %1 and ensure it's available in your PATH
    exit /b 1
)
echo [OK] %2 found
exit /b 0

REM Show help information
:show_help
echo.
echo QuestNav Documentation Generator
echo.
echo Usage: generate-docs.bat [OPTIONS]
echo.
echo Options:
echo   --clean        Clean existing documentation before generating new docs
echo   --proto-only   Generate only Protocol Buffer documentation
echo   --java-only    Generate only Java API documentation
echo   --csharp-only  Generate only C# API documentation
echo   --help, -h     Show this help message
echo.
echo Prerequisites:
echo   - protoc (Protocol Buffer compiler)
echo   - protoc-gen-doc (install with: go install github.com/pseudomuto/protoc-gen-doc/cmd/protoc-gen-doc@latest)
echo   - Java 17+ and Gradle (for Java documentation)
echo   - .NET SDK and DocFX (for C# documentation)
echo.
echo Examples:
echo   generate-docs.bat                    Generate all documentation
echo   generate-docs.bat --clean           Clean and generate all documentation
echo   generate-docs.bat --java-only       Generate only Java documentation
echo   generate-docs.bat --clean --proto-only  Clean and generate only Protocol Buffer docs
echo.
goto :end

:end
cd /d "%PROJECT_ROOT%"
endlocal
