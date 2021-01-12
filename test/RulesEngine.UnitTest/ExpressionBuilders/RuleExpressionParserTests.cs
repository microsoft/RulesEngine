//using Moq;
//using RulesEngine.ExpressionBuilders;
//using RulesEngine.Models;
//using System;
//using System.Diagnostics.CodeAnalysis;
//using System.Linq.Expressions;
//using Xunit;

//namespace RulesEngine.UnitTest.ExpressionBuilders
//{
//    [ExcludeFromCodeCoverage]
//    public class RuleExpressionParserTests
//    {
//        private MockRepository mockRepository;

//        private Mock<ReSettings> mockReSettings;

//        public RuleExpressionParserTests()
//        {
//            this.mockRepository = new MockRepository(MockBehavior.Strict);

//            this.mockReSettings = this.mockRepository.Create<ReSettings>();
//        }

//        private RuleExpressionParser CreateRuleExpressionParser()
//        {
//            return new RuleExpressionParser(
//                this.mockReSettings.Object);
//        }

//        [Fact]
//        public void Parse_StateUnderTest_ExpectedBehavior()
//        {
//            // Arrange
//            var ruleExpressionParser = this.CreateRuleExpressionParser();
//            string expression = null;
//            ParameterExpression[] parameters = null;
//            Type returnType = null;

//            // Act
//            var result = ruleExpressionParser.Parse(
//                expression,
//                parameters,
//                returnType);

//            // Assert
//            Assert.True(false);
//            this.mockRepository.VerifyAll();
//        }

//        [Fact]
//        public void Compile_StateUnderTest_ExpectedBehavior()
//        {
//            // Arrange
//            var ruleExpressionParser = this.CreateRuleExpressionParser();
//            string expression = null;
//            RuleParameter[] ruleParams = null;
//            RuleExpressionParameter[] ruleExpParams = null;

//            // Act
//            var result = ruleExpressionParser.Compile<bool>(
//                expression,
//                ruleParams,
//                ruleExpParams);

//            // Assert
//            Assert.True(false);
//            this.mockRepository.VerifyAll();
//        }

//        [Fact]
//        public void Evaluate_StateUnderTest_ExpectedBehavior()
//        {
//            // Arrange
//            var ruleExpressionParser = this.CreateRuleExpressionParser();
//            string expression = null;
//            RuleParameter[] ruleParams = null;
//            RuleExpressionParameter[] ruleExpParams = null;

//            // Act
//            var result = ruleExpressionParser.Evaluate<bool>(
//                expression,
//                ruleParams,
//                ruleExpParams);

//            // Assert
//            Assert.True(false);
//            this.mockRepository.VerifyAll();
//        }
//    }
//}
