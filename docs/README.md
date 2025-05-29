# Rules Engine Documentation

## Overview
The Rules Engine is a powerful and flexible system that allows you to define, validate, and execute business rules against your data. It supports various operators, conditions, and actions, making it suitable for complex business logic implementation.

## Features
- Rule definition with conditions and actions
- Support for multiple operators (logical, comparison, string, collection, date)
- Rule validation
- Rule evaluation against entity data
- Action execution
- Integration with Dataverse
- Performance optimization
- Comprehensive testing

## Getting Started

### Prerequisites
- .NET 6.0 or later
- Dataverse environment
- Required NuGet packages:
  - Microsoft.PowerPlatform.Dataverse.Client
  - Microsoft.Extensions.DependencyInjection

### Configuration
1. Add the Dataverse connection string to your `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "Dataverse": "AuthType=OAuth;Url=https://your-org.crm.dynamics.com;Username=your-username;Password=your-password;AppId=your-app-id;RedirectUri=app://58145B91-0C36-4500-8554-080854F2AC97;LoginPrompt=Auto"
  }
}
```

2. Register services in your `Program.cs`:
```csharp
services.AddSingleton<ServiceClient>(provider =>
{
    var connectionString = Configuration.GetConnectionString("Dataverse");
    return new ServiceClient(connectionString);
});

services.AddScoped<IRuleEvaluator, RuleEvaluator>();
services.AddScoped<IDataverseService, DataverseService>();
services.AddScoped<IActionExecutor, ActionExecutor>();
services.AddScoped<RuleValidator>();
```

## Rule Definition

### Basic Rule Structure
```csharp
var rule = new Rule
{
    Name = "High Value Account Rule",
    Description = "Identifies and processes high-value accounts",
    EntityName = "account",
    CreatedBy = "System",
    Environment = "Production",
    IsActive = true,
    RootCondition = new RuleCondition
    {
        Operator = "AND",
        SubConditions = new List<RuleCondition>
        {
            new()
            {
                Operator = "GREATER_THAN",
                FieldName = "revenue",
                FieldType = "Money",
                Value = 1000000
            },
            new()
            {
                Operator = "EQUALS",
                FieldName = "status",
                FieldType = "String",
                Value = "Active"
            }
        }
    },
    Actions = new List<RuleAction>
    {
        new()
        {
            ActionType = "UPDATE_FIELD",
            TargetEntity = "account",
            TargetField = "description",
            Value = "High-value account identified"
        }
    }
};
```

### Supported Operators

#### Logical Operators
- `AND`: All sub-conditions must be true
- `OR`: At least one sub-condition must be true
- `NOT`: Negates the result of a single sub-condition
- `XOR`: Exactly one sub-condition must be true

#### Comparison Operators
- `EQUALS`: Values are equal
- `NOT_EQUALS`: Values are not equal
- `GREATER_THAN`: First value is greater than second
- `LESS_THAN`: First value is less than second
- `GREATER_THAN_OR_EQUALS`: First value is greater than or equal to second
- `LESS_THAN_OR_EQUALS`: First value is less than or equal to second

#### String Operators
- `CONTAINS`: String contains the specified value
- `NOT_CONTAINS`: String does not contain the specified value
- `STARTS_WITH`: String starts with the specified value
- `ENDS_WITH`: String ends with the specified value
- `REGEX_MATCH`: String matches the specified regular expression
- `IS_EMPTY`: String is null or empty
- `IS_NOT_EMPTY`: String is not null and not empty

#### Collection Operators
- `IN`: Value exists in the collection
- `NOT_IN`: Value does not exist in the collection
- `CONTAINS_ALL`: Collection contains all specified values
- `CONTAINS_ANY`: Collection contains any of the specified values

#### Date Operators
- `IS_TODAY`: Date is today
- `IS_FUTURE`: Date is in the future
- `IS_PAST`: Date is in the past
- `IS_WITHIN_DAYS`: Date is within specified number of days

### Supported Actions

#### Field Updates
```csharp
new RuleAction
{
    ActionType = "UPDATE_FIELD",
    TargetEntity = "account",
    TargetField = "description",
    Value = "Updated by rule engine"
}
```

#### Record Operations
```csharp
// Create Record
new RuleAction
{
    ActionType = "CREATE_RECORD",
    TargetEntity = "contact",
    Parameters = new Dictionary<string, object>
    {
        { "firstname", "John" },
        { "lastname", "Doe" },
        { "emailaddress1", "john.doe@example.com" }
    }
}

// Update Record
new RuleAction
{
    ActionType = "UPDATE_RECORD",
    TargetEntity = "account",
    Parameters = new Dictionary<string, object>
    {
        { "name", "Updated Account Name" },
        { "revenue", 2000000 }
    }
}

// Delete Record
new RuleAction
{
    ActionType = "DELETE_RECORD",
    TargetEntity = "account",
    Parameters = new Dictionary<string, object>
    {
        { "recordId", "00000000-0000-0000-0000-000000000000" }
    }
}
```

#### Email Actions
```csharp
new RuleAction
{
    ActionType = "SEND_EMAIL",
    Parameters = new Dictionary<string, object>
    {
        { "to", "recipient@example.com" },
        { "subject", "Rule Triggered" },
        { "body", "This is the email body" }
    }
}
```

#### Workflow Actions
```csharp
new RuleAction
{
    ActionType = "START_WORKFLOW",
    Parameters = new Dictionary<string, object>
    {
        { "workflowId", "00000000-0000-0000-0000-000000000000" }
    }
}
```

#### Status and Assignment Actions
```csharp
// Set Status
new RuleAction
{
    ActionType = "SET_STATUS",
    Parameters = new Dictionary<string, object>
    {
        { "status", 1 },
        { "statusCode", 2 }
    }
}

// Assign Record
new RuleAction
{
    ActionType = "ASSIGN_RECORD",
    Parameters = new Dictionary<string, object>
    {
        { "assigneeId", "00000000-0000-0000-0000-000000000000" }
    }
}

// Share Record
new RuleAction
{
    ActionType = "SHARE_RECORD",
    Parameters = new Dictionary<string, object>
    {
        { "userId", "00000000-0000-0000-0000-000000000000" }
    }
}
```

## Usage Examples

### Creating and Validating a Rule
```csharp
var rule = new Rule
{
    // ... rule definition ...
};

var validator = new RuleValidator();
var validationResult = validator.ValidateRule(rule);

if (validationResult.IsValid)
{
    // Rule is valid, proceed with creation
}
else
{
    // Handle validation errors
    foreach (var error in validationResult.Errors)
    {
        Console.WriteLine(error);
    }
}
```

### Evaluating a Rule
```csharp
var entityData = new
{
    name = "Test Account",
    revenue = 2000000m,
    status = "Active",
    category = new[] { "Premium", "Enterprise" }
};

var evaluationResult = await _ruleEvaluator.EvaluateRuleAsync(rule, entityData);

if (evaluationResult.IsSuccess)
{
    // Rule conditions were met
    foreach (var action in evaluationResult.ExecutedActions)
    {
        Console.WriteLine($"Executed action: {action.ActionType}");
    }
}
```

### Complex Rule Example
```csharp
var rule = new Rule
{
    Name = "Complex Business Rule",
    Description = "Processes high-value accounts with specific criteria",
    EntityName = "account",
    CreatedBy = "System",
    Environment = "Production",
    IsActive = true,
    RootCondition = new RuleCondition
    {
        Operator = "OR",
        SubConditions = new List<RuleCondition>
        {
            new()
            {
                Operator = "AND",
                SubConditions = new List<RuleCondition>
                {
                    new()
                    {
                        Operator = "GREATER_THAN",
                        FieldName = "revenue",
                        FieldType = "Money",
                        Value = 1000000
                    },
                    new()
                    {
                        Operator = "CONTAINS_ALL",
                        FieldName = "category",
                        FieldType = "String",
                        Value = new[] { "Premium", "Enterprise" }
                    }
                }
            },
            new()
            {
                Operator = "AND",
                SubConditions = new List<RuleCondition>
                {
                    new()
                    {
                        Operator = "IS_WITHIN_DAYS",
                        FieldName = "createdon",
                        FieldType = "DateTime",
                        Value = 30
                    },
                    new()
                    {
                        Operator = "GREATER_THAN",
                        FieldName = "employees",
                        FieldType = "Int",
                        Value = 500
                    }
                }
            }
        }
    },
    Actions = new List<RuleAction>
    {
        new()
        {
            ActionType = "UPDATE_FIELD",
            TargetEntity = "account",
            TargetField = "description",
            Value = "High-value account processed"
        },
        new()
        {
            ActionType = "SEND_EMAIL",
            Parameters = new Dictionary<string, object>
            {
                { "to", "admin@example.com" },
                { "subject", "High-Value Account Identified" },
                { "body", "A high-value account has been identified and processed." }
            }
        }
    }
};
```

## Best Practices

1. **Rule Design**
   - Keep rules focused and single-purpose
   - Use meaningful names and descriptions
   - Validate rules before deployment
   - Consider performance implications of complex conditions

2. **Performance**
   - Use appropriate operators for the data type
   - Avoid deeply nested conditions
   - Consider caching frequently used rules
   - Monitor rule evaluation performance

3. **Maintenance**
   - Version your rules
   - Document rule purposes and dependencies
   - Test rules thoroughly before deployment
   - Monitor rule execution and errors

4. **Security**
   - Validate all input data
   - Use appropriate permissions for actions
   - Log rule executions and actions
   - Review and audit rules regularly

## Troubleshooting

### Common Issues

1. **Rule Validation Failures**
   - Check required fields are provided
   - Verify operator and field type compatibility
   - Ensure action parameters are correct
   - Validate field names and entity names

2. **Rule Evaluation Issues**
   - Verify entity data structure
   - Check field types match
   - Ensure conditions are properly nested
   - Validate action parameters

3. **Performance Issues**
   - Review rule complexity
   - Check for unnecessary conditions
   - Monitor database performance
   - Consider caching strategies

### Debugging Tips

1. Enable detailed logging
2. Use the rule validator to check rule structure
3. Test conditions individually
4. Monitor action execution
5. Review performance metrics

## API Reference

### RuleEvaluator
- `EvaluateRuleAsync(Rule rule, object entityData)`: Evaluates a rule against entity data
- Returns `RuleEvaluationResult` with success status and executed actions

### RuleValidator
- `ValidateRule(Rule rule)`: Validates a rule's structure and parameters
- Returns `ValidationResult` with validation status and errors

### DataverseService
- `CreateRuleAsync(Rule rule)`: Creates a new rule
- `GetRuleAsync(Guid id)`: Retrieves a rule by ID
- `UpdateRuleAsync(Rule rule)`: Updates an existing rule
- `DeleteRuleAsync(Guid id)`: Deletes a rule
- `GetRulesAsync()`: Retrieves all rules

### ActionExecutor
- `ExecuteActionAsync(RuleAction action, object entityData)`: Executes a rule action
- Returns `ActionExecutionResult` with execution status and details 