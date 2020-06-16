// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.Logging;
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

        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="RuleCompiler"/> class.
        /// </summary>
        /// <param name="expressionBuilderFactory">The expression builder factory.</param>
        /// <exception cref="ArgumentNullException">expressionBuilderFactory</exception>
        internal RuleCompiler(RuleExpressionBuilderFactory expressionBuilderFactory, ILogger logger)
        {
            if (expressionBuilderFactory == null)
            {
                throw new ArgumentNullException($"{nameof(expressionBuilderFactory)} can't be null.");
            }

            if (logger == null)
            {
                throw new ArgumentNullException($"{nameof(logger)} can't be null.");
            }

            _logger = logger;
            _expressionBuilderFactory = expressionBuilderFactory;
        }

        /// <summary>
        /// Compiles the rule
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rule"></param>
        /// <param name="input"></param>
        /// <param name="ruleParam"></param>
        /// <returns>Compiled func delegate</returns>
        public Delegate CompileRule(Rule rule,params RuleParameter[] ruleParams)
        {
            try
            {
                IEnumerable<ParameterExpression> typeParameterExpressions = GetParameterExpression(ruleParams).ToList(); // calling ToList to avoid multiple calls this the method for nested rule scenario.

                ParameterExpression ruleInputExp = Expression.Parameter(typeof(RuleInput), nameof(RuleInput));

                RuleFunc<RuleResultTree> ruleExpression = GetExpressionForRule(rule, typeParameterExpressions,ruleParams, ruleInputExp);

                return ruleExpression;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }

        // <summary>
        /// Gets the parameter expression.
        /// </summary>
        /// <param name="ruleParams">The types.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">
        /// types
        /// or
        /// type
        /// </exception>
        private IEnumerable<ParameterExpression> GetParameterExpression(params RuleParameter[] ruleParams)
        {
            if (ruleParams == null || !ruleParams.Any())
            {
                throw new ArgumentException($"{nameof(ruleParams)} can't be null/empty.");
            }

            foreach (var ruleParam in ruleParams)
            {
                if (ruleParam == null)
                {
                    throw new ArgumentException($"{nameof(ruleParam)} can't be null.");
                }

                yield return Expression.Parameter(ruleParam.Type, ruleParam.Name);
            }
        }

        /// <summary>
        /// Gets the expression for rule.
        /// </summary>
        /// <param name="rule">The rule.</param>
        /// <param name="typeParameterExpressions">The type parameter expressions.</param>
        /// <param name="ruleInputExp">The rule input exp.</param>
        /// <returns></returns>
        private RuleFunc<RuleResultTree> GetExpressionForRule(Rule rule, IEnumerable<ParameterExpression> typeParameterExpressions, RuleParameter[] ruleParams, ParameterExpression ruleInputExp)
        {
            ExpressionType nestedOperator;

            if (Enum.TryParse(rule.Operator, out nestedOperator) && nestedOperators.Contains(nestedOperator) &&
                rule.Rules != null && rule.Rules.Any())
            {
                return BuildNestedExpression(rule, nestedOperator, typeParameterExpressions, ruleParams, ruleInputExp);
            }
            else
            {
                return BuildExpression(rule, typeParameterExpressions,ruleParams);
            }
        }

        /// <summary>
        /// Builds the expression.
        /// </summary>
        /// <param name="rule">The rule.</param>
        /// <param name="typeParameterExpressions">The type parameter expressions.</param>
        /// <param name="ruleInputExp">The rule input exp.</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        private RuleFunc<RuleResultTree> BuildExpression(Rule rule, IEnumerable<ParameterExpression> typeParameterExpressions, RuleParameter[] ruleParams)
        {
            if (!rule.RuleExpressionType.HasValue)
            {
                throw new InvalidOperationException($"RuleExpressionType can not be null for leaf level expressions.");
            }

            var ruleExpressionBuilder = _expressionBuilderFactory.RuleGetExpressionBuilder(rule.RuleExpressionType.Value);

            var expression = ruleExpressionBuilder.BuildExpressionForRule(rule, typeParameterExpressions);

            return expression;
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
        private RuleFunc<RuleResultTree> BuildNestedExpression(Rule parentRule, ExpressionType operation, IEnumerable<ParameterExpression> typeParameterExpressions, RuleParameter[] ruleParams, ParameterExpression ruleInputExp)
        {
            var expressions = new List<RuleFunc<RuleResultTree>>();
            foreach (var r in parentRule.Rules)
            {
                expressions.Add(GetExpressionForRule(r, typeParameterExpressions, ruleParams ,ruleInputExp));
            }

            return (paramArray) =>
             {
                 var resultList = expressions.Select(fn => fn(paramArray));
                 RuleFunc<bool> isSuccess = (p) => ApplyOperation(resultList, operation);
                 RuleFunc<RuleResultTree> result =  Helpers.ToResultTree(parentRule, resultList,isSuccess);
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
