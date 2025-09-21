# API Reference

Welcome to the QuestNav API Reference documentation. This section provides comprehensive documentation for all QuestNav APIs and interfaces.

## Available Documentation

### 📋 Protocol Buffers
**[View Protocol Buffer Documentation →](/api/proto/)**

Complete documentation for all QuestNav protocol buffer definitions, including:
- **Command System**: Messages for sending commands to the Quest device
- **Data Structures**: Frame data, device status, and tracking information  
- **2D Geometry**: WPILib-compatible 2D geometric primitives
- **3D Geometry**: Advanced 3D geometric representations with quaternions

The protocol buffers define the communication interface between robot code and the Quest headset.

### ☕ Java API
**[View Java API Documentation →](/api/java/)**

Javadoc documentation for the QuestNav Java library, including:
- **QuestNav Class**: Main interface for communicating with Quest devices
- **PoseFrame**: Data structure for Quest pose information
- **Protocol Buffer Integration**: WPILib-compatible protobuf wrappers
- **Network Communication**: NetworkTables integration for robot communication

Use this library to integrate QuestNav into your FRC robot code.

### 🔷 C# API  
**[View C# API Documentation →](/api/csharp/)**

DocFX documentation for the Unity C# components, including:
- **Core Systems**: Quest tracking and pose estimation
- **UI Components**: User interface and visualization systems
- **Network Integration**: Communication with robot systems
- **Native Interfaces**: Low-level Quest device integration

This documentation covers the Unity application that runs on the Quest headset.

## Getting Started with the APIs

### For Robot Developers (Java)
1. Add the QuestNav library to your robot project
2. Review the [Java API documentation](/api/java/) for the `QuestNav` class
3. Check the [Protocol Buffer documentation](/api/proto/) to understand data structures
4. Follow the [robot code setup guide](/docs/getting-started/robot-code) for integration examples

### For Quest App Developers (C#)
1. Open the Unity project in the QuestNav repository
2. Review the [C# API documentation](/api/csharp/) for core components
3. Check the [Protocol Buffer documentation](/api/proto/) for communication interfaces
4. Follow the [app setup guide](/docs/getting-started/app-setup) for development setup

### For Integration Developers
1. Start with the [Protocol Buffer documentation](/api/proto/) to understand the communication protocol
2. Review both [Java](/api/java/) and [C#](/api/csharp/) APIs for implementation examples
3. Check the [development setup guide](/docs/development/development-setup) for build instructions

## Documentation Generation

All API documentation is automatically generated from source code:
- **Protocol Buffers**: Generated using `protoc-gen-doc` from `.proto` files with detailed comments
- **Java API**: Generated using Gradle Javadoc from Java source with comprehensive documentation
- **C# API**: Generated using DocFX from C# source with XML documentation comments

The documentation is kept up-to-date with each release and reflects the current state of the APIs.

## Need Help?

- **Issues**: Report bugs or request features on [GitHub](https://github.com/QuestNav/QuestNav/issues)
- **Discussions**: Ask questions in [GitHub Discussions](https://github.com/QuestNav/QuestNav/discussions)  
- **Community**: Join our [Discord server](https://discord.gg/hD3FtR7YAZ) for real-time help
- **Contributing**: See the [development guide](/docs/development/contributing) to contribute to the documentation
