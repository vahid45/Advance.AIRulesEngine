using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using RulesEngine.Core.Interfaces;
using RulesEngine.Core.Models;
using RulesEngine.Core.Services;
using RulesEngine.Dataverse.Services;

namespace RulesEngine.Tests.Integration
{
    public class RuleEngineIntegrationTests : IClassFixture<TestFixture>
    {
        private readonly IRuleService _ruleService;
        private readonly IActionExecutor _actionExecutor;
        private readonly IDataverseService _dataverseService;

        public RuleEngineIntegrationTests(TestFixture fixture)
        {
            _ruleService = fixture.ServiceProvider.GetRequiredService<IRuleService>();
            _actionExecutor = fixture.ServiceProvider.GetRequiredService<IActionExecutor>();
            _dataverseService = fixture.ServiceProvider.GetRequiredService<IDataverseService>();
        }

        [Fact]
        public async Task CreateAndEvaluateRule_IntegrationTest()
        {
            // Arrange
            var rule = CreateTestRule();

            // Act
            var createdRule = await _ruleService.CreateRuleAsync(rule);
            var evaluationResult = await _ruleService.EvaluateRuleAsync(createdRule.Id, CreateTestData());

            // Assert
            Assert.NotNull(createdRule);
            Assert.Equal(rule.Name, createdRule.Name);
            Assert.True(evaluationResult);
        }

        [Fact]
        public async Task UpdateAndDeleteRule_IntegrationTest()
        {
            // Arrange
            var rule = CreateTestRule();
            var createdRule = await _ruleService.CreateRuleAsync(rule);
            createdRule.Name = "Updated Rule Name";

            // Act
            var updatedRule = await _ruleService.UpdateRuleAsync(createdRule);
            await _ruleService.DeleteRuleAsync(createdRule.Id);
            var deletedRule = await _ruleService.GetRuleAsync(createdRule.Id);

            // Assert
            Assert.NotNull(updatedRule);
            Assert.Equal("Updated Rule Name", updatedRule.Name);
            Assert.Null(deletedRule);
        }

        [Fact]
        public async Task ExecuteRuleAction_IntegrationTest()
        {
            // Arrange
            var rule = CreateTestRule();
            var createdRule = await _ruleService.CreateRuleAsync(rule);
            var entityData = CreateTestData();

            // Act
            var evaluationResult = await _ruleService.EvaluateRuleAsync(createdRule.Id, entityData);
            var actionResult = await _actionExecutor.ExecuteActionAsync(createdRule.Actions[0], entityData);

            // Assert
            Assert.True(evaluationResult);
            Assert.True(actionResult);
        }

        [Fact]
        public async Task GetRulesByEntity_IntegrationTest()
        {
            // Arrange
            var rule1 = CreateTestRule();
            var rule2 = CreateTestRule();
            rule2.Name = "Test Rule 2";
            await _ruleService.CreateRuleAsync(rule1);
            await _ruleService.CreateRuleAsync(rule2);

            // Act
            var rules = await _ruleService.GetRulesByEntityAsync("account");

            // Assert
            Assert.NotNull(rules);
            Assert.Contains(rules, r => r.Name == rule1.Name);
            Assert.Contains(rules, r => r.Name == rule2.Name);
        }

        [Fact]
        public async Task ValidateAndExecuteRule_IntegrationTest()
        {
            // Arrange
            var rule = CreateTestRule();
            var entityData = CreateTestData();

            // Act
            var validationResult = _ruleService.ValidateRule(rule);
            var createdRule = await _ruleService.CreateRuleAsync(rule);
            var evaluationResult = await _ruleService.EvaluateRuleAsync(createdRule.Id, entityData);
            var actionResult = await _actionExecutor.ExecuteActionAsync(createdRule.Actions[0], entityData);

            // Assert
            Assert.True(validationResult.IsValid);
            Assert.True(evaluationResult);
            Assert.True(actionResult);
        }

        [Fact]
        public async Task HandleInvalidRule_IntegrationTest()
        {
            // Arrange
            var rule = CreateTestRule();
            rule.Name = string.Empty;

            // Act & Assert
            var validationResult = _ruleService.ValidateRule(rule);
            Assert.False(validationResult.IsValid);
            await Assert.ThrowsAsync<ArgumentException>(() => _ruleService.CreateRuleAsync(rule));
        }

        [Fact]
        public async Task HandleInvalidAction_IntegrationTest()
        {
            // Arrange
            var rule = CreateTestRule();
            rule.Actions[0].ActionType = "INVALID_ACTION";
            var entityData = CreateTestData();

            // Act & Assert
            var validationResult = _ruleService.ValidateRule(rule);
            Assert.False(validationResult.IsValid);
            await Assert.ThrowsAsync<ArgumentException>(() => _ruleService.CreateRuleAsync(rule));
        }

        [Fact]
        public async Task HandleConcurrentRuleExecution_IntegrationTest()
        {
            // Arrange
            var rule = CreateTestRule();
            var createdRule = await _ruleService.CreateRuleAsync(rule);
            var entityData = CreateTestData();
            var tasks = new List<Task<bool>>();

            // Act
            for (int i = 0; i < 10; i++)
            {
                tasks.Add(_ruleService.EvaluateRuleAsync(createdRule.Id, entityData));
            }
            var results = await Task.WhenAll(tasks);

            // Assert
            Assert.All(results, result => Assert.True(result));
        }

        private static Rule CreateTestRule()
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
                    Value = "Test Account"
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
                        Value = "Updated by rule"
                    }
                }
            };
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
    }

    public class TestFixture : IDisposable
    {
        public IServiceProvider ServiceProvider { get; }

        public TestFixture()
        {
            var services = new ServiceCollection();
            ConfigureServices(services);
            ServiceProvider = services.BuildServiceProvider();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IRuleService, RuleService>();
            services.AddScoped<IActionExecutor, ActionExecutor>();
            services.AddScoped<IDataverseService, DataverseService>();
            services.AddScoped<RuleValidator>();
            services.AddScoped<RuleEvaluator>();
        }

        public void Dispose()
        {
            if (ServiceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
} 