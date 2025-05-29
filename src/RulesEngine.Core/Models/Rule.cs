using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace RulesEngine.Core.Models
{
    public class Rule
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string EntityName { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? ModifiedDate { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public string? ModifiedBy { get; set; }
        public string Environment { get; set; } = string.Empty;
        public int Version { get; set; } = 1;
        public RuleCondition RootCondition { get; set; } = new();
        public List<RuleAction> Actions { get; set; } = new();
        public Dictionary<string, string> Metadata { get; set; } = new();

        public Rule()
        {
            Id = Guid.NewGuid();
            CreatedDate = DateTime.UtcNow;
        }

        public Rule(string name, string description, string entityName, string createdBy, string environment, RuleCondition rootCondition)
            : this()
        {
            Name = name;
            Description = description;
            EntityName = entityName;
            CreatedBy = createdBy;
            Environment = environment;
            RootCondition = rootCondition;
        }
    }
} 