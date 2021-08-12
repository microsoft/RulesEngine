// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using RulesEngine.Actions;
using RulesEngine.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RulesEngine.UnitTest.ActionTests.MockClass
{
    public class ReturnContextAction : ActionBase
    {
        public override ValueTask<object> Run(ActionContext context, RuleParameter[] ruleParameters)
        {
            var stringContext = context.GetContext<string>("stringContext");
            var intContext = context.GetContext<int>("intContext");
            var objectContext = context.GetContext<object>("objectContext");

            return new ValueTask<object>(new {
                stringContext,
                intContext,
                objectContext
            });
        }
    }
}
