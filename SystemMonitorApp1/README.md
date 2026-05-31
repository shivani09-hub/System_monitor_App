# System Monitor Application – Design Decisions and Implementation

## Project Overview

The System Monitor Application is a cross-platform console application developed using C# and .NET 8. The application continuously monitors system resources such as CPU usage, RAM usage, and Disk usage at configurable intervals. The collected metrics are displayed in the console and can be processed by plugins to extend functionality without modifying the core application.

## Architecture Pattern

The application follows the Clean Architecture pattern combined with Dependency Injection.

### Why Clean Architecture?

Clean Architecture was chosen because it promotes:

* Separation of concerns
* Maintainability
* Testability
* Scalability
* Loose coupling between components

The application is divided into multiple layers:

### Core Layer

Contains:

* Interfaces
* Domain Models

Examples:

* IMonitorPlugin
* ISystemMonitorService
* SystemMetrics

### Application Layer

Contains business logic and service orchestration.

### Infrastructure Layer

Contains:

* File logging plugin
* API integration plugin
* System resource monitoring implementation

### Presentation Layer

Contains the Console Application (Program.cs).

Dependency Injection is used to register services and plugins, making it easy to add or replace components without affecting existing functionality.

## Plugin Architecture

A plugin interface (IMonitorPlugin) was implemented to allow extensions to the monitoring system.

Current plugins:

1. FileLoggerPlugin

   * Stores monitoring information in local log files.

2. ApiPlugin

   * Sends monitoring data to a configurable REST API endpoint using HTTP POST.

Additional plugins can be added by implementing the IMonitorPlugin interface.

## Design Decisions

Several design decisions were made during implementation:

1. Interface-based design was used to reduce coupling.
2. Dependency Injection was implemented using Microsoft.Extensions.DependencyInjection.
3. Configuration settings were externalized into appsettings.json.
4. Plugins were isolated so failures in one plugin would not stop monitoring.
5. Platform-specific monitoring logic was abstracted to support future Linux and macOS implementations.

## Challenges Encountered

During development, several challenges were identified:

* Obtaining CPU usage in a cross-platform manner.
* Handling different operating system resource APIs.
* Ensuring plugins execute independently without crashing the application.
* Maintaining compatibility with .NET 8.

These challenges were addressed through abstraction and proper exception handling.

## Corner Cases Considered

The following corner cases were considered:

* API endpoint unavailable.
* Disk drive inaccessible.
* Plugin execution failure.
* Invalid configuration values.
* System metrics temporarily unavailable.
* Network connectivity issues.

The application logs errors and continues execution whenever possible.

## Limitations

Current limitations include:

* Full monitoring support is primarily implemented for Windows.
* Dynamic loading of external plugin DLLs is not implemented.
* Historical data storage is limited to file logging.
* No graphical dashboard is provided.

## Future Enhancements

Potential improvements include:

* Linux and macOS monitoring providers.
* Dynamic plugin discovery using reflection.
* Real-time dashboard interface.
* Database storage for historical metrics.
* Email and Slack notification plugins.
* Advanced analytics and alerting system.

## How to Build

1. Open Command Prompt.
2. Navigate to the project folder.

```bash
dotnet restore
dotnet build
```

## How to Run

```bash
dotnet run
```

The application will start monitoring system resources and execute all configured plugins automatically.
