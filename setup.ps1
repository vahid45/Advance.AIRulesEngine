# Create solution
dotnet new sln -n Advance.AIRulesEngine

# Create projects
dotnet new webapi -n RulesEngine.API -o src/RulesEngine.API --force
dotnet new classlib -n RulesEngine.Core -o src/RulesEngine.Core
dotnet new classlib -n RulesEngine.ML -o src/RulesEngine.ML
dotnet new classlib -n RulesEngine.Dataverse -o src/RulesEngine.Dataverse
dotnet new classlib -n RulesEngine.UI -o src/RulesEngine.UI

# Create test project
dotnet new xunit -n RulesEngine.Tests -o test/RulesEngine.Tests

# Add projects to solution
dotnet sln add src/RulesEngine.API/RulesEngine.API.csproj
dotnet sln add src/RulesEngine.Core/RulesEngine.Core.csproj
dotnet sln add src/RulesEngine.ML/RulesEngine.ML.csproj
dotnet sln add src/RulesEngine.Dataverse/RulesEngine.Dataverse.csproj
dotnet sln add src/RulesEngine.UI/RulesEngine.UI.csproj
dotnet sln add test/RulesEngine.Tests/RulesEngine.Tests.csproj

# Add project references
dotnet add src/RulesEngine.API/RulesEngine.API.csproj reference src/RulesEngine.Core/RulesEngine.Core.csproj
dotnet add src/RulesEngine.API/RulesEngine.API.csproj reference src/RulesEngine.Dataverse/RulesEngine.Dataverse.csproj
dotnet add src/RulesEngine.ML/RulesEngine.ML.csproj reference src/RulesEngine.Core/RulesEngine.Core.csproj
dotnet add src/RulesEngine.Dataverse/RulesEngine.Dataverse.csproj reference src/RulesEngine.Core/RulesEngine.Core.csproj
dotnet add src/RulesEngine.UI/RulesEngine.UI.csproj reference src/RulesEngine.Core/RulesEngine.Core.csproj

# Add test project references
dotnet add test/RulesEngine.Tests/RulesEngine.Tests.csproj reference src/RulesEngine.Core/RulesEngine.Core.csproj
dotnet add test/RulesEngine.Tests/RulesEngine.Tests.csproj reference src/RulesEngine.ML/RulesEngine.ML.csproj
dotnet add test/RulesEngine.Tests/RulesEngine.Tests.csproj reference src/RulesEngine.Dataverse/RulesEngine.Dataverse.csproj

# Create directories (ignore if they exist)
New-Item -ItemType Directory -Force -Path samples/rules
New-Item -ItemType Directory -Force -Path docs/api
New-Item -ItemType Directory -Force -Path docs/guides

Write-Host "Solution and project structure created successfully!" 