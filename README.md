# Advance.AIRulesEngine

An AI-assisted rules engine for Microsoft Dynamics 365 that enables low-code business process automation with machine learning capabilities.

## Features

- Low-code rule definition using JSON schema or visual designer
- Integration with Dynamics 365 (Dataverse) entities
- Machine learning-based rule suggestions
- Multi-condition rule evaluation
- Dynamic action execution
- Rule versioning and environment management
- Comprehensive execution logging

## Tech Stack

- Backend: ASP.NET Core (.NET 8)
- Database: Microsoft Dataverse (Dynamics 365)
- ML Engine: ML.NET
- Frontend: React (optional)
- Integration: Dynamics Web API

## Project Structure

```
/src
  ├── RulesEngine.API          # Main API entry point
  ├── RulesEngine.Core         # Core rule models and evaluator
  ├── RulesEngine.ML           # ML model training and suggestions
  ├── RulesEngine.Dataverse    # Dynamics 365 integration
  └── RulesEngine.UI           # Web frontend (optional)
/test                           # Unit tests
/samples                        # Example rules and datasets
/docs                           # Documentation
```

## Getting Started

### Prerequisites

- .NET 8 SDK
- Visual Studio 2022 or VS Code
- Dynamics 365 environment with API access
- SQL Server (for local development)

### Setup

1. Clone the repository
2. Restore NuGet packages:
   ```bash
   dotnet restore
   ```
3. Update the connection strings in `appsettings.json`
4. Run the solution:
   ```bash
   dotnet run --project src/RulesEngine.API
   ```

## Development

### Building the Solution

```bash
dotnet build
```

### Running Tests

```bash
dotnet test
```

## License

MIT License

## Contributing

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request 