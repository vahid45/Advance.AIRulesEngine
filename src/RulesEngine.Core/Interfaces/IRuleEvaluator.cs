using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RulesEngine.Core.Models;

namespace RulesEngine.Core.Interfaces
{
    public interface IRuleEvaluator
    {
        Task<RuleEvaluationResult> EvaluateRuleAsync(Rule rule, Dictionary<string, object> entityData);
        Task<bool> EvaluateConditionAsync(RuleCondition condition, Dictionary<string, object> entityData);
    }
} 