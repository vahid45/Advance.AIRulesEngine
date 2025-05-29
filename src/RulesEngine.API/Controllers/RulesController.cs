using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RulesEngine.Core.Interfaces;
using RulesEngine.Core.Models;
using RulesEngine.Dataverse;
using RulesEngine.ML;
using RulesEngine.Dataverse.Services;

namespace RulesEngine.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RulesController : ControllerBase
    {
        private readonly IRuleEvaluator _ruleEvaluator;
        private readonly IDataverseService _dataverseService;

        public RulesController(IRuleEvaluator ruleEvaluator, IDataverseService dataverseService)
        {
            _ruleEvaluator = ruleEvaluator;
            _dataverseService = dataverseService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Rule>>> GetRules([FromQuery] string? entityName = null)
        {
            try
            {
                var rules = await _dataverseService.GetRulesAsync(entityName);
                return Ok(rules);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving rules: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Rule>> GetRule(Guid id)
        {
            try
            {
                var rule = await _dataverseService.GetRuleAsync(id);
                if (rule == null)
                    return NotFound();

                return Ok(rule);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving rule: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<ActionResult<Rule>> CreateRule(Rule rule)
        {
            try
            {
                rule.Id = Guid.NewGuid();
                rule.CreatedDate = DateTime.UtcNow;
                rule.Version = 1;

                var createdRule = await _dataverseService.CreateRuleAsync(rule);
                return CreatedAtAction(nameof(GetRule), new { id = createdRule.Id }, createdRule);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error creating rule: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRule(Guid id, Rule rule)
        {
            try
            {
                if (id != rule.Id)
                    return BadRequest("Rule ID mismatch");

                var existingRule = await _dataverseService.GetRuleAsync(id);
                if (existingRule == null)
                    return NotFound();

                rule.ModifiedDate = DateTime.UtcNow;
                rule.Version = existingRule.Version + 1;

                await _dataverseService.UpdateRuleAsync(rule);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error updating rule: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRule(Guid id)
        {
            try
            {
                var rule = await _dataverseService.GetRuleAsync(id);
                if (rule == null)
                    return NotFound();

                await _dataverseService.DeleteRuleAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error deleting rule: {ex.Message}");
            }
        }

        [HttpPost("{ruleId}/evaluate")]
        public async Task<IActionResult> EvaluateRule(Guid ruleId, [FromBody] Dictionary<string, object> entityData)
        {
            try
            {
                var rule = await _dataverseService.GetRuleAsync(ruleId);
                if (rule == null)
                    return NotFound();

                var result = await _ruleEvaluator.EvaluateRuleAsync(rule, entityData);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
} 