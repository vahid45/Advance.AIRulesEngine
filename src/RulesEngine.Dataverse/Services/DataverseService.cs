using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using RulesEngine.Core.Interfaces;
using RulesEngine.Core.Models;

namespace RulesEngine.Dataverse.Services
{
    public class DataverseService : IDataverseService
    {
        private readonly ServiceClient _dataverseClient;
        private const string RuleEntityName = "adv_rule";

        public DataverseService(ServiceClient dataverseClient)
        {
            _dataverseClient = dataverseClient ?? throw new ArgumentNullException(nameof(dataverseClient));
        }

        public async Task<IEnumerable<Rule>> GetRulesAsync(string? entityName = null)
        {
            var query = new QueryExpression("adv_rule")
            {
                ColumnSet = new ColumnSet(true)
            };

            if (!string.IsNullOrEmpty(entityName))
            {
                query.Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression("adv_entityname", ConditionOperator.Equal, entityName)
                    }
                };
            }

            var result = await _dataverseClient.RetrieveMultipleAsync(query);
            return result.Entities.Select(e => MapToRule(e));
        }

        public async Task<Rule?> GetRuleAsync(Guid id)
        {
            try
            {
                var entity = await _dataverseClient.RetrieveAsync(RuleEntityName, id, new ColumnSet(true));
                return entity != null ? MapToRule(entity) : null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<Rule> CreateRuleAsync(Rule rule)
        {
            try
            {
                var entity = MapToEntity(rule);
                var id = await _dataverseClient.CreateAsync(entity);
                return await GetRuleAsync(id) ?? throw new InvalidOperationException("Failed to retrieve created rule");
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task UpdateRuleAsync(Rule rule)
        {
            try
            {
                var entity = MapToEntity(rule);
                await _dataverseClient.UpdateAsync(entity);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task DeleteRuleAsync(Guid ruleId)
        {
            try
            {
                await _dataverseClient.DeleteAsync(RuleEntityName, ruleId);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<object> GetEntityAsync(string entityName, Guid id)
        {
            var entity = await _dataverseClient.RetrieveAsync(entityName, id, new ColumnSet(true));
            if (entity == null)
            {
                throw new InvalidOperationException($"Entity {entityName} with id {id} not found");
            }
            return entity;
        }

        public async Task<IEnumerable<object>> QueryAsync(string entityName, string? filter = null, int? top = null)
        {
            var query = new QueryExpression(entityName)
            {
                ColumnSet = new ColumnSet(true)
            };

            if (!string.IsNullOrEmpty(filter))
            {
                // Parse the filter string into a FilterExpression
                // Example filter format: "fieldname operator value"
                var parts = filter.Split(' ', 3);
                if (parts.Length == 3)
                {
                    var fieldName = parts[0];
                    var operatorStr = parts[1];
                    var value = parts[2];

                    if (Enum.TryParse<ConditionOperator>(operatorStr, true, out var conditionOperator))
                    {
                        query.Criteria = new FilterExpression
                        {
                            FilterOperator = LogicalOperator.And,
                            Conditions = 
                            {
                                new ConditionExpression(fieldName, conditionOperator, value)
                            }
                        };
                    }
                }
            }

            if (top.HasValue)
            {
                query.TopCount = top.Value;
            }

            var response = await _dataverseClient.RetrieveMultipleAsync(query);
            return response.Entities;
        }

        private static Rule MapToRule(Entity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            var rule = new Rule(
                name: entity.GetAttributeValue<string>("adv_name") ?? string.Empty,
                description: entity.GetAttributeValue<string>("adv_description") ?? string.Empty,
                entityName: entity.GetAttributeValue<string>("adv_entityname") ?? string.Empty,
                createdBy: entity.GetAttributeValue<string>("adv_createdby") ?? string.Empty,
                environment: entity.GetAttributeValue<string>("adv_environment") ?? string.Empty,
                rootCondition: MapToCondition(entity.GetAttributeValue<string>("adv_rootcondition") ?? "{}")
            );

            rule.Id = entity.Id;
            rule.Version = entity.GetAttributeValue<int>("adv_version");
            rule.IsActive = entity.GetAttributeValue<bool>("adv_isactive");
            rule.CreatedDate = entity.GetAttributeValue<DateTime>("adv_createddate");
            rule.ModifiedDate = entity.GetAttributeValue<DateTime?>("adv_modifieddate");

            return rule;
        }

        private static Entity MapToEntity(Rule rule)
        {
            if (rule == null)
            {
                throw new ArgumentNullException(nameof(rule));
            }

            var entity = new Entity(RuleEntityName)
            {
                ["adv_name"] = rule.Name,
                ["adv_description"] = rule.Description,
                ["adv_entityname"] = rule.EntityName,
                ["adv_createdby"] = rule.CreatedBy,
                ["adv_environment"] = rule.Environment,
                ["adv_rootcondition"] = MapToJson(rule.RootCondition),
                ["adv_version"] = rule.Version,
                ["adv_isactive"] = rule.IsActive
            };

            if (rule.Id != Guid.Empty)
                entity.Id = rule.Id;

            return entity;
        }

        private static RuleCondition MapToCondition(string json)
        {
            // Implementation for deserializing JSON to RuleCondition
            throw new NotImplementedException();
        }

        private static string MapToJson(RuleCondition condition)
        {
            // Implementation for serializing RuleCondition to JSON
            throw new NotImplementedException();
        }
    }
} 