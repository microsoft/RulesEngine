﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Threading.Tasks;
using FluentValidation;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RulesEngine.Actions;
using RulesEngine.Enums;
using RulesEngine.Exceptions;
using RulesEngine.Interfaces;
using RulesEngine.Models;
using RulesEngine.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using RulesEngine.ExpressionBuilders;

namespace RulesEngine
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="RulesEngine.Interfaces.IRulesEngine" />
    public class RulesEngine : IRulesEngine
    {
        #region Variables
        private readonly ILogger _logger;
        private readonly ReSettings _reSettings;
        private readonly RulesCache _rulesCache = new RulesCache();
        private readonly MemoryCache _compiledParamsCache = new MemoryCache(new MemoryCacheOptions());
        private readonly ParamCompiler _ruleParamCompiler;
        private readonly RuleExpressionParser _ruleExpressionParser;
        private readonly RuleCompiler _ruleCompiler;
        private readonly ActionFactory _actionFactory;
        private const string ParamParseRegex = "(\\$\\(.*?\\))";
        #endregion

        #region Constructor
        public RulesEngine(string[] jsonConfig, ILogger logger, ReSettings reSettings = null) : this(logger, reSettings)
        {
            var workflowRules = jsonConfig.Select(item => JsonConvert.DeserializeObject<WorkflowRules>(item)).ToArray();
            AddWorkflow(workflowRules);
        }

        public RulesEngine(WorkflowRules[] workflowRules, ILogger logger = null, ReSettings reSettings = null) : this(logger, reSettings)
        {
            AddWorkflow(workflowRules);
        }

        public RulesEngine(ILogger logger = null, ReSettings reSettings = null)
        {
            _logger = logger ?? new NullLogger<RulesEngine>();
            _reSettings = reSettings ?? new ReSettings();
            _ruleExpressionParser = new RuleExpressionParser(_reSettings);
            _ruleParamCompiler = new ParamCompiler(_reSettings, _ruleExpressionParser);
            _ruleCompiler =  new RuleCompiler(new RuleExpressionBuilderFactory(_reSettings, _ruleExpressionParser),_logger);
            _actionFactory = new ActionFactory(GetActionRegistry(_reSettings));
        }

        private IDictionary<string,Func<ActionBase>> GetActionRegistry(ReSettings reSettings)
        {
            var actionDictionary = GetDefaultActionRegistry();
            var customActions = reSettings.CustomActions ?? new Dictionary<string, Func<ActionBase>>();
            foreach(var customAction in customActions){
                actionDictionary.Add(customAction);
            }
            return actionDictionary;

        }
        #endregion

        #region Public Methods

        /// <summary>
        /// This will execute all the rules of the specified workflow
        /// </summary>
        /// <param name="workflowName">The name of the workflow with rules to execute against the inputs</param>
        /// <param name="inputs">A variable number of inputs</param>
        /// <returns>List of rule results</returns>
        public async ValueTask<List<RuleResultTree>> ExecuteAllRulesAsync(string workflowName, params object[] inputs)
        {
            _logger.LogTrace($"Called {nameof(ExecuteAllRulesAsync)} for workflow {workflowName} and count of input {inputs.Count()}");

            var ruleParams = new List<RuleParameter>();

            for (int i = 0; i < inputs.Length; i++)
            {
                var input = inputs[i];
                ruleParams.Add(new RuleParameter($"input{i + 1}", input));
            }

            return await ExecuteAllRulesAsync(workflowName, ruleParams.ToArray());
        }

        /// <summary>
        /// This will execute all the rules of the specified workflow
        /// </summary>
        /// <param name="workflowName">The name of the workflow with rules to execute against the inputs</param>
        /// <param name="ruleParams">A variable number of rule parameters</param>
        /// <returns>List of rule results</returns>
        public async ValueTask<List<RuleResultTree>> ExecuteAllRulesAsync(string workflowName, params RuleParameter[] ruleParams)
        {
            var ruleResultList =  ValidateWorkflowAndExecuteRule(workflowName, ruleParams);
            foreach(var ruleResult in ruleResultList){
                var actionResult = await ExecuteActionForRuleResult(ruleResult,false);
                ruleResult.ActionResult = new ActionResult{
                    Output = actionResult.Output,
                    Exception = actionResult.Exception
                };
            }
            return ruleResultList;
        }


        public async ValueTask<ActionRuleResult> ExecuteActionWorkflowAsync(string workflowName, string ruleName, RuleParameter[] ruleParameters)
        {
            var compiledRule = CompileRule(workflowName, ruleName, ruleParameters);
            var resultTree = compiledRule(ruleParameters);
            return await ExecuteActionForRuleResult(resultTree,true);
        }

        private async ValueTask<ActionRuleResult> ExecuteActionForRuleResult(RuleResultTree resultTree, bool includeRuleResults=false)
        {
            ActionTriggerType triggerType = resultTree?.IsSuccess == true ? ActionTriggerType.onSuccess : ActionTriggerType.onFailure;

            if (resultTree?.Rule?.Actions != null && resultTree.Rule.Actions.ContainsKey(triggerType))
            {
                var actionInfo = resultTree.Rule.Actions[triggerType];
                var action = _actionFactory.Get(actionInfo.Name);
                var ruleParameters = resultTree.Inputs.Select(kv => new RuleParameter(kv.Key,kv.Value)).ToArray();
                return await action.ExecuteAndReturnResultAsync(new ActionContext(actionInfo.Context, resultTree), ruleParameters,includeRuleResults);
            }
            else
            {
                //If there is no action,return output as null and return the result for rule
                return new ActionRuleResult
                {
                    Output = null,
                    Results = includeRuleResults ? new List<RuleResultTree>() { resultTree }: null
                };
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Adds the workflow.
        /// </summary>
        /// <param name="workflowRules">The workflow rules.</param>
        /// <exception cref="RuleValidationException"></exception>
        public void AddWorkflow(params WorkflowRules[] workflowRules)
        {
            try
            {
                foreach (var workflowRule in workflowRules)
                {
                    var validator = new WorkflowRulesValidator();
                    validator.ValidateAndThrow(workflowRule);
                    _rulesCache.AddOrUpdateWorkflowRules(workflowRule.WorkflowName, workflowRule);
                }
            }
            catch (ValidationException ex)
            {
                throw new RuleValidationException(ex.Message, ex.Errors);
            }
        }

        /// <summary>
        /// Clears the workflows.
        /// </summary>
        public void ClearWorkflows()
        {
            _rulesCache.Clear();
        }

        /// <summary>
        /// Removes the workflow.
        /// </summary>
        /// <param name="workflowNames">The workflow names.</param>
        public void RemoveWorkflow(params string[] workflowNames)
        {
            foreach (var workflowName in workflowNames)
            {
                _rulesCache.Remove(workflowName);
            }
        }

        /// <summary>
        /// This will validate workflow rules then call execute method
        /// </summary>
        /// <typeparam name="T">type of entity</typeparam>
        /// <param name="input">input</param>
        /// <param name="workflowName">workflow name</param>
        /// <returns>list of rule result set</returns>
        private List<RuleResultTree> ValidateWorkflowAndExecuteRule(string workflowName, RuleParameter[] ruleParams)
        {
            List<RuleResultTree> result;

            if (RegisterRule(workflowName, ruleParams))
            {
                result = ExecuteAllRuleByWorkflow(workflowName, ruleParams);
            }
            else
            {
                _logger.LogTrace($"Rule config file is not present for the {workflowName} workflow");
                // if rules are not registered with Rules Engine
                throw new ArgumentException($"Rule config file is not present for the {workflowName} workflow");
            }
            return result;
        }

        /// <summary>
        /// This will compile the rules and store them to dictionary
        /// </summary>
        /// <param name="workflowName">workflow name</param>
        /// <param name="ruleParams">The rule parameters.</param>
        /// <returns>
        /// bool result
        /// </returns>
        private bool RegisterRule(string workflowName, params RuleParameter[] ruleParams)
        {
            string compileRulesKey = GetCompiledRulesKey(workflowName,ruleParams);
            if (_rulesCache.ContainsCompiledRules(compileRulesKey))
                return true;

            var workflowRules = _rulesCache.GetWorkFlowRules(workflowName);
            if (workflowRules != null)
            {
                var dictFunc = new Dictionary<string,RuleFunc<RuleResultTree>>();
                foreach (var rule in workflowRules.Rules)
                {
                    dictFunc.Add(rule.RuleName,CompileRule(workflowName, ruleParams, rule));
                }

                _rulesCache.AddOrUpdateCompiledRule(compileRulesKey,dictFunc);
                _logger.LogTrace($"Rules has been compiled for the {workflowName} workflow and added to dictionary");
                return true;
            }
            else
            {
                return false;
            }
        }


        private RuleFunc<RuleResultTree> CompileRule(string workflowName,string ruleName,RuleParameter[] ruleParameters){
            var rules = _rulesCache.GetRules(workflowName);
            var currentRule = rules?.SingleOrDefault(c => c.RuleName == ruleName);
            if(currentRule == null){
                throw new ArgumentException($"Workflow `{workflowName}` does not contain any rule named `{ruleName}`");
            }
            return CompileRule(workflowName,ruleParameters,currentRule);
        }

        private RuleFunc<RuleResultTree> CompileRule(string workflowName, RuleParameter[] ruleParams, Rule rule)
        {
            if(!_reSettings.EnableLocalParams){
                return _ruleCompiler.CompileRule(rule,ruleParams);
            }
            var compiledParamsKey = GetCompiledParamsCacheKey(workflowName, rule.RuleName, ruleParams);
            IEnumerable<CompiledParam> compiledParamList = _compiledParamsCache.GetOrCreate(compiledParamsKey, (entry) => _ruleParamCompiler.CompileParamsExpression(rule, ruleParams));
            var compiledRuleParameters = compiledParamList?.Select(c => c.AsRuleParameter()) ?? new List<RuleParameter>();
            var updatedRuleParams = ruleParams?.Concat(compiledRuleParameters);
            var compiledRule = _ruleCompiler.CompileRule(rule, updatedRuleParams?.ToArray());

            RuleFunc<RuleResultTree> updatedRule = (RuleParameter[] paramList) =>
            {
                var inputs = paramList.AsEnumerable();
                IEnumerable<CompiledParam> localParams = compiledParamList ?? new List<CompiledParam>();
                var evaluatedParamList = new List<RuleParameter>();
                foreach (var localParam in localParams)
                {
                    var evaluatedLocalParam = _ruleParamCompiler.EvaluateCompiledParam(localParam.Name, localParam.Value, inputs);
                    inputs = inputs.Append(evaluatedLocalParam);
                    evaluatedParamList.Add(evaluatedLocalParam);
                }
                var result = compiledRule(inputs.ToArray());
                result.RuleEvaluatedParams = evaluatedParamList;
                return result;
            };
            return updatedRule;
        }

       

        /// <summary>
        /// This will execute the compiled rules 
        /// </summary>
        /// <param name="workflowName"></param>
        /// <param name="ruleParams"></param>
        /// <returns>list of rule result set</returns>
        private List<RuleResultTree> ExecuteAllRuleByWorkflow(string workflowName, RuleParameter[] ruleParameters)
        {
            _logger.LogTrace($"Compiled rules found for {workflowName} workflow and executed");

            List<RuleResultTree> result = new List<RuleResultTree>();
            string compiledRulesCacheKey = GetCompiledRulesKey(workflowName,ruleParameters);
            foreach (var compiledRule in _rulesCache.GetCompiledRules(compiledRulesCacheKey)?.Values)
            {
                var resultTree = compiledRule(ruleParameters);
                result.Add(resultTree);
            }

            FormatErrorMessages(result);
            return result;
        }
        
        private string GetCompiledRulesKey(string workflowName, RuleParameter[] ruleParams)
        {
            var key =  $"{workflowName}-" + String.Join("-", ruleParams.Select(c => c.Type.Name));
            return key;
        }

        private string GetCompiledParamsCacheKey(string workflowName,string ruleName,RuleParameter[] ruleParams)
        {
           var key = $"compiledparams-{workflowName}-{ruleName}" + String.Join("-", ruleParams.Select(c => c.Type.Name));
           return key;
        }

        private IDictionary<string,Func<ActionBase>> GetDefaultActionRegistry(){
            return new Dictionary<string, Func<ActionBase>>{
                {"OutputExpression",() => new OutputExpressionAction(_ruleExpressionParser) },
                {"EvaluateRule", () => new EvaluateRuleAction(this) }
            };
        }

        /// <summary>
        /// The result
        /// </summary>
        /// <param name="ruleResultList">The result.</param>
        /// <returns>Updated error message.</returns>
        private IEnumerable<RuleResultTree> FormatErrorMessages(IEnumerable<RuleResultTree> ruleResultList)
        {
            if(_reSettings.EnableFormattedErrorMessage){
                foreach (var ruleResult in ruleResultList?.Where(r => !r.IsSuccess))
                {
                    var errorMessage = ruleResult?.Rule?.ErrorMessage;
                    if(errorMessage != null){
                        var errorParameters = Regex.Matches(errorMessage, ParamParseRegex);

                        var inputs = ruleResult.Inputs;
                        foreach (var param in errorParameters)
                        {
                            var paramVal = param?.ToString();
                            var property = paramVal?.Substring(2, paramVal.Length - 3);
                            if (property?.Split('.')?.Count() > 1)
                            {
                                var typeName = property?.Split('.')?[0];
                                var propertyName = property?.Split('.')?[1];
                                errorMessage = UpdateErrorMessage(errorMessage, inputs, property, typeName, propertyName);
                            }
                            else
                            {
                                var arrParams = inputs?.Select(c => new { Name = c.Key, c.Value });
                                var model = arrParams?.Where(a => string.Equals(a.Name, property))?.FirstOrDefault();
                                var value = model?.Value != null ? JsonConvert.SerializeObject(model?.Value) : null;
                                errorMessage = errorMessage?.Replace($"$({property})", value ?? $"$({property})");
                            }
                        }
                        ruleResult.ExceptionMessage = errorMessage;
                    }
                    
                }
            }
            return ruleResultList;
        }

        /// <summary>
        /// Updates the error message.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        /// <param name="evaluatedParams">The evaluated parameters.</param>
        /// <param name="property">The property.</param>
        /// <param name="typeName">Name of the type.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>Updated error message.</returns>
        private static string UpdateErrorMessage(string errorMessage, IDictionary<string,object> inputs, string property, string typeName, string propertyName)
        {
            var arrParams = inputs?.Select(c => new {Name = c.Key, c.Value });
            var model = arrParams?.Where(a => string.Equals(a.Name, typeName))?.FirstOrDefault();
            if (model != null)
            {
                var modelJson = JsonConvert.SerializeObject(model?.Value);
                var jObj = JObject.Parse(modelJson);
                JToken jToken = null;
                var val = jObj?.TryGetValue(propertyName, StringComparison.OrdinalIgnoreCase, out jToken);
                errorMessage = errorMessage.Replace($"$({property})", jToken != null ? jToken?.ToString() : $"({property})");
            }

            return errorMessage;
        }
        #endregion
    }
}
