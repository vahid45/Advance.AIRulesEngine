using RulesEngine.Core.Models;

namespace RulesEngine.Core.Interfaces
{
    public interface IRuleValidator
    {
        ValidationResult ValidateRule(Rule rule);
    }
} 