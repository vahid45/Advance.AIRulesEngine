using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using Xunit;
using RulesEngine.Core.Interfaces;
using RulesEngine.Core.Models;
using RulesEngine.Core.Services;

namespace RulesEngine.Tests.Services
{
    public class RuleEvaluatorTests
    {
        private readonly IRuleEvaluator _ruleEvaluator;
        private readonly Mock<IActionExecutor> _actionExecutorMock;

        public RuleEvaluatorTests()
        {
            _actionExecutorMock = new Mock<IActionExecutor>();
            _ruleEvaluator = new RuleEvaluator(_actionExecutorMock.Object);
        }

        [Fact]
        public async Task EvaluateRule_WithValidCondition_ReturnsTrue()
        {
            // Arrange
            var rule = new Rule(
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
                    Value = "Test Account"
                }
            );

            var entityData = new Dictionary<string, object>
            {
                { "name", "Test Account" }
            };

            // Act
            var result = await _ruleEvaluator.EvaluateRuleAsync(rule, entityData);

            // Assert
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task EvaluateRule_WithInvalidCondition_ReturnsFalse()
        {
            // Arrange
            var rule = new Rule(
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
                    Value = "Test Account"
                }
            );

            var entityData = new Dictionary<string, object>
            {
                { "name", "Different Account" }
            };

            // Act
            var result = await _ruleEvaluator.EvaluateRuleAsync(rule, entityData);

            // Assert
            Assert.False(result.IsSuccess);
        }

        [Fact]
        public async Task EvaluateRule_WithNestedConditions_ReturnsCorrectResult()
        {
            // Arrange
            var rule = new Rule(
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
                    Value = "Test Account",
                    SubConditions = new List<RuleCondition>
                    {
                        new()
                        {
                            FieldName = "status",
                            Operator = "EQUALS",
                            FieldType = "STRING",
                            Value = "Active"
                        }
                    }
                }
            );

            var entityData = new Dictionary<string, object>
            {
                { "name", "Test Account" },
                { "status", "Active" }
            };

            // Act
            var result = await _ruleEvaluator.EvaluateRuleAsync(rule, entityData);

            // Assert
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task EvaluateRule_WithMissingField_ReturnsFalse()
        {
            // Arrange
            var rule = new Rule(
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
                    Value = "Test Account"
                }
            );

            var entityData = new Dictionary<string, object>();

            // Act
            var result = await _ruleEvaluator.EvaluateRuleAsync(rule, entityData);

            // Assert
            Assert.False(result.IsSuccess);
        }

        [Fact]
        public async Task EvaluateRule_WithInvalidOperator_ThrowsException()
        {
            // Arrange
            var rule = new Rule(
                name: "Test Rule",
                description: "Test Description",
                entityName: "account",
                createdBy: "TestUser",
                environment: "Test",
                rootCondition: new RuleCondition
                {
                    FieldName = "name",
                    Operator = "INVALID_OPERATOR",
                    FieldType = "STRING",
                    Value = "Test Account"
                }
            );

            var entityData = new Dictionary<string, object>
            {
                { "name", "Test Account" }
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _ruleEvaluator.EvaluateRuleAsync(rule, entityData));
        }

        [Fact]
        public async Task EvaluateRule_WithNullRule_ThrowsException()
        {
            // Arrange
            var entityData = new Dictionary<string, object>
            {
                { "name", "Test Account" }
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _ruleEvaluator.EvaluateRuleAsync(null!, entityData));
        }

        [Fact]
        public async Task EvaluateRule_WithNullEntityData_ThrowsException()
        {
            // Arrange
            var rule = new Rule(
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
                    Value = "Test Account"
                }
            );

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _ruleEvaluator.EvaluateRuleAsync(rule, null!));
        }
    }
} 