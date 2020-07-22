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

        /// <summary>
        /// The logger
        /// </summary>
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

                Expression<Func<RuleInput, RuleResultTree>> ruleExpression = GetExpressionForRule(rule, typeParameterExpressions, ruleInputExp);

                var lambdaParameterExps = new List<ParameterExpression>(typeParameterExpressions) { ruleInputExp };


                var expression = Expression.Lambda(ruleExpression.Body, lambdaParameterExps);


                return expression.Compile();
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
        private Expression<Func<RuleInput, RuleResultTree>> GetExpressionForRule(Rule rule, IEnumerable<ParameterExpression> typeParameterExpressions, ParameterExpression ruleInputExp)
        {
            ExpressionType nestedOperator;

            if (Enum.TryParse(rule.Operator, out nestedOperator) && nestedOperators.Contains(nestedOperator) &&
                rule.Rules != null && rule.Rules.Any())
            {
                return BuildNestedExpression(rule, nestedOperator, typeParameterExpressions, ruleInputExp);
            }
            else
            {
                return BuildExpression(rule, typeParameterExpressions, ruleInputExp);
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
        private Expression<Func<RuleInput, RuleResultTree>> BuildExpression(Rule rule, IEnumerable<ParameterExpression> typeParameterExpressions, ParameterExpression ruleInputExp)
        {
            if (!rule.RuleExpressionType.HasValue)
            {
                throw new InvalidOperationException($"RuleExpressionType can not be null for leaf level expressions.");
            }

            var ruleExpressionBuilder = _expressionBuilderFactory.RuleGetExpressionBuilder(rule.RuleExpressionType.Value);

            var expression = ruleExpressionBuilder.BuildExpressionForRule(rule, typeParameterExpressions, ruleInputExp);

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
        private Expression<Func<RuleInput, RuleResultTree>> BuildNestedExpression(Rule parentRule, ExpressionType operation, IEnumerable<ParameterExpression> typeParameterExpressions, ParameterExpression ruleInputExp)
        {
            List<Expression<Func<RuleInput, RuleResultTree>>> expressions = new List<Expression<Func<RuleInput, RuleResultTree>>>();
            foreach (var r in parentRule.Rules)
            {
                expressions.Add(GetExpressionForRule(r, typeParameterExpressions, ruleInputExp));
            }

            List<MemberInitExpression> childRuleResultTree = new List<MemberInitExpression>();

            foreach (var exp in expressions)
            {
                var resultMemberInitExpression = exp.Body as MemberInitExpression;

                if (resultMemberInitExpression == null)// assert is a MemberInitExpression
                {
                    throw new InvalidCastException($"expression.Body '{exp.Body}' is not of MemberInitExpression type.");
                }

                childRuleResultTree.Add(resultMemberInitExpression);
            }

            Expression<Func<RuleInput, RuleResultTree>> nestedExpression = Helpers.ToResultTreeExpression(parentRule, childRuleResultTree, BinaryExpression(expressions, operation), typeParameterExpressions, ruleInputExp);

            return nestedExpression;
        }

        /// <summary>
        /// Binaries the expression.
        /// </summary>
        /// <param name="expressions">The expressions.</param>
        /// <param name="operationType">Type of the operation.</param>
        /// <returns>Binary Expression</returns>
        private BinaryExpression BinaryExpression(IList<Expression<Func<RuleInput, RuleResultTree>>> expressions, ExpressionType operationType)
        {
            if (expressions.Count == 1)
            {
                return ResolveIsSuccessBinding(expressions.First());
            }

            BinaryExpression nestedBinaryExp = Expression.MakeBinary(operationType, ResolveIsSuccessBinding(expressions[0]), ResolveIsSuccessBinding(expressions[1]));

            for (int i = 2; expressions.Count > i; i++)
            {
                nestedBinaryExp = Expression.MakeBinary(operationType, nestedBinaryExp, ResolveIsSuccessBinding(expressions[i]));
            }

            return nestedBinaryExp;
        }

        /// <summary>
        /// Resolves the is success binding.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>Binary expression of IsSuccess prop</returns>
        /// <exception cref="ArgumentNullException">expression</exception>
        /// <exception cref="InvalidCastException"></exception>
        /// <exception cref="NullReferenceException">
        /// IsSuccess
        /// or
        /// IsSuccess
        /// or
        /// IsSuccess
        /// </exception>
        private BinaryExpression ResolveIsSuccessBinding(Expression<Func<RuleInput, RuleResultTree>> expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException($"{nameof(expression)} should not be null.");
            }

            var memberInitExpression = expression.Body as MemberInitExpression;

            if (memberInitExpression == null)// assert it's a MemberInitExpression
            {
                throw new InvalidCastException($"expression.Body '{expression.Body}' is not of MemberInitExpression type.");
            }

            MemberAssignment isSuccessBinding = (MemberAssignment)memberInitExpression.Bindings.FirstOrDefault(f => f.Member.Name == nameof(RuleResultTree.IsSuccess));

            if (isSuccessBinding == null)
            {
                throw new NullReferenceException($"Expected {nameof(RuleResultTree.IsSuccess)} property binding not found in {memberInitExpression}.");
            }

            if (isSuccessBinding.Expression == null)
            {
                throw new NullReferenceException($"{nameof(RuleResultTree.IsSuccess)} assignment expression can not be null.");
            }

            BinaryExpression isSuccessExpression = isSuccessBinding.Expression as BinaryExpression;

            if (isSuccessExpression == null)
            {
                throw new NullReferenceException($"Expected {nameof(RuleResultTree.IsSuccess)} assignment expression to be of {typeof(BinaryExpression)} and not {isSuccessBinding.Expression.GetType()}");
            }

            return isSuccessExpression;
        } 
    }
}
