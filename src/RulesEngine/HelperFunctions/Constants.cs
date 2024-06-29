// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace RulesEngine.HelperFunctions
{
    /// <summary>
    /// Constants
    /// </summary>
    public static class Constants
    {
        public const string WORKFLOW_NAME_NULL_ERRMSG = "Workflow name can not be null or empty";
        public const string INJECT_WORKFLOW_RULES_ERRMSG = "Atleast one of Rules or WorkflowsToInject must be not empty";
        public const string RULE_CATEGORY_CONFIGURED_ERRMSG = "Rule Category should be configured";
        public const string RULE_NULL_ERRMSG = "Rules can not be null or zero";
        public const string NESTED_RULE_NULL_ERRMSG = "Nested rules can not be null";
        public const string NESTED_RULE_CONFIGURED_ERRMSG = "Nested rules can not be configured";
        public const string OPERATOR_NULL_ERRMSG = "Operator can not be null";
        public const string OPERATOR_INCORRECT_ERRMSG = "Operator {PropertyValue} is not allowed";
        public const string RULE_NAME_NULL_ERRMSG = "Rule Name can not be null";
        public const string OPERATOR_RULES_ERRMSG = "Cannot use Rules field when Operator is null";
        public const string LAMBDA_EXPRESSION_EXPRESSION_NULL_ERRMSG = "Expression cannot be null or empty when RuleExpressionType is LambdaExpression";
        public const string LAMBDA_EXPRESSION_OPERATOR_ERRMSG = "Cannot use Operator field when RuleExpressionType is LambdaExpression";
        public const string LAMBDA_EXPRESSION_RULES_ERRMSG = "Cannot use Rules field when RuleExpressionType is LambdaExpression";

    }
}
