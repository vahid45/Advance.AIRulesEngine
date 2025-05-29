using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RulesEngine.Core.Interfaces;
using RulesEngine.Core.Models;

namespace RulesEngine.Core.Services
{
    public class RuleEvaluator : IRuleEvaluator
    {
        private readonly IActionExecutor _actionExecutor;

        public RuleEvaluator(IActionExecutor actionExecutor)
        {
            _actionExecutor = actionExecutor ?? throw new ArgumentNullException(nameof(actionExecutor));
        }

        public async Task<RuleEvaluationResult> EvaluateRuleAsync(Rule rule, Dictionary<string, object> entityData)
        {
            try
            {
                if (rule == null)
                    throw new ArgumentNullException(nameof(rule));
                if (entityData == null)
                    throw new ArgumentNullException(nameof(entityData));

                if (!rule.IsActive)
                {
                    return new RuleEvaluationResult("Rule is not active")
                    {
                        IsSuccess = false
                    };
                }

                var conditionResult = await EvaluateConditionAsync(rule.RootCondition, entityData);
                var result = new RuleEvaluationResult(conditionResult ? "Rule conditions met" : "Rule conditions not met")
                {
                    IsSuccess = conditionResult
                };

                if (conditionResult)
                {
                    foreach (var action in rule.Actions)
                    {
                        try
                        {
                            var actionResult = await _actionExecutor.ExecuteActionAsync(action, entityData);
                            if (actionResult)
                            {
                                result.ExecutedActions.Add(action);
                            }
                            else
                            {
                                result.Error = new Exception("Action execution failed");
                                result.Message = "Error executing action";
                                break;
                            }
                        }
                        catch (Exception ex)
                        {
                            result.Error = ex;
                            result.Message = $"Error executing action: {ex.Message}";
                            break;
                        }
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                return new RuleEvaluationResult($"Error evaluating rule: {ex.Message}")
                {
                    IsSuccess = false,
                    Error = ex
                };
            }
        }

        public async Task<bool> EvaluateConditionAsync(RuleCondition condition, Dictionary<string, object> entityData)
        {
            if (condition == null)
                return false;

            // If there are sub-conditions, evaluate them based on the operator
            if (condition.SubConditions.Count > 0)
            {
                switch (condition.Operator.ToLower())
                {
                    case "and":
                        return condition.SubConditions.All(c => EvaluateConditionAsync(c, entityData).Result);
                    case "or":
                        return condition.SubConditions.Any(c => EvaluateConditionAsync(c, entityData).Result);
                    case "not":
                        return !(await EvaluateConditionAsync(condition.SubConditions.First(), entityData));
                    case "xor":
                        var xorResults = await Task.WhenAll(condition.SubConditions.Select(c => EvaluateConditionAsync(c, entityData)));
                        return xorResults.Count(r => r) == 1;
                    default:
                        return false;
                }
            }

            // Evaluate single condition
            if (!entityData.TryGetValue(condition.FieldName, out var fieldValue))
                return false;

            return EvaluateSingleCondition(condition, fieldValue);
        }

        private bool EvaluateSingleCondition(RuleCondition condition, object fieldValue)
        {
            if (fieldValue == null || condition.Value == null)
                return false;

            var fieldValueStr = fieldValue.ToString() ?? string.Empty;
            var conditionValueStr = condition.Value.ToString() ?? string.Empty;

            switch (condition.Operator.ToLower())
            {
                case "equals":
                    return fieldValueStr == conditionValueStr;
                case "notequals":
                    return fieldValueStr != conditionValueStr;
                case "contains":
                    return fieldValueStr.Contains(conditionValueStr);
                case "startswith":
                    return fieldValueStr.StartsWith(conditionValueStr);
                case "endswith":
                    return fieldValueStr.EndsWith(conditionValueStr);
                case "greaterthan":
                    return CompareValues(fieldValue, condition.Value) > 0;
                case "lessthan":
                    return CompareValues(fieldValue, condition.Value) < 0;
                case "greaterthanorequal":
                    return CompareValues(fieldValue, condition.Value) >= 0;
                case "lessthanorequal":
                    return CompareValues(fieldValue, condition.Value) <= 0;
                default:
                    return false;
            }
        }

        private int CompareValues(object value1, object value2)
        {
            if (value1 is IComparable comparable1 && value2 is IComparable comparable2)
            {
                return comparable1.CompareTo(comparable2);
            }
            return string.Compare(value1.ToString(), value2.ToString(), StringComparison.Ordinal);
        }

        private static object? GetPropertyValue(object entity, string propertyName)
        {
            var property = entity.GetType().GetProperty(propertyName);
            return property?.GetValue(entity);
        }

        private static bool IsInCollection(object? value, object? collection)
        {
            if (value == null || collection == null)
                return false;

            if (collection is IEnumerable<object> enumerable)
                return enumerable.Contains(value);

            if (collection is string str)
                return str.Contains(value.ToString() ?? string.Empty, StringComparison.OrdinalIgnoreCase);

            return false;
        }

        private static bool ContainsAll(object? value, object? collection)
        {
            if (value == null || collection == null)
                return false;

            if (value is IEnumerable<object> valueCollection && collection is IEnumerable<object> targetCollection)
                return targetCollection.All(item => valueCollection.Contains(item));

            return false;
        }

        private static bool ContainsAny(object? value, object? collection)
        {
            if (value == null || collection == null)
                return false;

            if (value is IEnumerable<object> valueCollection && collection is IEnumerable<object> targetCollection)
                return targetCollection.Any(item => valueCollection.Contains(item));

            return false;
        }

        private static bool IsToday(object? value)
        {
            if (value == null)
                return false;

            if (value is DateTime date)
                return date.Date == DateTime.Today;

            if (DateTime.TryParse(value.ToString(), out DateTime parsedDate))
                return parsedDate.Date == DateTime.Today;

            return false;
        }

        private static bool IsFuture(object? value)
        {
            if (value == null)
                return false;

            if (value is DateTime date)
                return date > DateTime.Now;

            if (DateTime.TryParse(value.ToString(), out DateTime parsedDate))
                return parsedDate > DateTime.Now;

            return false;
        }

        private static bool IsPast(object? value)
        {
            if (value == null)
                return false;

            if (value is DateTime date)
                return date < DateTime.Now;

            if (DateTime.TryParse(value.ToString(), out DateTime parsedDate))
                return parsedDate < DateTime.Now;

            return false;
        }

        private static bool IsWithinDays(object? value, object? days)
        {
            if (value == null || days == null)
                return false;

            if (!int.TryParse(days.ToString(), out int daysValue))
                return false;

            if (value is DateTime date)
                return (date - DateTime.Now).TotalDays <= daysValue;

            if (DateTime.TryParse(value.ToString(), out DateTime parsedDate))
                return (parsedDate - DateTime.Now).TotalDays <= daysValue;

            return false;
        }

        private bool EvaluateCondition(RuleCondition condition, Dictionary<string, object> entityData)
        {
            if (condition == null)
                return false;

            if (string.IsNullOrWhiteSpace(condition.FieldName) || 
                string.IsNullOrWhiteSpace(condition.Operator) || 
                condition.Value == null)
                return false;

            if (!entityData.TryGetValue(condition.FieldName, out var fieldValue))
                return false;

            return condition.Operator.ToUpper() switch
            {
                "EQUALS" => Equals(fieldValue, condition.Value),
                "NOT_EQUALS" => !Equals(fieldValue, condition.Value),
                "GREATER_THAN" => CompareValues(fieldValue, condition.Value) > 0,
                "LESS_THAN" => CompareValues(fieldValue, condition.Value) < 0,
                "GREATER_THAN_OR_EQUALS" => CompareValues(fieldValue, condition.Value) >= 0,
                "LESS_THAN_OR_EQUALS" => CompareValues(fieldValue, condition.Value) <= 0,
                "CONTAINS" => fieldValue?.ToString()?.Contains(condition.Value.ToString() ?? string.Empty) ?? false,
                "NOT_CONTAINS" => !(fieldValue?.ToString()?.Contains(condition.Value.ToString() ?? string.Empty) ?? false),
                "STARTS_WITH" => fieldValue?.ToString()?.StartsWith(condition.Value.ToString() ?? string.Empty) ?? false,
                "ENDS_WITH" => fieldValue?.ToString()?.EndsWith(condition.Value.ToString() ?? string.Empty) ?? false,
                "IS_EMPTY" => string.IsNullOrEmpty(fieldValue?.ToString()),
                "IS_NOT_EMPTY" => !string.IsNullOrEmpty(fieldValue?.ToString()),
                "IN" => IsInCollection(fieldValue, condition.Value),
                "NOT_IN" => !IsInCollection(fieldValue, condition.Value),
                "CONTAINS_ALL" => ContainsAll(fieldValue, condition.Value),
                "CONTAINS_ANY" => ContainsAny(fieldValue, condition.Value),
                "IS_TODAY" => IsToday(fieldValue),
                "IS_FUTURE" => IsFuture(fieldValue),
                "IS_PAST" => IsPast(fieldValue),
                "IS_WITHIN_DAYS" => IsWithinDays(fieldValue, condition.Value),
                _ => false
            };
        }
    }
} 