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
			Assert.Equal(typeof(System.Nullable<bool>), Utils.CoerceTypes(typeof(bool), null));
			Assert.Equal(typeof(System.Nullable<bool>), Utils.CoerceTypes(typeof(System.Nullable<bool>), typeof(bool)));
			Assert.Equal(typeof(System.Nullable<bool>), Utils.CoerceTypes(typeof(System.Nullable<bool>), null));
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
			Assert.Equal(typeof(object), Utils.CoerceTypes(typeof(char), typeof(int)));
			Assert.Equal(typeof(object), Utils.CoerceTypes(typeof(bool), typeof(int)));
			Assert.Equal(typeof(object), Utils.CoerceTypes(typeof(char), typeof(Enum)));
			Assert.Equal(typeof(System.Nullable<char>), Utils.CoerceTypes(typeof(char), null));
		}

		[Fact]
		public async Task NumericCoercionTest_ReturnsExpectedResults() {
			var workflowName = "NumericCoercionTestWorkflow";
			var testObjectName = "test";

			var workflow = new Workflow {
				WorkflowName = workflowName,
				Rules = new Rule[] {
										new Rule {
												RuleName = "NumericCoercionTestRuleWithoutCast",
												RuleExpressionType = RuleExpressionType.LambdaExpression,
												Expression = $"{testObjectName}.values.Any(it != null AND it <= -5.6)"
										},
										new Rule {
												RuleName = "NumericCoercionTestRuleWithCast",
												RuleExpressionType = RuleExpressionType.LambdaExpression,
												Expression = @$"{testObjectName}.values.Cast(""System.Nullable`1[System.Double]"").Any(it != null AND it <= -5.6)"
										}
								}
			};
			var engine = new RulesEngine();
			engine.AddOrUpdateWorkflow(workflow);

			async Task RunTest(object inputValues, int sampleSize, bool[] expectedResults, bool[] noExceptionExpecteds) {
				dynamic inputDataObject = new ExpandoObject();
				inputDataObject.values = inputValues;
				var inputDynamic = new RuleParameter(testObjectName, inputDataObject, sampleSize);
				var results = await engine.ExecuteAllRulesAsync(workflowName, inputDynamic);
				int index = 0;
				foreach (var result in results) {
					var expectedResult = expectedResults[index];
					var noExceptionExpected = noExceptionExpecteds[index++];
					if (noExceptionExpected)
						Assert.Equal(string.Empty, result.ExceptionMessage);
					else
						Assert.NotEqual(string.Empty, result.ExceptionMessage);
					Assert.True(result.IsSuccess == expectedResult);
				}
			}

			// Using a non-dynamic array will work. The runtime will treat this as an array of
			// doubles. Even though we are only using a sample size of 1, that is enough to
			// convince the engine that the collection is a collection of doubles.
			// So this will PASS since there ARE values < -5.6
			await RunTest(new[] { -1, -2, -3.0, -4.0, -5.0, -5.499, -5.5, -5.60001 }, 1, new[] { true, true }, new[] { true, true });
			// And this will FAIL since there are NO values < -5.6
			await RunTest(new[] { -1, -2, -3.0, -4.0, -5.0, -5.499, -5.5 }, 1, new[] { false, false }, new[] { true, true });

			// This test should PASS. We are passing a sampleSize of 1, which should mean that the
			// type detection code will look at only the first list element (an int), and convert
			// the entire list to ints. Subsequently, the -5.5 value (which should make the "< -5.6"
			// condition FAIL) will be converted to -6, causing it to PASS.
			// Note that -5.5 will, weirdly, become -6, even though a simple explicit cast of
			// "(int)-5.5" is actually "-5". This is because the Utils code uses
			// Convert.ChangeType(), which does a different style of conversion, and will convert
			// -5.5 to -6 for some reason.
			// The casting rule will fail though, as it will be attempting to cast ints to nullable
			// doubles.
			await RunTest(new List<dynamic> { -1, -2, -3.0, -4.0, -5.0, -5.499, -5.5 }, 1, new[] { true, false }, new[] { true, false });
			// Same test, but with a sample size of 0 (meaning "sample all data"). Since later
			// values are doubles, this will treat all values as doubles, and so we will get the
			// correct result of PASS (since there is a value < -5.6).
			await RunTest(new List<dynamic> { -1, -2, -3.0, -4.0, -5.0, -5.499, -5.5, -5.5, -5.60001 }, 0, new[] { true, true }, new[] { true, true });
			// Same test again, but without the < -5.6 value.
			await RunTest(new List<dynamic> { -1, -2, -3.0, -4.0, -5.0, -5.499, -5.5, -5.5 }, 0, new[] { false, false }, new[] { true, true });

			// Now we'll throw in a null value.
			// Again, this test will sample only the first element (an int), which will then make
			// the code attempt to convert the entire list to ints. This will throw an exception
			// when it hits the null.
			await Assert.ThrowsAsync<System.NotSupportedException>(async () => await RunTest(new List<dynamic> { -1, -2, -3.0, null, -4.0, -5.0, -5.499, -5.5, -5.5 }, 1, new[] { false, false }, new[] { true, true }));
			// But by checking the entire set of data, the type detection will decide that the
			// data should be treated as System.Nullable<System.Double>.
			// This test should FAIL simply because it doesn't have a value < -5.6
			await RunTest(new List<dynamic> { -1, -2, -3.0, null, -4.0, -5.0, -5.499, -5.5, -5.5 }, 0, new[] { false, false }, new[] { true, true });
			// This test should PASS because it DOES have a value < -5.6
			await RunTest(new List<dynamic> { -1, -2, -3.0, null, -4.0, -5.0, -5.499, -5.5, -5.5, -5.60001 }, 0, new[] { true, true }, new[] { true, true });

			// Now make sure it copes with type1 -> null -> type2 -> null -> etc
			// This test should FAIL simply because it doesn't have a value < -5.6
			await RunTest(new List<dynamic> { -1, null, -2, -3.0, null, -4.0, -5.0, -5.499, -5.5, -5.5 }, 0, new[] { false, false }, new[] { true, true });
			// This test should PASS because it DOES have a value < -5.6
			await RunTest(new List<dynamic> { -1, null, -2, -3.0, null, -4.0, -5.0, -5.499, -5.5, -5.5, -5.60001 }, 0, new[] { true, true }, new[] { true, true });
		}
	}
}
