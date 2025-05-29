using System;
using System.Collections.Generic;

namespace RulesEngine.Core.Models
{
    public class RuleCondition
    {
        public string FieldName { get; set; } = string.Empty;
        public string Operator { get; set; } = string.Empty;
        public string FieldType { get; set; } = string.Empty;
        public object? Value { get; set; }
        public List<RuleCondition> SubConditions { get; set; } = new();

        public RuleCondition()
        {
        }

        public RuleCondition(string fieldName, string @operator, string fieldType, object? value)
        {
            FieldName = fieldName;
            Operator = @operator;
            FieldType = fieldType;
            Value = value;
        }
    }
} 