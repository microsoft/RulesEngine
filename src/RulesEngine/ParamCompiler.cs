using RulesEngine.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using RulesEngine.ExpressionBuilders;
using RulesEngine.HelperFunctions;

namespace RulesEngine
{
    /// <summary>
    /// Rule param compilers
    /// </summary>
    internal class ParamCompiler
    {

        private readonly ReSettings _reSettings;
        private readonly RuleExpressionParser _ruleExpressionParser;

        internal ParamCompiler(ReSettings reSettings, RuleExpressionParser ruleExpressionParser)
        {
            _reSettings = reSettings;
            _ruleExpressionParser = ruleExpressionParser;
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
                var compiledParamDelegate = GetDelegateForRuleParam(param, ruleParams.ToArray());
                var evaluatedParam = EvaluateCompiledParam(param.Name, compiledParamDelegate, ruleParams);
                compiledParameters.Add(new CompiledParam { Name = param.Name, Value = compiledParamDelegate, ReturnType = evaluatedParam.Type });
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
        public RuleParameter EvaluateCompiledParam(string paramName, Func<object[],object> compiledParam, IEnumerable<RuleParameter> inputs)
        {
            var result = compiledParam(inputs.Select(c => c.Value).ToArray());
            return new RuleParameter(paramName, result);
        }


        /// <summary>
        /// Gets the expression for rule.
        /// </summary>
        /// <param name="param">The rule.</param>
        /// <param name="typeParameterExpressions">The type parameter expressions.</param>
        /// <param name="ruleInputExp">The rule input exp.</param>
        /// <returns></returns>
        private Func<object[],object> GetDelegateForRuleParam(LocalParam param, RuleParameter[] ruleParameters)
        {
            return _ruleExpressionParser.Compile<object>(param.Expression, ruleParameters);
        }
    }
}
