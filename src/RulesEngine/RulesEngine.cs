// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using FluentValidation;
using RulesEngine.Actions;
using RulesEngine.Exceptions;
using RulesEngine.ExpressionBuilders;
using RulesEngine.Extensions;
using RulesEngine.HelperFunctions;
using RulesEngine.Interfaces;
using RulesEngine.Models;
using RulesEngine.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace RulesEngine
{
    using System.Text.Json;
    using System.Text.Json.Nodes;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="IRulesEngine" />
    public class RulesEngine : IRulesEngine
    {
        #region Variables
        private readonly ReSettings _reSettings;
        private readonly RulesCache _rulesCache;
        private readonly RuleExpressionParser _ruleExpressionParser;
        private readonly RuleCompiler _ruleCompiler;
        private readonly ActionFactory _actionFactory;
        private const string ParamParseRegex = "(\\$\\(.*?\\))";
        #endregion

        #region Constructor
        public RulesEngine(string[] jsonConfig, ReSettings reSettings = null) : this(reSettings)
        {
            var workflow = jsonConfig.Select(item => JsonSerializer.Deserialize<Workflow>(item)).ToArray();
            AddWorkflow(workflow);
        }

        public RulesEngine(Workflow[] Workflows, ReSettings reSettings = null) : this(reSettings)
        {
            AddWorkflow(Workflows);
        }

        public RulesEngine(ReSettings reSettings = null)
        {
            _reSettings = reSettings == null ? new ReSettings(): new ReSettings(reSettings);
            if(_reSettings.CacheConfig == null)
            {
                _reSettings.CacheConfig = new MemCacheConfig();         
            }
            _rulesCache = new RulesCache(_reSettings);
            _ruleExpressionParser = new RuleExpressionParser(_reSettings);
            _ruleCompiler = new RuleCompiler(new RuleExpressionBuilderFactory(_reSettings, _ruleExpressionParser),_reSettings);
            _actionFactory = new ActionFactory(GetActionRegistry(_reSettings));
        }

        private IDictionary<string, Func<ActionBase>> GetActionRegistry(ReSettings reSettings)
        {
            var actionDictionary = GetDefaultActionRegistry();
            var customActions = reSettings.CustomActions ?? new Dictionary<string, Func<ActionBase>>();
            foreach (var customAction in customActions)
            {
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
            var ruleParams = new List<RuleParameter>();

            for (var i = 0; i < inputs.Length; i++)
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
            return await ExecuteAllRulesAsync(workflowName, ruleParams, default);
        }

        /// <summary>
        /// This will execute all the rules of the specified workflow with cooperative cancellation.
        /// </summary>
        /// <param name="workflowName">The name of the workflow with rules to execute against the inputs</param>
        /// <param name="ruleParams">The rule parameters</param>
        /// <param name="cancellationToken">Token observed between rules and before each action. A single
        /// rule's compiled expression is not interrupted mid-evaluation; cancellation is cooperative at
        /// rule and action boundaries.</param>
        /// <returns>List of rule results</returns>
        public async ValueTask<List<RuleResultTree>> ExecuteAllRulesAsync(string workflowName, RuleParameter[] ruleParams, CancellationToken cancellationToken)
        {
            var sortedRuleParams = ruleParams.ToList();
            sortedRuleParams.Sort((RuleParameter a, RuleParameter b) => string.Compare(a.Name, b.Name));
            var ruleResultList = ValidateWorkflowAndExecuteRule(workflowName, sortedRuleParams.ToArray(), cancellationToken);
            if (_reSettings.AutoExecuteActions)
            {
                await ExecuteActionAsync(ruleResultList, cancellationToken);
            }
            return ruleResultList;
        }

        private async ValueTask ExecuteActionAsync(IEnumerable<RuleResultTree> ruleResultList, CancellationToken cancellationToken = default)
        {
            foreach (var ruleResult in ruleResultList)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if(ruleResult.ChildResults !=  null)
                {
                    await ExecuteActionAsync(ruleResult.ChildResults, cancellationToken);
                }
                var actionResult = await ExecuteActionForRuleResult(ruleResult, false);
                ruleResult.ActionResult = new ActionResult {
                    Output = actionResult.Output,
                    Exception = actionResult.Exception
                };
                ThrowIfActionExceptionShouldPropagate(actionResult, ruleResult);
            }
        }

        public async ValueTask<ActionRuleResult> ExecuteActionWorkflowAsync(string workflowName, string ruleName, RuleParameter[] ruleParameters)
        {
            // Sort to match the cache-key convention used by ExecuteAllRulesAsync.
            var sortedRuleParams = ruleParameters.ToList();
            sortedRuleParams.Sort((a, b) => string.Compare(a.Name, b.Name));
            var sortedArr = sortedRuleParams.ToArray();

            // Compile the whole workflow once and reuse — was previously recompiled every call,
            // which was the hot path the reporter of #471 saw.
            if (!RegisterRule(workflowName, sortedArr))
            {
                throw new ArgumentException($"Rule config file is not present for the {workflowName} workflow");
            }

            var compiledRulesCacheKey = GetCompiledRulesKey(workflowName, sortedArr);
            var compiledRules = _rulesCache.GetCompiledRules(compiledRulesCacheKey);
            if (compiledRules == null || !compiledRules.TryGetValue(ruleName, out var compiledRule))
            {
                throw new ArgumentException($"Workflow `{workflowName}` does not contain any rule named `{ruleName}`");
            }

            var extendedRuleParameters = ApplyGlobalParams(compiledRulesCacheKey, sortedArr);
            var resultTree = compiledRule(extendedRuleParameters);
            // Mirror ExecuteAllRulesAsync's behavior: format the per-rule ErrorMessage template
            // into ExceptionMessage before any action runs / before returning. See #519.
            FormatErrorMessages(new[] { resultTree });
            var actionResult = await ExecuteActionForRuleResult(resultTree, true);
            ThrowIfActionExceptionShouldPropagate(actionResult, resultTree);
            return actionResult;
        }

        // Invokes the supplied globals delegate (if any) and appends the results as RuleParameters.
        private static RuleParameter[] AppendGlobals(RuleParameter[] ruleParameters, Func<object[], Dictionary<string, object>> globalParamsDelegate)
        {
            if (globalParamsDelegate == null)
            {
                return ruleParameters;
            }
            var inputs = ruleParameters.Select(c => c.Value).ToArray();
            var evaluated = globalParamsDelegate(inputs);
            var globals = evaluated.Select(kv => new RuleParameter(kv.Key, kv.Value));
            return ruleParameters.Concat(globals).ToArray();
        }

        private void ThrowIfActionExceptionShouldPropagate(ActionRuleResult actionResult, RuleResultTree resultTree)
        {
            if (actionResult?.Exception == null)
            {
                return;
            }
            if (_reSettings.IgnoreException || _reSettings.EnableExceptionAsErrorMessage)
            {
                return;
            }
            actionResult.Exception.Data[nameof(Rule.RuleName)] = resultTree?.Rule?.RuleName;
            throw actionResult.Exception;
        }

        private async ValueTask<ActionRuleResult> ExecuteActionForRuleResult(RuleResultTree resultTree, bool includeRuleResults = false)
        {
            var ruleActions = resultTree?.Rule?.Actions;
            var actionInfo = resultTree?.IsSuccess == true ? ruleActions?.OnSuccess : ruleActions?.OnFailure;

            if (actionInfo != null)
            {
                var action = _actionFactory.Get(actionInfo.Name);
                var ruleParameters = resultTree.Inputs.Select(kv => new RuleParameter(kv.Key, kv.Value)).ToArray();
                return await action.ExecuteAndReturnResultAsync(new ActionContext(actionInfo.Context, resultTree), ruleParameters, includeRuleResults);
            }
            else
            {
                //If there is no action,return output as null and return the result for rule
                return new ActionRuleResult {
                    Output = null,
                    Results = includeRuleResults ? new List<RuleResultTree>() { resultTree } : null
                };
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Adds the workflow if the workflow name is not already added. Ignores the rest.
        /// </summary>
        /// <param name="workflows">The workflow rules.</param>
        /// <exception cref="RuleValidationException"></exception>
        public void AddWorkflow(params Workflow[] workflows)
        {
            try
            {
                foreach (var workflow in workflows)
                {                    
                    var validator = new WorkflowsValidator();
                    validator.ValidateAndThrow(workflow);
                    if (!_rulesCache.ContainsWorkflows(workflow.WorkflowName))
                    {
                        _rulesCache.AddOrUpdateWorkflows(workflow.WorkflowName, workflow);
                    }
                    else
                    {
                        throw new ValidationException($"Cannot add workflow `{workflow.WorkflowName}` as it already exists. Use `AddOrUpdateWorkflow` to update existing workflow");
                    }
                }
            }
            catch (ValidationException ex)
            {
                throw new RuleValidationException(ex.Message, ex.Errors);
            }
        }

        /// <summary>
        /// Adds new workflow rules if not previously added.
        /// Or updates the rules for an existing workflow.
        /// </summary>
        /// <param name="workflows">The workflow rules.</param>
        /// <exception cref="RuleValidationException"></exception>
        public void AddOrUpdateWorkflow(params Workflow[] workflows)
        {
            try
            {
                foreach (var workflow in workflows)
                {
                    var validator = new WorkflowsValidator();
                    validator.ValidateAndThrow(workflow);
                    _rulesCache.AddOrUpdateWorkflows(workflow.WorkflowName, workflow);
                }
            }
            catch (ValidationException ex)
            {
                throw new RuleValidationException(ex.Message, ex.Errors);
            }
        }

        public List<string> GetAllRegisteredWorkflowNames()
        {
            return _rulesCache.GetAllWorkflowNames();
        }

        /// <summary>
        /// Checks is workflow exist.
        /// </summary>
        /// <param name="workflowName">The workflow name.</param>
        /// <returns> <c>true</c> if contains the specified workflow name; otherwise, <c>false</c>.</returns>
        public bool ContainsWorkflow(string workflowName)
        {
            return _rulesCache.ContainsWorkflows(workflowName);
        }

        /// <summary>
        /// Clears the workflow.
        /// </summary>
        public void ClearWorkflows()
        {
            _rulesCache.Clear();
        }

        /// <summary>
        /// Removes the workflows.
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
        private List<RuleResultTree> ValidateWorkflowAndExecuteRule(string workflowName, RuleParameter[] ruleParams, CancellationToken cancellationToken = default)
        {
            List<RuleResultTree> result;

            if (RegisterRule(workflowName, ruleParams))
            {
                result = ExecuteAllRuleByWorkflow(workflowName, ruleParams, cancellationToken);
            }
            else
            {
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
            var compileRulesKey = GetCompiledRulesKey(workflowName, ruleParams);
            if (_rulesCache.AreCompiledRulesUpToDate(compileRulesKey, workflowName))
            {
                return true;
            }

            var workflow = _rulesCache.GetWorkflow(workflowName);
            if (workflow != null)
            {
                var dictFunc = new Dictionary<string, RuleFunc<RuleResultTree>>();
                if (_reSettings.AutoRegisterInputType)
                {
                    var collector = new HashSet<Type>(_reSettings.CustomTypes.Safe());

                    foreach (var rp in ruleParams)
                    {
                        CollectAllElementTypes(rp.Type, collector);
                    }

                    _reSettings.CustomTypes = collector.ToArray();
                }

                // Compile global params ONCE per workflow registration. The resulting delegate is
                // invoked once per ExecuteAllRulesAsync call (in ExecuteAllRuleByWorkflow) and its
                // results passed as extra RuleParameters to each rule. See #714.
                RuleExpressionParameter[] globalParamValues;
                try
                {
                    globalParamValues = _ruleCompiler.GetRuleExpressionParameters(workflow.RuleExpressionType, workflow.GlobalParams, ruleParams);
                }
                catch (Exception ex)
                {
                    // Mirror the legacy per-rule error reporting when global-param compilation fails.
                    foreach (var rule in workflow.Rules.Where(c => c.Enabled))
                    {
                        var msg = $"Error while compiling rule `{rule.RuleName}`: {ex.Message}";
                        dictFunc.Add(rule.RuleName, Helpers.ToRuleExceptionResult(_reSettings, rule, new RuleException(msg, ex)));
                    }
                    _rulesCache.AddOrUpdateCompiledRule(compileRulesKey, dictFunc);
                    return true;
                }

                var globalParamExp = new Lazy<RuleExpressionParameter[]>(() => globalParamValues);

                if (globalParamValues.Length > 0)
                {
                    var globalParamsDelegate = _ruleCompiler.CompileScopedParams(workflow.RuleExpressionType, ruleParams, globalParamValues);
                    _rulesCache.AddOrUpdateGlobalParamsDelegate(compileRulesKey, globalParamsDelegate);
                }

                var enabledRules = workflow.Rules.Where(c => c.Enabled).ToArray();
                var compiledFuncs = new RuleFunc<RuleResultTree>[enabledRules.Length];
                if (_reSettings.EnableParallelRuleCompilation)
                {
                    try
                    {
                        System.Threading.Tasks.Parallel.For(0, enabledRules.Length, i => {
                            compiledFuncs[i] = CompileRule(enabledRules[i], workflow.RuleExpressionType, ruleParams, globalParamExp);
                        });
                    }
                    catch (AggregateException ae)
                    {
                        // Preserve the serial-compilation contract: the first rule that fails
                        // to compile surfaces its own exception, not an AggregateException.
                        System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ae.InnerExceptions[0]).Throw();
                    }
                }
                else
                {
                    for (var i = 0; i < enabledRules.Length; i++)
                    {
                        compiledFuncs[i] = CompileRule(enabledRules[i], workflow.RuleExpressionType, ruleParams, globalParamExp);
                    }
                }
                for (var i = 0; i < enabledRules.Length; i++)
                {
                    dictFunc.Add(enabledRules[i].RuleName, compiledFuncs[i]);
                }

                _rulesCache.AddOrUpdateCompiledRule(compileRulesKey, dictFunc);
                return true;
            }
            else
            {
                return false;
            }
        }


        private RuleFunc<RuleResultTree> CompileRule(Rule rule, RuleExpressionType ruleExpressionType, RuleParameter[] ruleParams, Lazy<RuleExpressionParameter[]> scopedParams)
        {
            return _ruleCompiler.CompileRule(rule, ruleExpressionType, ruleParams, scopedParams);
        }

        private static void CollectAllElementTypes(Type t, ISet<Type> collector)
        {
            if (t == null || collector.Contains(t))
                return;

            collector.Add(t);

            if (t.IsGenericType)
            {
                foreach (var ga in t.GetGenericArguments())
                    CollectAllElementTypes(ga, collector);
            }

            if (t.IsArray)
            {
                CollectAllElementTypes(t.GetElementType(), collector);
            }

            if (Nullable.GetUnderlyingType(t) is Type underly && !collector.Contains(underly))
            {
                CollectAllElementTypes(underly, collector);
            }
        }

        /// <summary>
        /// This will execute the compiled rules 
        /// </summary>
        /// <param name="workflowName"></param>
        /// <param name="ruleParams"></param>
        /// <returns>list of rule result set</returns>
        private List<RuleResultTree> ExecuteAllRuleByWorkflow(string workflowName, RuleParameter[] ruleParameters, CancellationToken cancellationToken = default)
        {
            var result = new List<RuleResultTree>();
            var compiledRulesCacheKey = GetCompiledRulesKey(workflowName, ruleParameters);
            var compiledRules = _rulesCache.GetCompiledRules(compiledRulesCacheKey);

            RuleParameter[] extendedRuleParameters;
            Exception globalEvaluationException = null;
            try
            {
                extendedRuleParameters = ApplyGlobalParams(compiledRulesCacheKey, ruleParameters);
            }
            catch (Exception ex)
            {
                globalEvaluationException = ex;
                extendedRuleParameters = ruleParameters;
            }

            var ruleByName = new Dictionary<string, Rule>();
            foreach (var rule in _rulesCache.GetWorkflow(workflowName)?.Rules?.Where(c => c.Enabled) ?? Enumerable.Empty<Rule>())
            {
                ruleByName[rule.RuleName] = rule;
            }

            foreach (var compiledRule in compiledRules)
            {
                cancellationToken.ThrowIfCancellationRequested();
                RuleResultTree resultTree;
                if (globalEvaluationException != null && ruleByName != null && ruleByName.TryGetValue(compiledRule.Key, out var rule))
                {
                    var msg = $"Error while executing scoped params for rule `{rule.RuleName}` - {globalEvaluationException.Message}";
                    var errFn = Helpers.ToRuleExceptionResult(_reSettings, rule, new RuleException(msg, globalEvaluationException));
                    resultTree = errFn(ruleParameters);
                }
                else
                {
                    resultTree = compiledRule.Value(extendedRuleParameters);
                }
                result.Add(resultTree);
            }

            FormatErrorMessages(result);
            return result;
        }

        private RuleParameter[] ApplyGlobalParams(string compiledRulesCacheKey, RuleParameter[] ruleParameters)
        {
            return AppendGlobals(ruleParameters, _rulesCache.GetGlobalParamsDelegate(compiledRulesCacheKey));
        }

        private string GetCompiledRulesKey(string workflowName, RuleParameter[] ruleParams)
        {
            var ruleParamsKey = string.Join("-", ruleParams.Select(c => $"{c.Name}_{c.Type.Name}"));
            var key = $"{workflowName}-" + ruleParamsKey;
            return key;
        }

        private IDictionary<string, Func<ActionBase>> GetDefaultActionRegistry()
        {
            return new Dictionary<string, Func<ActionBase>>{
                {"OutputExpression",() => new OutputExpressionAction(_ruleExpressionParser) },
                {"EvaluateRule", () => new EvaluateRuleAction(this,_ruleExpressionParser) }
            };
        }

        /// <summary>
        /// The result
        /// </summary>
        /// <param name="ruleResultList">The result.</param>
        /// <returns>Updated error message.</returns>
        private IEnumerable<RuleResultTree> FormatErrorMessages(IEnumerable<RuleResultTree> ruleResultList)
        {
            if (_reSettings.EnableFormattedErrorMessage)
            {
                foreach (var ruleResult in ruleResultList?.Where(r => !r.IsSuccess))
                {
                    var errorMessage = ruleResult?.Rule?.ErrorMessage;
                    if (string.IsNullOrWhiteSpace(ruleResult.ExceptionMessage) && errorMessage != null)
                    {
                        var errorParameters = Regex.Matches(errorMessage, ParamParseRegex);

                        var inputs = ruleResult.Inputs;
                        foreach (var param in errorParameters)
                        {
                            var paramVal = param?.ToString();
                            var property = paramVal?.Substring(2, paramVal.Length - 3);
                            if (property?.Split('.')?.Count() > 1)
                            {
                                errorMessage = UpdateErrorMessage(errorMessage, inputs, property);
                            }
                            else
                            {
                                var arrParams = inputs?.Select(c => new { Name = c.Key, c.Value });
                                var model = arrParams?.Where(a => string.Equals(a.Name, property))?.FirstOrDefault();
                                var value = model?.Value != null ? JsonSerializer.Serialize(model?.Value) : null;
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
        /// Resolves a dotted-path placeholder like $(input1.Inner.Name) against the rule inputs,
        /// walking arbitrary depth. See #696.
        /// </summary>
        private static string UpdateErrorMessage(string errorMessage, IDictionary<string, object> inputs, string property)
        {
            var segments = property.Split('.');
            var typeName = segments[0];
            var model = inputs?.FirstOrDefault(c => string.Equals(c.Key, typeName));
            if (model?.Value == null)
            {
                return errorMessage;
            }

            var modelJson = JsonSerializer.Serialize(model.Value.Value);
            JsonNode current = JsonNode.Parse(modelJson);
            for (var i = 1; i < segments.Length && current != null; i++)
            {
                current = current is JsonObject jObj && jObj.TryGetPropertyValue(segments[i], out var next)
                    ? next
                    : null;
            }

            if (current == null)
            {
                return errorMessage;
            }

            // JsonValue (leaf scalar) should render without quotes; objects/arrays render as JSON.
            var replacement = current is JsonValue v && v.TryGetValue<string>(out var stringValue)
                ? stringValue
                : current.ToString();
            return errorMessage.Replace($"$({property})", replacement);
        }
        #endregion
    }
}
