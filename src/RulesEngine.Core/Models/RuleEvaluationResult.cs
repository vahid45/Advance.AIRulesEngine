using System;
using System.Collections.Generic;
using RulesEngine.Core.Models;

namespace RulesEngine.Core.Models
{
    public class RuleEvaluationResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public Exception? Error { get; set; }
        public List<RuleAction> ExecutedActions { get; set; } = new();
        public Dictionary<string, object> ExecutionContext { get; set; } = new();

        public RuleEvaluationResult(string message)
        {
            Message = message ?? throw new ArgumentNullException(nameof(message));
        }
    }
} 