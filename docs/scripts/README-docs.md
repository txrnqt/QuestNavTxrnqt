# QuestNav Documentation Generator

This directory contains scripts to automatically generate comprehensive documentation for all QuestNav components.

## Generated Documentation Types

1. **Protocol Buffer Documentation** - HTML documentation for all `.proto` files with detailed comments
2. **Java API Documentation** - Javadoc for the QuestNav Java library
3. **C# API Documentation** - DocFX documentation for the Unity C# components

## Prerequisites

Before running the documentation generators, ensure you have the following tools installed:

### Required Tools

1. **Protocol Buffer Compiler (protoc)**
   - Download from: https://github.com/protocolbuffers/protobuf/releases
   - Ensure `protoc` is in your PATH

2. **Protocol Buffer Documentation Generator (protoc-gen-doc)**
   ```bash
   go install github.com/pseudomuto/protoc-gen-doc/cmd/protoc-gen-doc@latest
   ```

3. **Java 17+ and Gradle**
   - Java: https://adoptium.net/
   - Gradle is included in the project (gradlew)

4. **DocFX for C# Documentation**
   ```bash
   dotnet tool install -g docfx
   ```

## Usage

### Windows Batch Script

```cmd
# Generate all documentation
generate-docs.bat

# Clean and generate all documentation
generate-docs.bat --clean

# Generate only specific documentation types
generate-docs.bat --proto-only
generate-docs.bat --java-only
generate-docs.bat --csharp-only

# Show help
generate-docs.bat --help
```

### PowerShell Script (Recommended)

```powershell
# Generate all documentation
.\generate-docs.ps1

# Clean and generate all documentation
.\generate-docs.ps1 -Clean

# Generate only specific documentation types
.\generate-docs.ps1 -ProtoOnly
.\generate-docs.ps1 -JavaOnly
.\generate-docs.ps1 -CSharpOnly

# Show help
.\generate-docs.ps1 -Help
```

## Output Locations

All generated documentation is placed in the `docs/static/api/` directory:

- **Protocol Buffers**: `docs/static/api/proto/index.html`
- **Java API**: `docs/static/api/java/index.html`
- **C# API**: `docs/static/api/csharp/index.html`

## Script Features

### Error Handling
- Prerequisite checking before execution
- Detailed error messages with suggestions
- Graceful failure with proper exit codes

### Flexibility
- Generate all documentation types or specific ones
- Clean existing documentation before regeneration
- Cross-platform PowerShell support

### Logging
- Color-coded output for easy reading
- Progress indicators for each step
- Success/failure status reporting

## Troubleshooting

### Common Issues

1. **"protoc not found"**
   - Install Protocol Buffer compiler and add to PATH
   - Verify with: `protoc --version`

2. **"protoc-gen-doc not found"**
   - Install with Go: `go install github.com/pseudomuto/protoc-gen-doc/cmd/protoc-gen-doc@latest`
   - Ensure Go's bin directory is in PATH

3. **"java not found"**
   - Install Java 17 or later
   - Verify with: `java --version`

4. **"docfx not found"**
   - Install DocFX: `dotnet tool install -g docfx`
   - Verify with: `docfx --version`

5. **Gradle build failures**
   - Ensure Java 17+ is installed and set as JAVA_HOME
   - Run `.\gradlew.bat --version` in the questnav-lib directory

### Getting Help

If you encounter issues:

1. Run the script with verbose output to see detailed error messages
2. Check that all prerequisites are properly installed
3. Verify that you're running the script from the project root directory
4. Check the individual tool documentation for specific configuration requirements

## Development

### Adding New Documentation Types

To add support for additional documentation types:

1. Add new command-line parameters to both scripts
2. Implement prerequisite checking for required tools
3. Add the generation logic in the appropriate section
4. Update the final summary to include the new documentation type
5. Update this README with the new documentation type information

### Modifying Output Locations

To change where documentation is generated:

1. Update the output directory variables at the top of each script
2. Ensure the target directories are created in the setup section
3. Update this README with the new locations
