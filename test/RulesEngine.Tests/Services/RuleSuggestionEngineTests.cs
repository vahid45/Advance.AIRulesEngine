using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ML;
using Microsoft.ML.Data;
using RulesEngine.Core.Models;
using RulesEngine.ML;
using Xunit;

namespace RulesEngine.Tests.Services
{
    public class RuleSuggestionEngineTests
    {
        private readonly MLContext _mlContext;
        private readonly ITransformer _model;

        public RuleSuggestionEngineTests()
        {
            _mlContext = new MLContext();
            _model = CreateTestModel();
        }

        [Fact]
        public async Task SuggestRuleAsync_WithValidData_ReturnsValidSuggestion()
        {
            // Arrange
            var engine = new RuleSuggestionEngine(_model);
            var entityData = new EntityData
            {
                EntityName = "SupportTicket",
                FieldName = "Priority",
                FieldType = "OPTIONSET",
                FieldValue = 1.0f,
                Outcome = "High"
            };

            // Act
            var suggestion = await engine.SuggestRuleAsync(entityData);

            // Assert
            Assert.NotNull(suggestion);
            Assert.Equal("SupportTicket", suggestion.EntityName);
            Assert.NotNull(suggestion.FieldName);
            Assert.NotNull(suggestion.Operator);
            Assert.NotNull(suggestion.Value);
            Assert.True(suggestion.Confidence >= 0 && suggestion.Confidence <= 1);
        }

        [Fact]
        public void CreateRuleFromSuggestion_WithValidSuggestion_ReturnsValidRule()
        {
            // Arrange
            var engine = new RuleSuggestionEngine(_model);
            var suggestion = new RuleSuggestion
            {
                EntityName = "SupportTicket",
                FieldName = "Priority",
                FieldType = "OPTIONSET",
                Operator = "EQUALS",
                Value = "High",
                Confidence = 0.85f,
                PredictedOutcome = "Escalate"
            };

            // Act
            var rule = engine.CreateRuleFromSuggestion(suggestion);

            // Assert
            Assert.NotNull(rule);
            Assert.Equal("Suggested Rule for SupportTicket", rule.Name);
            Assert.Equal("SupportTicket", rule.EntityName);
            Assert.Equal("AI", rule.CreatedBy);
            Assert.Equal("Development", rule.Environment);
            Assert.NotNull(rule.RootCondition);
            Assert.Equal("Priority", rule.RootCondition.FieldName);
            Assert.Equal("EQUALS", rule.RootCondition.Operator);
            Assert.Equal("OPTIONSET", rule.RootCondition.FieldType);
            Assert.Equal("High", rule.RootCondition.Value);
        }

        [Fact]
        public void TrainModel_WithValidData_DoesNotThrowException()
        {
            // Arrange
            var engine = new RuleSuggestionEngine(_model);
            var trainingData = new List<EntityData>
            {
                new()
                {
                    EntityName = "SupportTicket",
                    FieldName = "Priority",
                    FieldType = "OPTIONSET",
                    FieldValue = 1.0f,
                    Outcome = "High"
                },
                new()
                {
                    EntityName = "SupportTicket",
                    FieldName = "Priority",
                    FieldType = "OPTIONSET",
                    FieldValue = 2.0f,
                    Outcome = "Medium"
                }
            };

            // Act & Assert
            var exception = Record.Exception(() => engine.TrainModel(trainingData));
            Assert.Null(exception);
        }

        [Fact]
        public void SuggestRule_WithHistoricalData_ReturnsValidRule()
        {
            // Arrange
            var engine = new RuleSuggestionEngine(_model);
            var historicalData = new List<EntityData>
            {
                new()
                {
                    EntityName = "SupportTicket",
                    FieldName = "Priority",
                    FieldType = "OPTIONSET",
                    FieldValue = 1.0f,
                    Outcome = "High"
                }
            };

            // Act
            var rule = engine.SuggestRule("SupportTicket", historicalData);

            // Assert
            Assert.NotNull(rule);
            Assert.Equal("Suggested Rule for SupportTicket", rule.Name);
            Assert.Equal("SupportTicket", rule.EntityName);
            Assert.Equal("AI", rule.CreatedBy);
            Assert.Equal("Development", rule.Environment);
            Assert.NotNull(rule.RootCondition);
        }

        [Fact]
        public void Constructor_WithNullModel_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new RuleSuggestionEngine(null));
        }

        private ITransformer CreateTestModel()
        {
            var data = new List<EntityData>
            {
                new()
                {
                    EntityName = "SupportTicket",
                    FieldName = "Priority",
                    FieldType = "OPTIONSET",
                    FieldValue = 1.0f,
                    Outcome = "High"
                },
                new()
                {
                    EntityName = "SupportTicket",
                    FieldName = "Priority",
                    FieldType = "OPTIONSET",
                    FieldValue = 2.0f,
                    Outcome = "Medium"
                }
            };

            var dataView = _mlContext.Data.LoadFromEnumerable(data);

            var pipeline = _mlContext.Transforms.Conversion.MapValueToKey("Label", "Outcome")
                .Append(_mlContext.Transforms.Text.FeaturizeText("EntityNameFeaturized", "EntityName"))
                .Append(_mlContext.Transforms.Text.FeaturizeText("FieldNameFeaturized", "FieldName"))
                .Append(_mlContext.Transforms.Concatenate("Features", 
                    "EntityNameFeaturized", "FieldNameFeaturized", "FieldValue"))
                .Append(_mlContext.MulticlassClassification.Trainers.SdcaNonCalibrated())
                .Append(_mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));

            return pipeline.Fit(dataView);
        }
    }
} 