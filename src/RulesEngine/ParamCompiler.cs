using Microsoft.Extensions.Logging;
using RulesEngine.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Linq;

namespace RulesEngine
{
    /// <summary>
    /// Rule param compilers
    /// </summary>
    internal class ParamCompiler
    {
        /// <summary>
        /// The expression builder factory
        /// </summary>
        private readonly RuleExpressionBuilderFactory _expressionBuilderFactory;

        /// <summary>
        /// The logger
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParamCompiler"/> class.
        /// </summary>
        /// <param name="expressionBuilderFactory">The expression builder factory.</param>
        /// <exception cref="ArgumentNullException">expressionBuilderFactory</exception>
        internal ParamCompiler(RuleExpressionBuilderFactory expressionBuilderFactory, ILogger logger)
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
        /// Compiles the and evaluate parameter expression.
        /// </summary>
        /// <param name="rule">The rule.</param>
        /// <param name="ruleParams">The rule parameters.</param>
        /// <returns>
        /// IEnumerable&lt;RuleParameter&gt;.
        /// </returns>
        public IEnumerable<CompiledParam> CompileParamsExpression(Rule rule, IEnumerable<RuleParameter> ruleParams)
        {
           
            if(rule.LocalParams == null)    return null;

            var compiledParameters = new List<CompiledParam>();
            var evaluatedParameters = new List<RuleParameter>();
            foreach (var param in rule.LocalParams)
            {
                IEnumerable<ParameterExpression> typeParameterExpressions = GetParameterExpression(ruleParams.ToArray()).ToList(); // calling ToList to avoid multiple calls this the method for nested rule scenario.
                var ruleParamExpression = GetExpressionForRuleParam(param, typeParameterExpressions);
                var lambdaParameterExps = new List<ParameterExpression>(typeParameterExpressions);
                var expression = Expression.Lambda(ruleParamExpression, lambdaParameterExps);
                var compiledParam = expression.Compile();
                compiledParameters.Add(new CompiledParam { Name = param.Name, Value = compiledParam, Parameters = evaluatedParameters });
                var evaluatedParam = this.EvaluateCompiledParam(param.Name, compiledParam, ruleParams.Select(c => c.Value).ToArray());
                ruleParams = ruleParams.Append(evaluatedParam);
                evaluatedParameters.Add(evaluatedParam);
            }

            return compiledParameters;
        }

        /// <summary>Evaluates the compiled parameter.</summary>
        /// <param name="paramName">Name of the parameter.</param>
        /// <param name="compiledParam">The compiled parameter.</param>
        /// <param name="ruleParams">The rule parameters.</param>
        /// <returns>RuleParameter.</returns>
        public RuleParameter EvaluateCompiledParam(string paramName, Delegate compiledParam, IEnumerable<object> inputs)
        {
            var result = compiledParam.DynamicInvoke(new List<object>(inputs).ToArray());
            return new RuleParameter(paramName, result);
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
        /// <param name="param">The rule.</param>
        /// <param name="typeParameterExpressions">The type parameter expressions.</param>
        /// <param name="ruleInputExp">The rule input exp.</param>
        /// <returns></returns>
        private Expression GetExpressionForRuleParam(LocalParam param, IEnumerable<ParameterExpression> typeParameterExpressions)
        {
            return BuildExpression(param, typeParameterExpressions);
        }

        /// <summary>
        /// Builds the expression.
        /// </summary>
        /// <param name="param">The rule.</param>
        /// <param name="typeParameterExpressions">The type parameter expressions.</param>
        /// <param name="ruleInputExp">The rule input exp.</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        private Expression BuildExpression(LocalParam param, IEnumerable<ParameterExpression> typeParameterExpressions)
        {
            var ruleExpressionBuilder = _expressionBuilderFactory.RuleGetExpressionBuilder(RuleExpressionType.LambdaExpression);

            var expression = ruleExpressionBuilder.BuildExpressionForRuleParam(param, typeParameterExpressions);

            return expression;
        }
    }
}
