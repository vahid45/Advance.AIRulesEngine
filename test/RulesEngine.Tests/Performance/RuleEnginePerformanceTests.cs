using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using Xunit;
using RulesEngine.Core.Interfaces;
using RulesEngine.Core.Models;
using RulesEngine.Core.Services;

namespace RulesEngine.Tests.Performance
{
    public class RuleEnginePerformanceTests
    {
        private readonly IRuleEvaluator _ruleEvaluator;
        private readonly Mock<IActionExecutor> _actionExecutorMock;

        public RuleEnginePerformanceTests()
        {
            _actionExecutorMock = new Mock<IActionExecutor>();
            _ruleEvaluator = new RuleEvaluator(_actionExecutorMock.Object);
        }

        [Fact]
        public async Task EvaluateRule_PerformanceTest()
        {
            // Arrange
            var rule = CreateComplexRule();
            var entityData = CreateTestData();
            var stopwatch = new Stopwatch();
            var iterations = 1000;

            // Act
            stopwatch.Start();
            for (int i = 0; i < iterations; i++)
            {
                var result = await _ruleEvaluator.EvaluateRuleAsync(rule, entityData);
                Assert.True(result.IsSuccess);
            }
            stopwatch.Stop();

            // Assert
            var averageTime = stopwatch.ElapsedMilliseconds / (double)iterations;
            Assert.True(averageTime < 10, $"Average evaluation time ({averageTime}ms) exceeded 10ms threshold");
        }

        [Fact]
        public async Task EvaluateRule_ConcurrentPerformanceTest()
        {
            // Arrange
            var rule = CreateComplexRule();
            var entityData = CreateTestData();
            var stopwatch = new Stopwatch();
            var iterations = 1000;
            var tasks = new List<Task<RuleEvaluationResult>>();

            // Act
            stopwatch.Start();
            for (int i = 0; i < iterations; i++)
            {
                tasks.Add(_ruleEvaluator.EvaluateRuleAsync(rule, entityData));
            }
            var results = await Task.WhenAll(tasks);
            stopwatch.Stop();

            // Assert
            var averageTime = stopwatch.ElapsedMilliseconds / (double)iterations;
            Assert.True(averageTime < 10, $"Average concurrent evaluation time ({averageTime}ms) exceeded 10ms threshold");
            Assert.All(results, result => Assert.True(result.IsSuccess));
        }

        [Fact]
        public async Task EvaluateMultipleRules_PerformanceTest()
        {
            // Arrange
            var rules = CreateMultipleRules(100);
            var entityData = CreateTestData();
            var iterations = 100;
            var stopwatch = new Stopwatch();

            // Act
            stopwatch.Start();
            for (int i = 0; i < iterations; i++)
            {
                foreach (var rule in rules)
                {
                    var result = await _ruleEvaluator.EvaluateRuleAsync(rule, entityData);
                    Assert.True(result.IsSuccess);
                }
            }
            stopwatch.Stop();

            // Assert
            var totalRules = rules.Count * iterations;
            var averageTime = stopwatch.ElapsedMilliseconds / (double)totalRules;
            Assert.True(averageTime < 10, $"Average evaluation time per rule ({averageTime}ms) exceeded 10ms threshold");
        }

        [Fact]
        public async Task EvaluateRule_WithLargeDataSet_PerformanceTest()
        {
            // Arrange
            var rule = CreateComplexRule();
            var entityData = CreateLargeTestData(1000);
            var iterations = 10;
            var stopwatch = new Stopwatch();

            // Act
            stopwatch.Start();
            for (int i = 0; i < iterations; i++)
            {
                var result = await _ruleEvaluator.EvaluateRuleAsync(rule, entityData);
                Assert.True(result.IsSuccess);
            }
            stopwatch.Stop();

            // Assert
            var averageTime = stopwatch.ElapsedMilliseconds / (double)iterations;
            Assert.True(averageTime < 100, $"Average evaluation time with large dataset ({averageTime}ms) exceeded 100ms threshold");
        }

        [Fact]
        public async Task EvaluateRule_WithDeepNesting_PerformanceTest()
        {
            // Arrange
            var rule = CreateDeeplyNestedRule(10);
            var entityData = CreateTestData();
            var iterations = 100;
            var stopwatch = new Stopwatch();

            // Act
            stopwatch.Start();
            for (int i = 0; i < iterations; i++)
            {
                var result = await _ruleEvaluator.EvaluateRuleAsync(rule, entityData);
                Assert.True(result.IsSuccess);
            }
            stopwatch.Stop();

            // Assert
            var averageTime = stopwatch.ElapsedMilliseconds / (double)iterations;
            Assert.True(averageTime < 20, $"Average evaluation time with deep nesting ({averageTime}ms) exceeded 20ms threshold");
        }

        private static Rule CreateComplexRule()
        {
            return new Rule(
                name: "Complex Rule",
                description: "Test Description",
                entityName: "account",
                createdBy: "TestUser",
                environment: "Test",
                rootCondition: new RuleCondition
                {
                    Operator = "AND",
                    FieldType = "BOOLEAN",
                    SubConditions = new List<RuleCondition>
                    {
                        new()
                        {
                            FieldName = "name",
                            Operator = "EQUALS",
                            FieldType = "STRING",
                            Value = "Test Account"
                        },
                        new()
                        {
                            Operator = "OR",
                            FieldType = "BOOLEAN",
                            SubConditions = new List<RuleCondition>
                            {
                                new()
                                {
                                    FieldName = "status",
                                    Operator = "EQUALS",
                                    FieldType = "STRING",
                                    Value = "Active"
                                },
                                new()
                                {
                                    FieldName = "type",
                                    Operator = "EQUALS",
                                    FieldType = "STRING",
                                    Value = "Customer"
                                }
                            }
                        }
                    }
                }
            );
        }

        private static List<Rule> CreateMultipleRules(int count)
        {
            return Enumerable.Range(1, count)
                .Select(i => new Rule(
                    name: $"Rule {i}",
                    description: $"Test Description {i}",
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
                ))
                .ToList();
        }

        private static Dictionary<string, object> CreateTestData()
        {
            return new Dictionary<string, object>
            {
                { "name", "Test Account" },
                { "status", "Active" },
                { "type", "Customer" }
            };
        }

        private static Dictionary<string, object> CreateLargeTestData(int fieldCount)
        {
            var data = new Dictionary<string, object>();
            for (int i = 0; i < fieldCount; i++)
            {
                data[$"field{i}"] = $"value{i}";
            }
            return data;
        }

        private static Rule CreateDeeplyNestedRule(int depth)
        {
            var currentCondition = new RuleCondition
            {
                FieldName = "name",
                Operator = "EQUALS",
                FieldType = "STRING",
                Value = "Test Account"
            };

            for (int i = 0; i < depth; i++)
            {
                currentCondition = new RuleCondition
                {
                    Operator = "AND",
                    FieldType = "BOOLEAN",
                    SubConditions = new List<RuleCondition>
                    {
                        currentCondition,
                        new()
                        {
                            FieldName = $"field{i}",
                            Operator = "EQUALS",
                            FieldType = "STRING",
                            Value = $"value{i}"
                        }
                    }
                };
            }

            return new Rule(
                name: "Deeply Nested Rule",
                description: "Test Description",
                entityName: "account",
                createdBy: "TestUser",
                environment: "Test",
                rootCondition: currentCondition
            );
        }
    }
} 