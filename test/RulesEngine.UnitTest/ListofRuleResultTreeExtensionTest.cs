﻿using RulesEngine.Extensions;
using RulesEngine.Models;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace RulesEngine.UnitTest
{
    [Trait("Category", "Unit")]
    public class ListofRuleResultTreeExtensionTest
    {
        [Fact]
        public void OnSuccessWithSuccessTest()
        {
            var rulesResultTree = new List<RuleResultTree>()
            {
                new RuleResultTree()
                {
                    ChildResults = null,
                    ExceptionMessage = string.Empty,
                    Input = new object(),
                    IsSuccess = true,
                    Rule = new Rule()
                    {
                        RuleName = "Test Rule 1"
                    }
                },
                new RuleResultTree()
                {
                    ChildResults = null,
                    ExceptionMessage = string.Empty,
                    Input = new object(),
                    IsSuccess = false,
                    Rule = new Rule()
                    {
                        RuleName = "Test Rule 2"
                    }
                },

            };

            var successEventName = string.Empty;

            rulesResultTree.OnSuccess((eventName) =>
            {
                successEventName = eventName;
            });

            Assert.True(successEventName.Equals("Test Rule 1"));
        }

        [Fact]
        public void OnSuccessWithSuccessWithEventTest()
        {
            var rulesResultTree = new List<RuleResultTree>()
            {
                new RuleResultTree()
                {
                    ChildResults = null,
                    ExceptionMessage = string.Empty,
                    Input = new object(),
                    IsSuccess = true,
                    Rule = new Rule()
                    {
                        RuleName = "Test Rule 1",
                        SuccessEvent = "Event 1"
                    }
                },
                new RuleResultTree()
                {
                    ChildResults = null,
                    ExceptionMessage = string.Empty,
                    Input = new object(),
                    IsSuccess = false,
                    Rule = new Rule()
                    {
                        RuleName = "Test Rule 2"
                    }
                },

            };

            var successEventName = string.Empty;

            rulesResultTree.OnSuccess((eventName) =>
            {
                successEventName = eventName;
            });

            Assert.True(successEventName.Equals("Event 1"));
        }

        [Fact]
        public void OnSuccessWithouSuccessTest()
        {
            var rulesResultTree = new List<RuleResultTree>()
            {
                new RuleResultTree()
                {
                    ChildResults = null,
                    ExceptionMessage = string.Empty,
                    Input = new object(),
                    IsSuccess = false,
                    Rule = new Rule()
                    {
                        RuleName = "Test Rule 1"
                    }
                },
                new RuleResultTree()
                {
                    ChildResults = null,
                    ExceptionMessage = string.Empty,
                    Input = new object(),
                    IsSuccess = false,
                    Rule = new Rule()
                    {
                        RuleName = "Test Rule 2"
                    }
                },

            };

            var successEventName = string.Empty;

            rulesResultTree.OnSuccess((eventName) =>
            {
                successEventName = eventName;
            });

            Assert.True(successEventName.Equals(string.Empty));
        }


        [Fact]
        public void OnFailWithSuccessTest()
        {
            var rulesResultTree = new List<RuleResultTree>()
            {
                new RuleResultTree()
                {
                    ChildResults = null,
                    ExceptionMessage = string.Empty,
                    Input = new object(),
                    IsSuccess = true,
                    Rule = new Rule()
                    {
                        RuleName = "Test Rule 1"
                    }
                },
                new RuleResultTree()
                {
                    ChildResults = null,
                    ExceptionMessage = string.Empty,
                    Input = new object(),
                    IsSuccess = false,
                    Rule = new Rule()
                    {
                        RuleName = "Test Rule 2"
                    }
                },

            };

            var successEventName = true;

            rulesResultTree.OnFail(() =>
            {
                successEventName = false;
            });

            Assert.True(successEventName);
        }

        [Fact]
        public void OnFailWithoutSuccessTest()
        {
            var rulesResultTree = new List<RuleResultTree>()
            {
                new RuleResultTree()
                {
                    ChildResults = null,
                    ExceptionMessage = string.Empty,
                    Input = new object(),
                    IsSuccess = false,
                    Rule = new Rule()
                    {
                        RuleName = "Test Rule 1"
                    }
                },
                new RuleResultTree()
                {
                    ChildResults = null,
                    ExceptionMessage = string.Empty,
                    Input = new object(),
                    IsSuccess = false,
                    Rule = new Rule()
                    {
                        RuleName = "Test Rule 2"
                    }
                },

            };

            var successEventName = true;

            rulesResultTree.OnFail(() =>
            {
                successEventName = false;
            });

            Assert.False(successEventName);
        }
    }
}
