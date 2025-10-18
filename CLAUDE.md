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

### Documentation Standards

- **XML Documentation**: All public classes, methods, and properties must include comprehensive XML documentation comments
- **Comment Language**: All comments and documentation must be written in Japanese
- **Documentation Style**: Use clear, concise descriptions that explain the purpose, parameters, return values, and any important behavior
- **Parameter Documentation**: Document all parameters including their purpose and any constraints
- **Return Value Documentation**: Clearly describe what the method returns and when it might return null
- **Exception Documentation**: Document any exceptions that might be thrown using `<exception>` tags

### Development Methodology

- **Test-Driven Development (TDD)**: All new features must be developed using TDD approach
  - Write failing tests first
  - Implement minimal code to make tests pass
  - Refactor code while keeping tests green
- **Test Coverage**: Maintain high test coverage for all business logic
- **Test Structure**: Use AAA pattern (Arrange, Act, Assert) for test organization
- **Test Naming**: Use descriptive Japanese test method names that explain the scenario being tested

### Documentation Maintenance

- **IMPORTANT**: When making changes that affect setup procedures or user operations, **always update the corresponding documentation**
- Relevant documentation files that may need updates:
  - `docs/gmail-api-setup.md` - Gmail API authentication setup
  - `docs/development/console-setup.md` - Console application development setup
  - Configuration files (appsettings.json, etc.)
  - README files in affected directories
- Keep documentation in sync with code changes to ensure users have accurate setup instructions

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
- to memorize