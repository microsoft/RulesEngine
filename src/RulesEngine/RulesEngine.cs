// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using FluentValidation;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RulesEngine.Exceptions;
using RulesEngine.HelperFunctions;
using RulesEngine.Interfaces;
using RulesEngine.Models;
using RulesEngine.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

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
        private readonly ParamCompiler ruleParamCompiler;
        private const string ParamParseRegex = "(\\$\\(.*?\\))";
        #endregion

        #region Constructor
        public RulesEngine(string[] jsonConfig, ILogger logger, ReSettings reSettings = null) : this(logger, reSettings)
        {
            var workflowRules = jsonConfig.Select(item => JsonConvert.DeserializeObject<WorkflowRules>(item)).ToArray();
            AddWorkflow(workflowRules);
        }

        public RulesEngine(WorkflowRules[] workflowRules, ILogger logger, ReSettings reSettings = null) : this(logger, reSettings)
        {
            AddWorkflow(workflowRules);
        }

        public RulesEngine(ILogger logger, ReSettings reSettings = null)
        {
            _logger = logger ?? new NullLogger<RulesEngine>();
            _reSettings = reSettings ?? new ReSettings();
            ruleParamCompiler = new ParamCompiler(new RuleExpressionBuilderFactory(_reSettings), _logger);
        }
        #endregion

        #region Public Methods

        /// <summary>
        /// This will execute all the rules of the specified workflow
        /// </summary>
        /// <param name="workflowName">The name of the workflow with rules to execute against the inputs</param>
        /// <param name="inputs">A variable number of inputs</param>
        /// <returns>List of rule results</returns>
        public List<RuleResultTree> ExecuteRule(string workflowName, params object[] inputs)
        {
            _logger.LogTrace($"Called ExecuteRule for workflow {workflowName} and count of input {inputs.Count()}");

            var ruleParams = new List<RuleParameter>();

            for (int i = 0; i < inputs.Length; i++)
            {
                var input = inputs[i];
                ruleParams.Add(new RuleParameter($"input{i + 1}", input));
            }

            return ExecuteRule(workflowName, ruleParams.ToArray());
        }

        /// <summary>
        /// This will execute all the rules of the specified workflow
        /// </summary>
        /// <param name="workflowName">The name of the workflow with rules to execute against the inputs</param>
        /// <param name="ruleParams">A variable number of rule parameters</param>
        /// <returns>List of rule results</returns>
        public List<RuleResultTree> ExecuteRule(string workflowName, params RuleParameter[] ruleParams)
        {
            return ValidateWorkflowAndExecuteRule(workflowName, ruleParams);
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
                result = ExecuteRuleByWorkflow(workflowName, ruleParams);
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
                var lstFunc = new List<RuleFunc<RuleResultTree>>();
                var ruleCompiler = new RuleCompiler(new RuleExpressionBuilderFactory(_reSettings), _logger);
                foreach (var rule in _rulesCache.GetRules(workflowName))
                {
                    lstFunc.Add(CompileRule(workflowName, ruleParams, ruleCompiler, rule));
                }

                _rulesCache.AddOrUpdateCompiledRule(compileRulesKey,lstFunc );
                _logger.LogTrace($"Rules has been compiled for the {workflowName} workflow and added to dictionary");
                return true;
            }
            else
            {
                return false;
            }
        }

        private RuleFunc<RuleResultTree> CompileRule(string workflowName, RuleParameter[] ruleParams,RuleCompiler ruleCompiler, Rule rule)
        {
            var compiledParamsKey = GetCompiledParamsCacheKey(workflowName, rule.RuleName, ruleParams);
            IEnumerable<CompiledParam> compiledParamList = _compiledParamsCache.GetOrCreate(compiledParamsKey, (entry) => ruleParamCompiler.CompileParamsExpression(rule, ruleParams));
            var compiledRuleParameters = compiledParamList?.Select(c => c.AsRuleParameter()) ?? new List<RuleParameter>();
            var updatedRuleParams = ruleParams?.Concat(compiledRuleParameters);
            var compiledRule = ruleCompiler.CompileRule(rule, updatedRuleParams?.ToArray());

            RuleFunc<RuleResultTree> updatedRule = (object[] paramList) =>
            {
                var inputs = paramList.AsEnumerable();
                IEnumerable<CompiledParam> localParams = compiledParamList ?? new List<CompiledParam>();
                var evaluatedParamList = new List<RuleParameter>();
                foreach (var localParam in localParams)
                {
                    var evaluatedLocalParam = ruleParamCompiler.EvaluateCompiledParam(localParam.Name, localParam.Value, inputs);
                    inputs = inputs.Append(evaluatedLocalParam.Value);
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
        private List<RuleResultTree> ExecuteRuleByWorkflow(string workflowName, RuleParameter[] ruleParameters)
        {
            _logger.LogTrace($"Compiled rules found for {workflowName} workflow and executed");

            List<RuleResultTree> result = new List<RuleResultTree>();
            string compiledRulesCacheKey = GetCompiledRulesKey(workflowName,ruleParameters);
            foreach (var compiledRule in _rulesCache.GetCompiledRules(compiledRulesCacheKey))
            {
                var inputs = ruleParameters.Select(c => c.Value).ToArray();
                var resultTree = compiledRule(inputs);
                result.Add(resultTree);
            }

            FormatErrorMessages(result?.Where(r => !r.IsSuccess));
            return result;
        }
        
        private string GetCompiledRulesKey(string workflowName, RuleParameter[] ruleParams)
        {
            var key =  $"{workflowName}-" + String.Join("-", ruleParams.Select(c => c.Type.Name));
            return key.GetHashCode().ToString();
        }

        private string GetCompiledParamsCacheKey(string workflowName,string ruleName,RuleParameter[] ruleParams)
        {
           var key = $"compiledparams-{workflowName}-{ruleName}" + String.Join("-", ruleParams.Select(c => c.Type.Name));
           return key.GetHashCode().ToString();
        }

        /// <summary>
        /// The result
        /// </summary>
        /// <param name="ruleResultList">The result.</param>
        /// <returns>Updated error message.</returns>
        private IEnumerable<RuleResultTree> FormatErrorMessages(IEnumerable<RuleResultTree> ruleResultList)
        {
            foreach (var ruleResult in ruleResultList)
            {
                var errorParameters = Regex.Matches(ruleResult?.Rule?.ErrorMessage, ParamParseRegex);
                var errorMessage = ruleResult?.Rule?.ErrorMessage;
                var evaluatedParams = ruleResult?.RuleEvaluatedParams;
                foreach (var param in errorParameters)
                {
                    var paramVal = param?.ToString();
                    var property = paramVal?.Substring(2, paramVal.Length - 3);
                    if (property?.Split('.')?.Count() > 1)
                    {
                        var typeName = property?.Split('.')?[0];
                        var propertyName = property?.Split('.')?[1];
                        errorMessage = UpdateErrorMessage(errorMessage, evaluatedParams, property, typeName, propertyName);
                    }
                    else
                    {
                        var arrParams = evaluatedParams?.Select(c => new { c.Name, c.Value });
                        var model = arrParams?.Where(a => string.Equals(a.Name, property))?.FirstOrDefault();
                        var value = model?.Value != null ? JsonConvert.SerializeObject(model?.Value) : null;
                        errorMessage = errorMessage?.Replace($"$({property})", value ?? $"$({property})");
                    }
                }

                ruleResult.Rule.ErrorMessage = errorMessage;
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
        private static string UpdateErrorMessage(string errorMessage, IEnumerable<RuleParameter> evaluatedParams, string property, string typeName, string propertyName)
        {
            var arrParams = evaluatedParams?.Select(c => new { c.Name, c.Value });
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
