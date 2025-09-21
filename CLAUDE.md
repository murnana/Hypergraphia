# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Hypergraphia is a Personal Knowledge Management (PKM) tool designed to assist with various aspects of knowledge management, similar to Notion or Obsidian. The project is written in C# using .NET 8.0 and follows a multi-project architecture.

## Architecture

The solution consists of four main projects:

- **Hypergraphia.Core**: Core business logic and shared functionality (.NET 8.0 library)
- **Hypergraphia.MAUI**: Cross-platform mobile/desktop application using .NET MAUI
- **Hypergraphia.Console**: Command-line interface application with AOT compilation enabled
- **Hypergraphia.Core.Tests**: Unit tests using NUnit framework

All projects reference the Core library, establishing a clean dependency hierarchy where platform-specific implementations depend on the core business logic.

## Development Commands

### Building and Testing
```bash
# Restore dependencies (including MAUI workloads)
dotnet workload restore

# Build the entire solution
dotnet build

# Run all tests
dotnet test --verbosity normal

# Build specific project
dotnet build Hypergraphia.Core
dotnet build Hypergraphia.MAUI
dotnet build Hypergraphia.Console

# Run Console application
dotnet run --project Hypergraphia.Console

# Run MAUI application (requires platform-specific setup)
dotnet run --project Hypergraphia.MAUI --framework net8.0-windows10.0.19041.0
```

### Publishing
```bash
# Publish Console app with AOT
dotnet publish Hypergraphia.Console -c Release

# Publish MAUI app for specific platform
dotnet publish Hypergraphia.MAUI -c Release -f net8.0-android
```

## Code Standards

- **Language Version**: C# 12
- **Target Framework**: .NET 8.0
- **Nullable Reference Types**: Enabled across all projects
- **Implicit Usings**: Enabled
- **Warnings as Errors**: Enabled in both Debug and Release configurations
- **Testing Framework**: NUnit 4.x with NUnit3TestAdapter

## Project-Specific Notes

### MAUI Project
- Targets multiple platforms: Android, iOS, macOS Catalyst, and Windows
- Uses Microsoft.Maui.Controls for UI framework
- Debug logging enabled in development builds
- Contains platform-specific configurations and resources

### Console Project
- Configured for AOT (Ahead-of-Time) compilation in Release builds
- Uses InvariantGlobalization for reduced binary size
- Suitable for command-line tooling and automation

### Core Library
- Contains shared business logic and domain models
- Should remain platform-agnostic
- Referenced by all other projects in the solution

## CI/CD

GitHub Actions workflow runs on push/PR to main branch:
- Tests on both Ubuntu and Windows
- Uses .NET 8.0.x
- Runs `dotnet workload restore` and `dotnet test`
- Configured in `.github/workflows/dotnet-tests.yml`

## Dependencies

Key dependencies managed via Dependabot:
- NUnit testing framework
- Coverlet for code coverage
- Microsoft.Maui.Controls for UI
- Microsoft.Extensions.Logging.Debug for development logging