using System.Collections.Generic;

namespace RulesEngine.Core.Models
{
    public class RuleAction
    {
        public string ActionType { get; set; } = string.Empty;
        public string TargetEntity { get; set; } = string.Empty;
        public string TargetField { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public Dictionary<string, object> Parameters { get; set; } = new();
    }
} 