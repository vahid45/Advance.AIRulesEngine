using System;
using System.Collections.Generic;
using System.Linq;
using RulesEngine.Core.Models;
using RulesEngine.Core.Services;
using Xunit;

namespace RulesEngine.Tests.Services
{
    public class RuleValidatorTests
    {
        private readonly RuleValidator _validator;

        public RuleValidatorTests()
        {
            _validator = new RuleValidator();
        }

        [Fact]
        public void ValidateRule_WithValidRule_ReturnsNoErrors()
        {
            // Arrange
            var rule = CreateValidRule();

            // Act
            var result = _validator.ValidateRule(rule);

            // Assert
            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void ValidateRule_WithInvalidName_ReturnsError()
        {
            // Arrange
            var rule = CreateValidRule();
            rule.Name = string.Empty;

            // Act
            var result = _validator.ValidateRule(rule);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains("Rule name is required", result.Errors);
        }

        [Fact]
        public void ValidateRule_WithInvalidDescription_ReturnsError()
        {
            // Arrange
            var rule = CreateValidRule();
            rule.Description = string.Empty;

            // Act
            var result = _validator.ValidateRule(rule);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains("Rule description is required", result.Errors);
        }

        [Fact]
        public void ValidateRule_WithInvalidEntityName_ReturnsError()
        {
            // Arrange
            var rule = CreateValidRule();
            rule.EntityName = string.Empty;

            // Act
            var result = _validator.ValidateRule(rule);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains("Entity name is required", result.Errors);
        }

        [Fact]
        public void ValidateRule_WithInvalidOperator_ReturnsError()
        {
            // Arrange
            var rule = CreateValidRule();
            rule.RootCondition.Operator = "INVALID_OPERATOR";

            // Act
            var result = _validator.ValidateRule(rule);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains("Invalid operator", result.Errors);
        }

        [Fact]
        public void ValidateRule_WithInvalidFieldType_ReturnsError()
        {
            // Arrange
            var rule = CreateValidRule();
            rule.RootCondition.FieldType = "INVALID_TYPE";

            // Act
            var result = _validator.ValidateRule(rule);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains("Invalid field type", result.Errors);
        }

        [Fact]
        public void ValidateRule_WithInvalidActionType_ReturnsError()
        {
            // Arrange
            var rule = CreateValidRule();
            rule.Actions[0].ActionType = "INVALID_ACTION";

            // Act
            var result = _validator.ValidateRule(rule);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains("Invalid action type", result.Errors);
        }

        [Fact]
        public void ValidateRule_WithMissingActionParameters_ReturnsError()
        {
            // Arrange
            var rule = CreateValidRule();
            rule.Actions[0].Parameters.Clear();

            // Act
            var result = _validator.ValidateRule(rule);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains("required", result.Errors);
        }

        [Fact]
        public void ValidateRule_WithInvalidEmail_ReturnsError()
        {
            // Arrange
            var rule = CreateValidRule();
            rule.Actions[0].Parameters["to"] = "invalid-email";

            // Act
            var result = _validator.ValidateRule(rule);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains("Invalid email", result.Errors);
        }

        [Fact]
        public void ValidateRule_WithInvalidGuid_ReturnsError()
        {
            // Arrange
            var rule = CreateValidRule();
            rule.Actions[0].Parameters["recordId"] = "invalid-guid";

            // Act
            var result = _validator.ValidateRule(rule);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains("Invalid GUID", result.Errors);
        }

        [Fact]
        public void ValidateRule_WithInvalidNumber_ReturnsError()
        {
            // Arrange
            var rule = CreateValidRule();
            rule.Actions[0].Parameters["amount"] = "invalid-number";

            // Act
            var result = _validator.ValidateRule(rule);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains("Invalid number", result.Errors);
        }

        [Fact]
        public void ValidateRule_WithInvalidDate_ReturnsError()
        {
            // Arrange
            var rule = CreateValidRule();
            rule.Actions[0].Parameters["date"] = "invalid-date";

            // Act
            var result = _validator.ValidateRule(rule);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains("Invalid date", result.Errors);
        }

        [Fact]
        public void ValidateRule_WithInvalidBoolean_ReturnsError()
        {
            // Arrange
            var rule = CreateValidRule();
            rule.Actions[0].Parameters["isActive"] = "invalid-boolean";

            // Act
            var result = _validator.ValidateRule(rule);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains("Invalid boolean", result.Errors);
        }

        [Fact]
        public void ValidateRule_WithInvalidMetadata_ReturnsError()
        {
            // Arrange
            var rule = CreateValidRule();
            rule.Metadata["key"] = new string('x', 1001);

            // Act
            var result = _validator.ValidateRule(rule);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains("Metadata value cannot exceed 1000 characters", result.Errors);
        }

        [Fact]
        public void ValidateRule_WithTooManyActions_ReturnsError()
        {
            // Arrange
            var rule = CreateValidRule();
            for (int i = 0; i < 11; i++)
            {
                rule.Actions.Add(new RuleAction
                {
                    ActionType = "UPDATE_FIELD",
                    TargetEntity = "account",
                    TargetField = "description",
                    Value = $"Action {i}"
                });
            }

            // Act
            var result = _validator.ValidateRule(rule);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains("Maximum of 10 actions allowed", result.Errors);
        }

        [Fact]
        public void ValidateRule_WithTooManySubConditions_ReturnsError()
        {
            // Arrange
            var rule = CreateValidRule();
            rule.RootCondition.Operator = "AND";
            for (int i = 0; i < 11; i++)
            {
                rule.RootCondition.SubConditions.Add(new RuleCondition
                {
                    FieldName = "field",
                    Operator = "EQUALS",
                    FieldType = "STRING",
                    Value = $"value {i}"
                });
            }

            // Act
            var result = _validator.ValidateRule(rule);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains("cannot have more than 10 sub-conditions", result.Errors);
        }

        private static Rule CreateValidRule()
        {
            return new Rule(
                name: "Test Rule",
                description: "Test Description",
                entityName: "account",
                createdBy: "TestUser",
                environment: "Test",
                rootCondition: new RuleCondition
                {
                    FieldName = "name",
                    Operator = "EQUALS",
                    FieldType = "STRING",
                    Value = "Test"
                }
            )
            {
                Actions = new List<RuleAction>
                {
                    new()
                    {
                        ActionType = "UPDATE_FIELD",
                        TargetEntity = "account",
                        TargetField = "description",
                        Value = "Updated by rule",
                        Parameters = new Dictionary<string, object>
                        {
                            { "to", "test@example.com" },
                            { "recordId", Guid.NewGuid().ToString() },
                            { "amount", "100.50" },
                            { "date", DateTime.Now.ToString("O") },
                            { "isActive", "true" }
                        }
                    }
                }
            };
        }
    }
} 