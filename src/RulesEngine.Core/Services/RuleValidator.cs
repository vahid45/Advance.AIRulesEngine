using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using RulesEngine.Core.Interfaces;
using RulesEngine.Core.Models;

namespace RulesEngine.Core.Services
{
    public class RuleValidator : IRuleValidator
    {
        private static readonly HashSet<string> ValidOperators = new(StringComparer.OrdinalIgnoreCase)
        {
            // Logical operators
            "AND", "OR", "NOT", "XOR",
            // Comparison operators
            "EQUALS", "NOT_EQUALS", "GREATER_THAN", "LESS_THAN",
            "GREATER_THAN_OR_EQUALS", "LESS_THAN_OR_EQUALS",
            // String operators
            "CONTAINS", "NOT_CONTAINS", "STARTS_WITH", "ENDS_WITH",
            "REGEX_MATCH", "IS_EMPTY", "IS_NOT_EMPTY",
            // Collection operators
            "IN", "NOT_IN", "CONTAINS_ALL", "CONTAINS_ANY",
            // Date operators
            "IS_TODAY", "IS_FUTURE", "IS_PAST", "IS_WITHIN_DAYS"
        };

        private static readonly HashSet<string> ValidActionTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "UPDATE_FIELD", "CREATE_RECORD", "UPDATE_RECORD", "DELETE_RECORD",
            "SEND_EMAIL", "START_WORKFLOW", "SET_STATUS", "ASSIGN_RECORD", "SHARE_RECORD"
        };

        private static readonly HashSet<string> ValidFieldTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "String", "Int", "Decimal", "Boolean", "DateTime", "Guid", "OptionSet", "Money", "Lookup"
        };

        private static readonly Dictionary<string, string> FieldTypeRegex = new()
        {
            { "Guid", @"^[0-9a-fA-F]{8}-([0-9a-fA-F]{4}-){3}[0-9a-fA-F]{12}$" },
            { "Email", @"^[^@\s]+@[^@\s]+\.[^@\s]+$" },
            { "Phone", @"^\+?[0-9]{10,15}$" }
        };

        public ValidationResult ValidateRule(Rule rule)
        {
            var result = new ValidationResult();

            if (rule == null)
            {
                result.IsValid = false;
                result.Errors.Add("Rule cannot be null");
                return result;
            }

            // Validate basic properties
            if (string.IsNullOrWhiteSpace(rule.Name))
            {
                result.IsValid = false;
                result.Errors.Add("Rule name is required");
            }

            if (string.IsNullOrWhiteSpace(rule.EntityName))
            {
                result.IsValid = false;
                result.Errors.Add("Entity name is required");
            }

            // Validate conditions
            if (rule.RootCondition == null)
            {
                result.IsValid = false;
                result.Errors.Add("Root condition is required");
            }
            else
            {
                ValidateCondition(rule.RootCondition, result);
            }

            // Validate actions
            if (rule.Actions != null)
            {
                foreach (var action in rule.Actions)
                {
                    if (string.IsNullOrWhiteSpace(action.ActionType))
                    {
                        result.AddError("Action type is required");
                    }

                    if (string.IsNullOrWhiteSpace(action.TargetEntity))
                    {
                        result.AddError("Target entity is required for action");
                    }

                    if (action.Parameters == null)
                    {
                        result.AddError("Action parameters cannot be null");
                    }
                }
            }

            return result;
        }

        private void ValidateCondition(RuleCondition condition, ValidationResult result)
        {
            if (condition == null)
            {
                result.IsValid = false;
                result.Errors.Add("Condition cannot be null");
                return;
            }

            // Validate field name
            if (string.IsNullOrWhiteSpace(condition.FieldName))
            {
                result.IsValid = false;
                result.Errors.Add("Field name is required for condition");
            }

            // Validate operator
            if (string.IsNullOrWhiteSpace(condition.Operator))
            {
                result.IsValid = false;
                result.Errors.Add("Operator is required for condition");
            }

            // Validate field type
            if (string.IsNullOrWhiteSpace(condition.FieldType))
            {
                result.IsValid = false;
                result.Errors.Add("Field type is required for condition");
            }

            // Validate value
            if (condition.Value == null)
            {
                result.IsValid = false;
                result.Errors.Add("Value is required for condition");
            }

            // Validate sub-conditions
            if (condition.SubConditions != null)
            {
                foreach (var subCondition in condition.SubConditions)
                {
                    ValidateCondition(subCondition, result);
                }
            }
        }

        private void ValidateAction(RuleAction action, ValidationResult result)
        {
            if (action == null)
            {
                result.IsValid = false;
                result.Errors.Add("Action cannot be null");
                return;
            }

            // Validate action type
            if (string.IsNullOrWhiteSpace(action.ActionType))
            {
                result.IsValid = false;
                result.Errors.Add("Action type is required");
            }

            // Validate target entity
            if (string.IsNullOrWhiteSpace(action.TargetEntity))
            {
                result.IsValid = false;
                result.Errors.Add("Target entity is required for action");
            }

            // Validate target field for field update actions
            if (action.ActionType.Equals("UPDATE_FIELD", StringComparison.OrdinalIgnoreCase) &&
                string.IsNullOrWhiteSpace(action.TargetField))
            {
                result.IsValid = false;
                result.Errors.Add("Target field is required for field update action");
            }

            // Validate value for field update actions
            if (action.ActionType.Equals("UPDATE_FIELD", StringComparison.OrdinalIgnoreCase) &&
                action.Value == null)
            {
                result.IsValid = false;
                result.Errors.Add("Value is required for field update action");
            }

            // Validate parameters
            if (action.Parameters == null)
            {
                result.IsValid = false;
                result.Errors.Add("Parameters dictionary cannot be null");
            }
        }

        private void ValidateOperatorValue(RuleCondition condition, ValidationResult result)
        {
            switch (condition.Operator.ToUpper())
            {
                case "REGEX_MATCH":
                    try
                    {
                        _ = new Regex(condition.Value?.ToString() ?? string.Empty);
                    }
                    catch
                    {
                        result.AddError("Invalid regular expression pattern");
                    }
                    break;

                case "IS_WITHIN_DAYS":
                    if (!int.TryParse(condition.Value?.ToString(), out int days) || days < 0)
                        result.AddError("Days value must be a positive integer");
                    break;

                case "IN":
                case "NOT_IN":
                case "CONTAINS_ALL":
                case "CONTAINS_ANY":
                    if (condition.Value == null)
                    {
                        result.AddError($"{condition.Operator} operator requires a non-null value");
                        break;
                    }

                    var valueType = condition.Value.GetType();
                    if (!valueType.IsArray && 
                        !typeof(IEnumerable<object>).IsAssignableFrom(valueType) && 
                        valueType != typeof(string))
                    {
                        result.AddError($"{condition.Operator} operator requires a collection value");
                    }
                    break;
            }
        }

        private void ValidateFieldValue(string fieldType, object value, ValidationResult result)
        {
            if (value == null)
            {
                result.AddError("Field value cannot be null");
                return;
            }

            var stringValue = value.ToString();
            if (stringValue == null)
            {
                result.AddError("Field value cannot be converted to string");
                return;
            }

            switch (fieldType.ToUpper())
            {
                case "STRING":
                    if (stringValue.Length > 4000)
                        result.AddError("String value cannot exceed 4000 characters");
                    break;

                case "INT":
                    if (!int.TryParse(stringValue, out _))
                        result.AddError("Value must be a valid integer");
                    break;

                case "DECIMAL":
                    if (!decimal.TryParse(stringValue, out _))
                        result.AddError("Value must be a valid decimal number");
                    break;

                case "BOOLEAN":
                    if (!bool.TryParse(stringValue, out _))
                        result.AddError("Value must be a valid boolean");
                    break;

                case "DATETIME":
                    if (!DateTime.TryParse(stringValue, out _))
                        result.AddError("Value must be a valid date/time");
                    break;

                case "GUID":
                    if (!Guid.TryParse(stringValue, out _))
                        result.AddError("Value must be a valid GUID");
                    break;

                case "OPTIONSET":
                    if (!int.TryParse(stringValue, out _))
                        result.AddError("Value must be a valid option set value");
                    break;

                case "MONEY":
                    if (!decimal.TryParse(stringValue, out _))
                        result.AddError("Value must be a valid money amount");
                    break;

                case "LOOKUP":
                    if (!Guid.TryParse(stringValue, out _))
                        result.AddError("Value must be a valid lookup ID");
                    break;

                default:
                    result.AddError($"Unsupported field type: {fieldType}");
                    break;
            }
        }

        private static bool IsValidEntityName(string name)
        {
            return Regex.IsMatch(name, @"^[a-zA-Z_][a-zA-Z0-9_]*$");
        }

        private static bool IsValidFieldName(string name)
        {
            return Regex.IsMatch(name, @"^[a-zA-Z_][a-zA-Z0-9_]*$");
        }

        private static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
                return regex.IsMatch(email);
            }
            catch
            {
                return false;
            }
        }
    }
} 