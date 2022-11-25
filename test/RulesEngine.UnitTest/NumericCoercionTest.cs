// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using RulesEngine.HelperFunctions;
using RulesEngine.Models;
using Xunit;

namespace RulesEngine.UnitTest {
	[ExcludeFromCodeCoverage]
	public class NumericCoercionTest {
		[Fact]
		public void NumericCoercionTest_TypesAreCoercedCorrectly() {
			Assert.Equal(typeof(int), Utils.CoerceNumericTypes(typeof(int), typeof(Int32)));
			Assert.Equal(typeof(int), Utils.CoerceNumericTypes(typeof(sbyte), typeof(ushort)));
			Assert.Equal(typeof(short), Utils.CoerceNumericTypes(typeof(SByte), typeof(byte)));
			Assert.Equal(typeof(byte), Utils.CoerceNumericTypes(typeof(byte), typeof(byte)));
			Assert.Equal(typeof(ulong), Utils.CoerceNumericTypes(typeof(uint), typeof(ulong)));
			Assert.Equal(typeof(ushort), Utils.CoerceNumericTypes(typeof(byte), typeof(UInt16)));
			Assert.Equal(typeof(double), Utils.CoerceNumericTypes(typeof(Double), typeof(ulong)));
			Assert.Equal(typeof(decimal), Utils.CoerceNumericTypes(typeof(long), typeof(ulong)));
			Assert.Equal(typeof(float), Utils.CoerceNumericTypes(typeof(Single), typeof(long)));
			Assert.Equal(typeof(float), Utils.CoerceNumericTypes(typeof(float), typeof(UInt64)));
			Assert.Equal(typeof(double), Utils.CoerceNumericTypes(typeof(double), typeof(Int64)));
		}

		[Fact]
		public async Task NumericCoercionTest_ReturnsExpectedResults() {
			var workflowName = "NumericCoercionTestWorkflow";
			var workflow = new Workflow {
				WorkflowName = workflowName,
				Rules = new Rule[] {
										new Rule {
												RuleName = "NumericCoercionTestRule",
												RuleExpressionType = RuleExpressionType.LambdaExpression,
												Expression = "test.values.Any(values => values <= -5.6)"
										}
								}
			};
			var engine = new RulesEngine();
			engine.AddOrUpdateWorkflow(workflow);

			{
				// This data should PASS. Rule requires at least one value < -5.6
				dynamic passDataStaticArray = new ExpandoObject();
				passDataStaticArray.values = new[] { -1, -2, -3, -4, -5, -5.499, -5.5, -5.61 };
				var input_pass_static = new RuleParameter("test", passDataStaticArray);

				// This data should FAIL. Rule requires at least one value < -5.6
				dynamic failDataStaticArray = new ExpandoObject();
				failDataStaticArray.values = new[] { -1, -2, -3, -4, -5, -5.499, -5.5 };
				var input_fail_static = new RuleParameter("test", failDataStaticArray);

				var pass_results_static = await engine.ExecuteAllRulesAsync(workflowName, input_pass_static);
				var fail_results_static = await engine.ExecuteAllRulesAsync(workflowName, input_fail_static);
				Assert.True(pass_results_static.First().IsSuccess);
				Assert.False(fail_results_static.First().IsSuccess);
			}

			{
				// This data should FAIL. Rule requires at least one value < -5.6
				// BUT ... without the coercion, it will PASS, as the array will become an int array,
				// and -5.5 will, weirdly, become -6, even though "(int)-5.5" is "-5".
				// This is because the Utils code uses Convert.ChangeType(), which will
				// convert -5.5 to -6.
				dynamic failDataDynamicArray = new ExpandoObject();
				failDataDynamicArray.values = new List<dynamic> { 12, -1, -2, (ushort)12, -3, -4, -5, -5.499, -5.5 };
				var input_fail_dynamic = new RuleParameter("test", failDataDynamicArray);

				// This data should PASS. Rule requires at least one value < -5.6
				dynamic passDataDynamicArray = new ExpandoObject();
				passDataDynamicArray.values = new List<dynamic> { -1, -2, -3, -4, -5, -5.499, -5.5, -5.61 };
				var input_pass_dynamic = new RuleParameter("test", passDataDynamicArray);

				var pass_results_dynamic = await engine.ExecuteAllRulesAsync(workflowName, input_pass_dynamic);
				var fail_results_dynamic = await engine.ExecuteAllRulesAsync(workflowName, input_fail_dynamic);
				Assert.True(pass_results_dynamic.First().IsSuccess);
				Assert.False(fail_results_dynamic.First().IsSuccess);
			}
		}
	}
}
