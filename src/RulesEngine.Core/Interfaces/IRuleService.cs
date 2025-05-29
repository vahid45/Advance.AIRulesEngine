using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RulesEngine.Core.Models;

namespace RulesEngine.Core.Interfaces
{
    public interface IRuleService
    {
        Task<Rule> CreateRuleAsync(Rule rule);
        Task<Rule> UpdateRuleAsync(Rule rule);
        Task DeleteRuleAsync(Guid id);
        Task<Rule?> GetRuleAsync(Guid id);
        Task<IEnumerable<Rule>> GetRulesByEntityAsync(string entityName);
        Task<bool> EvaluateRuleAsync(Guid ruleId, Dictionary<string, object> entityData);
        ValidationResult ValidateRule(Rule rule);
    }
} 