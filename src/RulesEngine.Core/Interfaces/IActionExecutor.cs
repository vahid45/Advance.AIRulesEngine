using System.Threading.Tasks;
using RulesEngine.Core.Models;
using System.Collections.Generic;

namespace RulesEngine.Core.Interfaces
{
    public interface IActionExecutor
    {
        Task<bool> ExecuteActionAsync(RuleAction action, Dictionary<string, object> entityData);
        Task<bool> ValidateActionAsync(RuleAction action, Dictionary<string, object> entityData);
    }
} 