// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.Logging;
using RulesEngine.ExpressionBuilders;
using RulesEngine.HelperFunctions;
using RulesEngine.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace RulesEngine
{
    /// <summary>
    /// Rule compilers
    /// </summary>
    internal class RuleCompiler
    {
        /// <summary>
        /// The nested operators
        /// </summary>
        private readonly ExpressionType[] nestedOperators = new ExpressionType[] { ExpressionType.And, ExpressionType.AndAlso, ExpressionType.Or, ExpressionType.OrElse };

        /// <summary>
        /// The expression builder factory
        /// </summary>
        private readonly RuleExpressionBuilderFactory _expressionBuilderFactory;

        /// <summary>
        /// The logger
        /// </summary>
        private readonly ILogger _logger;
        private readonly RuleExpressionParser _ruleExpressionParser;

        /// <summary>
        /// Initializes a new instance of the <see cref="RuleCompiler"/> class.
        /// </summary>
        /// <param name="expressionBuilderFactory">The expression builder factory.</param>
        /// <exception cref="ArgumentNullException">expressionBuilderFactory</exception>
        internal RuleCompiler(RuleExpressionBuilderFactory expressionBuilderFactory, RuleExpressionParser ruleExpressionParser, ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException($"{nameof(logger)} can't be null.");
            _ruleExpressionParser = ruleExpressionParser;
            _expressionBuilderFactory = expressionBuilderFactory ?? throw new ArgumentNullException($"{nameof(expressionBuilderFactory)} can't be null.");
        }

        /// <summary>
        /// Compiles the rule
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rule"></param>
        /// <param name="input"></param>
        /// <param name="ruleParam"></param>
        /// <returns>Compiled func delegate</returns>
        internal RuleFunc<RuleResultTree> CompileRule(Rule rule, RuleParameter[] ruleParams, LocalParam[] globalParams)
        {
            try
            {
                if (rule == null)
                {
                    throw new ArgumentNullException(nameof(rule));
                }
                var globalParamExp = GetRuleExpressionParameters(globalParams, ruleParams, new RuleExpressionParameter[] { });

                var ruleExpression = GetDelegateForRule(rule, ruleParams,globalParamExp);
                return ruleExpression;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }



        /// <summary>
        /// Gets the expression for rule.
        /// </summary>
        /// <param name="rule">The rule.</param>
        /// <param name="typeParameterExpressions">The type parameter expressions.</param>
        /// <param name="ruleInputExp">The rule input exp.</param>
        /// <returns></returns>
        private RuleFunc<RuleResultTree> GetDelegateForRule(Rule rule, RuleParameter[] ruleParams, RuleExpressionParameter[] ruleExpParamList)
        {
            ruleExpParamList = ruleExpParamList.Concat(GetRuleExpressionParameters(rule?.LocalParams, ruleParams, ruleExpParamList)).ToArray();

            if (Enum.TryParse(rule.Operator, out ExpressionType nestedOperator) && nestedOperators.Contains(nestedOperator) &&
                rule.Rules != null && rule.Rules.Any())
            {
                return BuildNestedRuleFunc(rule, nestedOperator, ruleParams, ruleExpParamList);
            }
            else
            {
                return BuildRuleFunc(rule, ruleParams,ruleExpParamList);
            }
        }

        private RuleExpressionParameter[] GetRuleExpressionParameters(IEnumerable<LocalParam> localParams, RuleParameter[] ruleParams, RuleExpressionParameter[] globalParams)
        {
            var ruleExpParams = new List<RuleExpressionParameter>(globalParams);

            if (localParams?.Any() == true) {

                var parameters = ruleParams.Select(c => c.ParameterExpression)
                                            .Concat(ruleExpParams.Select(c => c.ParameterExpression))
                                            .ToList();

                foreach (var lp in localParams)
                {
                    var lpExpression = _ruleExpressionParser.Parse(lp.Expression, parameters.ToArray() ,null).Body;
                    var ruleExpParam = new RuleExpressionParameter() {
                        ParameterExpression = Expression.Parameter(lpExpression.Type, lp.Name),
                        ValueExpression = lpExpression 
                    };
                    parameters.Add(ruleExpParam.ParameterExpression);
                    ruleExpParams.Add(ruleExpParam);
                }
            }

            return ruleExpParams.ToArray();

        }

        /// <summary>
        /// Builds the expression.
        /// </summary>
        /// <param name="rule">The rule.</param>
        /// <param name="typeParameterExpressions">The type parameter expressions.</param>
        /// <param name="ruleInputExp">The rule input exp.</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        private RuleFunc<RuleResultTree> BuildRuleFunc(Rule rule, RuleParameter[] ruleParams, RuleExpressionParameter[] ruleExpressionParams)
        {
            if (!rule.RuleExpressionType.HasValue)
            {
                throw new InvalidOperationException($"RuleExpressionType can not be null for leaf level expressions.");
            }

            var ruleExpressionBuilder = _expressionBuilderFactory.RuleGetExpressionBuilder(rule.RuleExpressionType.Value);

            var ruleFunc = ruleExpressionBuilder.BuildDelegateForRule(rule, ruleParams, ruleExpressionParams);

            return ruleFunc;
        }

        /// <summary>
        /// Builds the nested expression.
        /// </summary>
        /// <param name="parentRule">The parent rule.</param>
        /// <param name="childRules">The child rules.</param>
        /// <param name="operation">The operation.</param>
        /// <param name="typeParameterExpressions">The type parameter expressions.</param>
        /// <param name="ruleInputExp">The rule input exp.</param>
        /// <returns>Expression of func delegate</returns>
        /// <exception cref="InvalidCastException"></exception>
        private RuleFunc<RuleResultTree> BuildNestedRuleFunc(Rule parentRule, ExpressionType operation, RuleParameter[] ruleParams, RuleExpressionParameter[] scopedParams)
        {
            var ruleFuncList = new List<RuleFunc<RuleResultTree>>();
            foreach (var r in parentRule.Rules)
            {
                ruleFuncList.Add(GetDelegateForRule(r, ruleParams,scopedParams));
            }

            return (paramArray) => {
                var resultList = ruleFuncList.Select(fn => fn(paramArray)).ToList();
                Func<object[], bool> isSuccess = (p) => ApplyOperation(resultList, operation);
                var result = Helpers.ToResultTree(parentRule, resultList, isSuccess);
                return result(paramArray);
            };
        }


        private bool ApplyOperation(IEnumerable<RuleResultTree> ruleResults, ExpressionType operation)
        {
            switch (operation)
            {
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                    return ruleResults.All(r => r.IsSuccess);

                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    return ruleResults.Any(r => r.IsSuccess);
                default:
                    return false;
            }
        }
    }
}
