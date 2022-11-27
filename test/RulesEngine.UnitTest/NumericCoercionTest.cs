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
			Assert.Equal(typeof(int), Utils.CoerceTypes(typeof(int), typeof(Int32)));
			Assert.Equal(typeof(int), Utils.CoerceTypes(typeof(sbyte), typeof(ushort)));
			Assert.Equal(typeof(short), Utils.CoerceTypes(typeof(SByte), typeof(byte)));
			Assert.Equal(typeof(byte), Utils.CoerceTypes(typeof(byte), typeof(byte)));
			Assert.Equal(typeof(ulong), Utils.CoerceTypes(typeof(uint), typeof(ulong)));
			Assert.Equal(typeof(ushort), Utils.CoerceTypes(typeof(byte), typeof(UInt16)));
			Assert.Equal(typeof(double), Utils.CoerceTypes(typeof(Double), typeof(ulong)));
			Assert.Equal(typeof(decimal), Utils.CoerceTypes(typeof(long), typeof(ulong)));
			Assert.Equal(typeof(System.Nullable<decimal>), Utils.CoerceTypes(typeof(decimal), null));
			Assert.Equal(typeof(float), Utils.CoerceTypes(typeof(Single), typeof(long)));
			Assert.Equal(typeof(float), Utils.CoerceTypes(typeof(float), typeof(UInt64)));
			Assert.Equal(typeof(double), Utils.CoerceTypes(typeof(double), typeof(Int64)));
			Assert.Equal(typeof(System.Nullable<double>), Utils.CoerceTypes(typeof(double), null));
		}

		[Fact]
		public async Task NumericCoercionTest_ReturnsExpectedResults() {
			var workflowName = "NumericCoercionTestWorkflow";
			var workflow = new Workflow {
				WorkflowName = workflowName,
				Rules = new Rule[] {
										new Rule {
												RuleName = "NumericCoercionTestRuleWithoutCast",
												RuleExpressionType = RuleExpressionType.LambdaExpression,
												Expression = "test.values.Any(it != null AND it <= -5.6)"
										},
										new Rule {
												RuleName = "NumericCoercionTestRuleWithCast",
												RuleExpressionType = RuleExpressionType.LambdaExpression,
												Expression = "test.values.Cast(\"System.Nullable`1[System.Double]\").Any(it != null AND it <= -5.6)"
										}
								}
			};
			var engine = new RulesEngine();
			engine.AddOrUpdateWorkflow(workflow);

			{
				// This data should PASS. Rule requires at least one value < -5.6
				dynamic passDataStaticArray = new ExpandoObject();
				passDataStaticArray.values = new[] { -1, -2, -3, -4, -5, -5.499, -5.5, -5.61 };
				// Passing 0 for the TypeDetectionSampleSize means "sample all data please".
				var input_pass_static = new RuleParameter("test", passDataStaticArray, 0);

				// This data should FAIL. Rule requires at least one value < -5.6
				dynamic failDataStaticArray = new ExpandoObject();
				failDataStaticArray.values = new[] { -1, -2, -3, -4, -5, -5.499, -5.5 };
				// Passing 0 for the TypeDetectionSampleSize means "sample all data please".
				var input_fail_static = new RuleParameter("test", failDataStaticArray, 0);

				var pass_results_static = await engine.ExecuteAllRulesAsync(workflowName, input_pass_static);
				var fail_results_static = await engine.ExecuteAllRulesAsync(workflowName, input_fail_static);
				Assert.True(pass_results_static.First().IsSuccess);
				Assert.False(fail_results_static.First().IsSuccess);
			}

			async Task RunDynamicTest(List<dynamic> passData, List<dynamic> failData) {
				// This data should PASS. Rule requires at least one value < -5.6
				dynamic passDataDynamicArray = new ExpandoObject();
				passDataDynamicArray.values = passData;
				// Passing 0 for the TypeDetectionSampleSize means "sample all data please".
				var input_pass_dynamic = new RuleParameter("test", passDataDynamicArray, 0);

				// This data should FAIL. Rule requires at least one value < -5.6
				// BUT ... without the coercion, it will PASS, as the array will become an int array,
				// and -5.5 will, weirdly, become -6, even though "(int)-5.5" is "-5".
				// This is because the Utils code uses Convert.ChangeType(), which will
				// convert -5.5 to -6.
				dynamic failDataDynamicArray = new ExpandoObject();
				failDataDynamicArray.values = failData;
				// Passing 0 for the TypeDetectionSampleSize means "sample all data please".
				var input_fail_dynamic = new RuleParameter("test", failDataDynamicArray, 0);

				var pass_results_dynamic = await engine.ExecuteAllRulesAsync(workflowName, input_pass_dynamic);
				var fail_results_dynamic = await engine.ExecuteAllRulesAsync(workflowName, input_fail_dynamic);
				Assert.True(pass_results_dynamic.First().IsSuccess);
				Assert.False(fail_results_dynamic.First().IsSuccess);
			}

			// Simple numeric coercion test.
			// Type detected should be "double".
			await RunDynamicTest(new List<dynamic> { -1, -2, -3.0, -4.0, -5.0, -5.499, -5.5, -5.61 }, new List<dynamic> { -1, -2, -3.0, -4.0, -5.0, -5.499, -5.5 });
			// Check that we can add a null into the mix okay.
			// Type detected should be "System.Nullable<System.Double>"
			await RunDynamicTest(new List<dynamic> { -1, -2, -3.0, null, -4.0, -5.0, -5.499, -5.5, -5.61 }, new List<dynamic> { -1, -2, -3.0, null, -4.0, -5.0, -5.499, -5.5 });
		}
	}
}
