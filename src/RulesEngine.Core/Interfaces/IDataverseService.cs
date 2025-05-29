using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RulesEngine.Core.Models;

namespace RulesEngine.Core.Interfaces
{
    public interface IDataverseService
    {
        Task<IEnumerable<Rule>> GetRulesAsync(string? entityName = null);
        Task<Rule?> GetRuleAsync(Guid id);
        Task<Rule> CreateRuleAsync(Rule rule);
        Task UpdateRuleAsync(Rule rule);
        Task DeleteRuleAsync(Guid ruleId);
        Task<object> GetEntityAsync(string entityName, Guid id);
        Task<IEnumerable<object>> QueryAsync(string entityName, string? filter = null, int? top = null);
    }
} 