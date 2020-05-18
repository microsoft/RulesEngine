// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Rules.HelperFunctions;
using Xunit;

namespace Microsoft.Rules.UnitTest
{
    [Trait("Category", "Unit")]
    public class ExpressionUtilsTest
    {
        [Fact]
        public void CheckContainsTest()
        {
            var result = ExpressionUtils.CheckContains("", "");
            Assert.False(result);

            result = ExpressionUtils.CheckContains(null, "");
            Assert.False(result);

            result = ExpressionUtils.CheckContains("4", "1,2,3,4,5");
            Assert.True(result);

            result = ExpressionUtils.CheckContains("6", "1,2,3,4,5");
            Assert.False(result);
        }
    }
}
