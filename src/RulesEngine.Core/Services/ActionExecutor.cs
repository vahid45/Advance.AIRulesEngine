using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Crm.Sdk.Messages;
using RulesEngine.Core.Interfaces;
using RulesEngine.Core.Models;

namespace RulesEngine.Core.Services
{
    public class ActionExecutor : IActionExecutor
    {
        private readonly ServiceClient _dataverseClient;

        public ActionExecutor(ServiceClient dataverseClient)
        {
            _dataverseClient = dataverseClient ?? throw new ArgumentNullException(nameof(dataverseClient));
        }

        public async Task<bool> ExecuteActionAsync(RuleAction action, Dictionary<string, object> entityData)
        {
            try
            {
                // Validate the action first
                var isValid = await ValidateActionAsync(action, entityData);
                if (!isValid)
                    return false;

                // Execute the action based on its type
                switch (action.ActionType.ToUpper())
                {
                    case "UPDATE_FIELD":
                        return await UpdateFieldAsync(action, entityData);
                    case "CREATE_RECORD":
                        return await CreateRecordAsync(action, entityData);
                    case "UPDATE_RECORD":
                        return await UpdateRecordAsync(action, entityData);
                    case "DELETE_RECORD":
                        return await DeleteRecordAsync(action, entityData);
                    case "SEND_EMAIL":
                        return await SendEmailAsync(action, entityData);
                    case "START_WORKFLOW":
                        return await StartWorkflowAsync(action, entityData);
                    case "SET_STATUS":
                        return await SetStatusAsync(action, entityData);
                    case "ASSIGN_RECORD":
                        return await AssignRecordAsync(action, entityData);
                    case "SHARE_RECORD":
                        return await ShareRecordAsync(action, entityData);
                    default:
                        throw new ArgumentException($"Unsupported action type: {action.ActionType}");
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> ValidateActionAsync(RuleAction action, Dictionary<string, object> entityData)
        {
            if (action == null)
                return false;

            if (string.IsNullOrWhiteSpace(action.ActionType))
                return false;

            // Validate action-specific requirements
            return action.ActionType.ToUpper() switch
            {
                "UPDATE_FIELD" => await ValidateUpdateFieldAsync(action),
                "CREATE_RECORD" => await ValidateCreateRecordAsync(action),
                "UPDATE_RECORD" => await ValidateUpdateRecordAsync(action),
                "DELETE_RECORD" => await ValidateDeleteRecordAsync(action),
                "SEND_EMAIL" => await ValidateSendEmailAsync(action),
                "START_WORKFLOW" => await ValidateStartWorkflowAsync(action),
                "SET_STATUS" => await ValidateSetStatusAsync(action),
                "ASSIGN_RECORD" => await ValidateAssignRecordAsync(action),
                "SHARE_RECORD" => await ValidateShareRecordAsync(action),
                _ => false
            };
        }

        private async Task<bool> ValidateUpdateFieldAsync(RuleAction action)
        {
            if (string.IsNullOrWhiteSpace(action.TargetEntity) ||
                string.IsNullOrWhiteSpace(action.TargetField) ||
                action.Value == null)
                return false;

            return await Task.FromResult(true);
        }

        private async Task<bool> ValidateCreateRecordAsync(RuleAction action)
        {
            if (string.IsNullOrWhiteSpace(action.TargetEntity) ||
                action.Parameters == null ||
                !action.Parameters.Any())
                return false;

            return await Task.FromResult(true);
        }

        private async Task<bool> ValidateUpdateRecordAsync(RuleAction action)
        {
            if (string.IsNullOrWhiteSpace(action.TargetEntity) ||
                action.Parameters == null ||
                !action.Parameters.Any())
                return false;

            return await Task.FromResult(true);
        }

        private async Task<bool> ValidateDeleteRecordAsync(RuleAction action)
        {
            if (string.IsNullOrWhiteSpace(action.TargetEntity) ||
                !action.Parameters.ContainsKey("recordId"))
                return false;

            return await Task.FromResult(true);
        }

        private async Task<bool> ValidateSendEmailAsync(RuleAction action)
        {
            if (!action.Parameters.ContainsKey("to") ||
                !action.Parameters.ContainsKey("subject"))
                return false;

            return await Task.FromResult(true);
        }

        private async Task<bool> ValidateStartWorkflowAsync(RuleAction action)
        {
            if (!action.Parameters.ContainsKey("workflowId"))
                return false;

            return await Task.FromResult(true);
        }

        private async Task<bool> ValidateSetStatusAsync(RuleAction action)
        {
            if (!action.Parameters.ContainsKey("status"))
                return false;

            return await Task.FromResult(true);
        }

        private async Task<bool> ValidateAssignRecordAsync(RuleAction action)
        {
            if (!action.Parameters.ContainsKey("assigneeId"))
                return false;

            return await Task.FromResult(true);
        }

        private async Task<bool> ValidateShareRecordAsync(RuleAction action)
        {
            if (!action.Parameters.ContainsKey("userId"))
                return false;

            return await Task.FromResult(true);
        }

        private async Task<bool> UpdateFieldAsync(RuleAction action, Dictionary<string, object> entityData)
        {
            if (!entityData.TryGetValue("id", out var id) || id == null)
                throw new ArgumentException("Entity ID is required for update action");

            var entity = new Entity(action.TargetEntity)
            {
                Id = new Guid(id.ToString() ?? throw new InvalidOperationException("Invalid entity ID")),
                [action.TargetField] = action.Value
            };

            await _dataverseClient.UpdateAsync(entity);
            return true;
        }

        private async Task<bool> CreateRecordAsync(RuleAction action, Dictionary<string, object> entityData)
        {
            var entity = new Entity(action.TargetEntity);
            
            // Map fields from parameters
            foreach (var param in action.Parameters)
            {
                entity[param.Key] = param.Value;
            }

            await _dataverseClient.CreateAsync(entity);
            return true;
        }

        private async Task<bool> UpdateRecordAsync(RuleAction action, Dictionary<string, object> entityData)
        {
            if (!action.Parameters.TryGetValue("recordId", out var recordId))
                throw new ArgumentException("Record ID is required for update action");

            var entity = new Entity(action.TargetEntity)
            {
                Id = new Guid(recordId.ToString() ?? throw new InvalidOperationException("Invalid record ID"))
            };

            foreach (var param in action.Parameters)
            {
                if (param.Key != "recordId")
                {
                    entity[param.Key] = param.Value;
                }
            }

            await _dataverseClient.UpdateAsync(entity);
            return true;
        }

        private async Task<bool> DeleteRecordAsync(RuleAction action, Dictionary<string, object> entityData)
        {
            if (!action.Parameters.TryGetValue("recordId", out var recordId))
                throw new ArgumentException("Record ID is required for delete action");

            await _dataverseClient.DeleteAsync(
                action.TargetEntity,
                new Guid(recordId.ToString() ?? throw new InvalidOperationException("Invalid record ID")));
            return true;
        }

        private async Task<bool> SendEmailAsync(RuleAction action, Dictionary<string, object> entityData)
        {
            var email = new Entity("email")
            {
                ["subject"] = action.Parameters.GetValueOrDefault("subject", "Rule Engine Notification"),
                ["body"] = action.Parameters.GetValueOrDefault("body", string.Empty),
                ["to"] = action.Parameters.GetValueOrDefault("to", string.Empty),
                ["from"] = action.Parameters.GetValueOrDefault("from", string.Empty)
            };

            await _dataverseClient.CreateAsync(email);
            return true;
        }

        private async Task<bool> StartWorkflowAsync(RuleAction action, Dictionary<string, object> entityData)
        {
            if (!action.Parameters.TryGetValue("workflowId", out var workflowId))
                throw new ArgumentException("Workflow ID is required");

            if (!entityData.TryGetValue("id", out var id) || id == null)
                throw new ArgumentException("Entity ID is required for workflow action");

            var request = new ExecuteWorkflowRequest
            {
                WorkflowId = new Guid(workflowId.ToString() ?? throw new InvalidOperationException("Invalid workflow ID")),
                EntityId = new Guid(id.ToString() ?? throw new InvalidOperationException("Invalid entity ID"))
            };

            await _dataverseClient.ExecuteAsync(request);
            return true;
        }

        private async Task<bool> SetStatusAsync(RuleAction action, Dictionary<string, object> entityData)
        {
            if (!action.Parameters.TryGetValue("status", out var status))
                throw new ArgumentException("Status is required");

            if (!entityData.TryGetValue("id", out var id) || id == null)
                throw new ArgumentException("Entity ID is required for status update");

            var request = new SetStateRequest
            {
                EntityMoniker = new EntityReference(action.TargetEntity, new Guid(id.ToString() ?? throw new InvalidOperationException("Invalid entity ID"))),
                State = new OptionSetValue(int.Parse(status.ToString() ?? throw new InvalidOperationException("Invalid status value"))),
                Status = action.Parameters.TryGetValue("statusCode", out var statusCode) 
                    ? new OptionSetValue(int.Parse(statusCode.ToString() ?? throw new InvalidOperationException("Invalid status code value")))
                    : null
            };

            await _dataverseClient.ExecuteAsync(request);
            return true;
        }

        private async Task<bool> AssignRecordAsync(RuleAction action, Dictionary<string, object> entityData)
        {
            if (!action.Parameters.TryGetValue("assigneeId", out var assigneeId))
                throw new ArgumentException("Assignee ID is required");

            if (!entityData.TryGetValue("id", out var id) || id == null)
                throw new ArgumentException("Entity ID is required for assignment");

            var request = new AssignRequest
            {
                Assignee = new EntityReference("systemuser", new Guid(assigneeId.ToString() ?? throw new InvalidOperationException("Invalid assignee ID"))),
                Target = new EntityReference(action.TargetEntity, new Guid(id.ToString() ?? throw new InvalidOperationException("Invalid entity ID")))
            };

            await _dataverseClient.ExecuteAsync(request);
            return true;
        }

        private async Task<bool> ShareRecordAsync(RuleAction action, Dictionary<string, object> entityData)
        {
            if (!action.Parameters.TryGetValue("userId", out var userId))
                throw new ArgumentException("User ID is required");

            if (!entityData.TryGetValue("id", out var id) || id == null)
                throw new ArgumentException("Entity ID is required for sharing");

            var request = new GrantAccessRequest
            {
                PrincipalAccess = new PrincipalAccess
                {
                    Principal = new EntityReference("systemuser", new Guid(userId.ToString() ?? throw new InvalidOperationException("Invalid user ID"))),
                    AccessMask = AccessRights.ReadAccess
                },
                Target = new EntityReference(action.TargetEntity, new Guid(id.ToString() ?? throw new InvalidOperationException("Invalid entity ID")))
            };

            await _dataverseClient.ExecuteAsync(request);
            return true;
        }
    }
} 