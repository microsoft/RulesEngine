// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using RulesEngine.Exceptions;
using RulesEngine.HelperFunctions;
using RulesEngine.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core.Exceptions;
using System.Linq.Expressions;

namespace RulesEngine.ExpressionBuilders
{
    internal sealed class LambdaExpressionBuilder : RuleExpressionBuilderBase
    {
        private readonly ReSettings _reSettings;
        private readonly RuleExpressionParser _ruleExpressionParser;

        internal LambdaExpressionBuilder(ReSettings reSettings, RuleExpressionParser ruleExpressionParser)
        {
            _reSettings = reSettings;
            _ruleExpressionParser = ruleExpressionParser;
        }

        internal override RuleFunc<RuleResultTree> BuildDelegateForRule(Rule rule, RuleParameter[] ruleParams)
        {
            try
            {
                var ruleDelegate = _ruleExpressionParser.Compile<bool>(rule.Expression, ruleParams);
                return Helpers.ToResultTree(_reSettings, rule, null, ruleDelegate);
            }
            catch (Exception ex)
            {
                Helpers.HandleRuleException(ex,rule,_reSettings);

                var detail = ex.Message;
                if (detail != null
                    && (detail.Contains("exists in type 'Object'")
                        || detail.Contains("'System.Object'"))
                    && (rule?.Expression?.Contains('(') == true))
                {
                    // Dynamic.Core can only resolve members and operators against a static return type.
                    // If a custom/static method's declared return type is `object`, member access or
                    // operator usage on its result fails. See #717.
                    detail += " (Hint: a method called in this expression appears to have an `object` return type. " +
                              "Change its return type to the concrete class — Dynamic.Core cannot resolve members or operators on `object`.)";
                }

                var exceptionMessage = Helpers.GetExceptionMessage($"Exception while parsing expression `{rule?.Expression}` - {detail}",
                                                                    _reSettings);

                bool func(object[] param) => false;

                return Helpers.ToResultTree(_reSettings, rule, null,func, exceptionMessage);
            }
        }

        internal override Expression Parse(string expression, ParameterExpression[] parameters, Type returnType)
        {
            try
            {
                return _ruleExpressionParser.Parse(expression, parameters, returnType);
            }
            catch(ParseException ex)
            {
                throw new ExpressionParserException(ex.Message, expression);
            }
            
        }

        internal override Func<object[],Dictionary<string,object>> CompileScopedParams(RuleParameter[] ruleParameters, RuleExpressionParameter[] scopedParameters)
        {
            return _ruleExpressionParser.CompileRuleExpressionParameters(ruleParameters, scopedParameters);
        }
    }
}
