# Installation Guide

## Prerequisites

- .NET 9.0 SDK or later
- Visual Studio 2022 or later (recommended)
- PowerShell 7.0 or later (for setup scripts)
- Dataverse environment with appropriate permissions

## Installation Steps

1. Clone the repository:
   ```bash
   git clone https://github.com/yourusername/Advance.AIRuleEngine.git
   cd Advance.AIRuleEngine
   ```

2. Run the setup script:
   ```powershell
   ./setup.ps1
   ```
   This script will:
   - Restore NuGet packages
   - Build the solution
   - Run initial tests
   - Configure the development environment

3. Configure Dataverse Connection:
   - Open `src/RulesEngine.API/appsettings.json`
   - Add your Dataverse connection string:
   ```json
   {
     "ConnectionStrings": {
       "Dataverse": "AuthType=OAuth;Url=https://your-org.crm.dynamics.com;Username=your-username;Password=your-password;AppId=your-app-id;RedirectUri=your-redirect-uri;"
     }
   }
   ```

4. Build and Run:
   ```bash
   dotnet build
   cd src/RulesEngine.API
   dotnet run
   ```

5. Access the API:
   - API Documentation: https://localhost:7011/swagger
   - Health Check: https://localhost:7011/health

## Project Structure

- `src/RulesEngine.Core`: Core business logic and models
- `src/RulesEngine.API`: REST API implementation
- `src/RulesEngine.Dataverse`: Dataverse integration
- `src/RulesEngine.ML`: Machine learning components
- `src/RulesEngine.UI`: User interface (if applicable)
- `tests/`: Unit and integration tests
- `docs/`: Documentation
- `samples/`: Example implementations

## Development Setup

1. Open the solution in Visual Studio:
   ```bash
   Advance.AIRulesEngine.sln
   ```

2. Install required NuGet packages:
   ```bash
   dotnet restore
   ```

3. Build the solution:
   ```bash
   dotnet build
   ```

4. Run tests:
   ```bash
   dotnet test
   ```

## Troubleshooting

### Common Issues

1. **Build Errors**
   - Ensure you have .NET 9.0 SDK installed
   - Run `dotnet restore` to restore packages
   - Check for any missing dependencies

2. **Dataverse Connection Issues**
   - Verify your connection string
   - Check network connectivity
   - Ensure proper permissions are set

3. **Swagger Not Accessible**
   - Ensure the API is running
   - Check if the port (7011) is available
   - Verify HTTPS certificate is trusted

### Getting Help

- Check the [API Documentation](https://localhost:7011/swagger)
- Review the [README](../README.md)
- Open an issue on GitHub
- Contact support at support@example.com 