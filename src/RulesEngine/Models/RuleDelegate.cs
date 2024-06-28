// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace RulesEngine.Models
{
    public delegate T RuleFunc<T>(params RuleParameter[] ruleParameters);
}
