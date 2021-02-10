// Copyright (c) Microsoft Corporation.
//  Licensed under the MIT License.

using AutoFixture;
using RulesEngine.Actions;
using RulesEngine.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace RulesEngine.UnitTest
{
    [ExcludeFromCodeCoverage]
    public class ActionContextTests
    {
        [Fact]
        public void GetParentRuleResult_ReturnsParentRule()
        {
            // Arrange
            var fixture = new Fixture();
            var contextInput = fixture.Create<string>();
            var context = new Dictionary<string, object> {
                { nameof(contextInput), contextInput }
            };
            var parentRuleResult = new RuleResultTree();

            var actionContext = new ActionContext(context, parentRuleResult);

            // Act
            var result = actionContext.GetParentRuleResult();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(parentRuleResult, result);
        }

        [Fact]
        public void GetContext_ValidName_ReturnsContext()
        {
            // Arrange
            var fixture = new Fixture();
            var contextInput = fixture.Create<string>();
            var context = new Dictionary<string, object> {
                { nameof(contextInput), contextInput }
            };
            var parentRuleResult = new RuleResultTree();

            var actionContext = new ActionContext(context, parentRuleResult);
            string name = nameof(contextInput);

            // Act
            var result = actionContext.GetContext<string>(name);

            // Assert
            Assert.Equal(contextInput, result);
        }

        [Fact]
        public void GetContext_ObjectContext_ReturnsTypedContext()
        {
            // Arrange
            var fixture = new Fixture();
            var contextInput = fixture.CreateMany<string>();
            var context = new Dictionary<string, object> {
                { nameof(contextInput), contextInput }
            };
            var parentRuleResult = new RuleResultTree();


            var actionContext = new ActionContext(context, parentRuleResult);
            string name = nameof(contextInput);

            // Act
            var result = actionContext.GetContext<List<string>>(name);

            // Assert
            Assert.Equal(contextInput, result);
        }

        [Fact]
        public void GetContext_ValidNameWithStringCaseDiffernce_ReturnsContext()
        {
            // Arrange
            var fixture = new Fixture();
            var contextInput = fixture.Create<string>();
            var context = new Dictionary<string, object> {
                { nameof(contextInput), contextInput }
            };
            var parentRuleResult = new RuleResultTree();

            var actionContext = new ActionContext(context, parentRuleResult);
            string name = nameof(contextInput).ToUpper();

            // Act
            var result = actionContext.GetContext<string>(name);

            // Assert
            Assert.Equal(contextInput, result);
        }

        [Fact]
        public void GetContext_InvalidName_ThrowsArgumentException()
        {
            // Arrange
            var fixture = new Fixture();
            var contextInput = fixture.Create<string>();
            var context = new Dictionary<string, object> {
                { nameof(contextInput), contextInput }
            };
            var parentRuleResult = new RuleResultTree();

            var actionContext = new ActionContext(context, parentRuleResult);
            string name = fixture.Create<string>();

            // Act
            Assert.Throws<ArgumentException>(() => actionContext.GetContext<string>(name));
        }

        [Fact]
        public void GetContext_PrimitiveInputs_ReturnsResult()
        {
            // Arrange
            var fixture = new Fixture();
            var intInput = fixture.Create<int>();
            var strInput = fixture.Create<string>();
            var floatInput = fixture.Create<float>();

            var context = new Dictionary<string, object> {
                { nameof(intInput), intInput },
                { nameof(strInput), strInput },
                { nameof(floatInput), floatInput },
            };
            var parentRuleResult = new RuleResultTree();

            var actionContext = new ActionContext(context, parentRuleResult);

            // Act
            var intResult = actionContext.GetContext<int>(nameof(intInput));
            var strResult = actionContext.GetContext<string>(nameof(strInput));
            var floatResult = actionContext.GetContext<float>(nameof(floatInput));

            // Assert
            Assert.Equal(intInput, intResult);
            Assert.Equal(strInput, strResult);
            Assert.Equal(floatInput, floatResult);
        }

        [Fact]
        public void GetContext_InvalidNameListContext_ThrowsArgumentException()
        {
            // Arrange
            var fixture = new Fixture();
            var contextInput = fixture.CreateMany<string>();
            var context = new Dictionary<string, object> {
                { nameof(contextInput), contextInput }
            };
            var parentRuleResult = new RuleResultTree();

            var actionContext = new ActionContext(context, parentRuleResult);
            string name = fixture.Create<string>();

            // Act
            Assert.Throws<ArgumentException>(() => actionContext.GetContext<List<string>>(name));
        }

        [Fact]
        public void GetContext_InvalidTypeConversion_ThrowsArgumentException()
        {
            // Arrange
            var fixture = new Fixture();
            var contextInput = fixture.CreateMany<string>();
            var context = new Dictionary<string, object> {
                { nameof(contextInput), contextInput }
            };
            var parentRuleResult = new RuleResultTree();

            var actionContext = new ActionContext(context, parentRuleResult);
            string name = nameof(contextInput);

            // Act
            Assert.Throws<ArgumentException>(() => actionContext.GetContext<RuleResultTree>(name));
        }
    }
}
