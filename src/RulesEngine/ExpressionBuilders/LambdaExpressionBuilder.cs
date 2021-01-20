﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using RulesEngine.HelperFunctions;
using RulesEngine.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace RulesEngine.ExpressionBuilders
{
    /// <summary>
    /// This class will build the list expression
    /// </summary>
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
                return Helpers.ToResultTree(rule, null, ruleDelegate);
            }
            catch (Exception ex)
            {
                ex.Data.Add(nameof(rule.RuleName), rule.RuleName);
                ex.Data.Add(nameof(rule.Expression), rule.Expression);

                if (!_reSettings.EnableExceptionAsErrorMessage) throw;
              
                bool func(object[] param) => false;
                var exceptionMessage = $"Exception while parsing expression `{rule?.Expression}` - {ex.Message}";
                return Helpers.ToResultTree(rule, null,func, exceptionMessage);
            }
        }

        internal override LambdaExpression Parse(string expression, ParameterExpression[] parameters, Type returnType)
        {
            return _ruleExpressionParser.Parse(expression, parameters, returnType);
        }

        internal override Func<object[],Dictionary<string,object>> CompileScopedParams(RuleParameter[] ruleParameters, RuleExpressionParameter[] scopedParameters)
        {
            return _ruleExpressionParser.CompileRuleExpressionParameters(ruleParameters, scopedParameters);
        }
    }
}
