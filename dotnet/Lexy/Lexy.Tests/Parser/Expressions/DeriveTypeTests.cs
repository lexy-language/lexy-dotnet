using Lexy.Poc.Core.Language;
using Lexy.Poc.Core.Parser;
using Lexy.Poc.Parser.ExpressionParser;
using NUnit.Framework;
using Shouldly;

namespace Lexy.Poc.Parser.Expressions
{
    public class DeriveTypeTests : ScopedServicesTestFixture
    {
        [Test]
        public void NumberLiteral()
        {
            var validationContext = new ValidationContext(GetService<IParserContext>());
            var expression = this.ParseExpression("5");
            var type = expression.DeriveType(validationContext);

            type.ShouldBe(PrimitiveType.Number);
        }

        [Test]
        public void StringLiteral()
        {
            var validationContext = new ValidationContext(GetService<IParserContext>());
            var expression = this.ParseExpression(@"""abc""");
            var type = expression.DeriveType(validationContext);

            type.ShouldBe(PrimitiveType.String);
        }

        [Test]
        public void BooleanLiteral()
        {
            var validationContext = new ValidationContext(GetService<IParserContext>());
            var expression = this.ParseExpression(@"true");
            var type = expression.DeriveType(validationContext);

            type.ShouldBe(PrimitiveType.Boolean);
        }

        [Test]
        public void BooleanLiteralFalse()
        {
            var validationContext = new ValidationContext(GetService<IParserContext>());
            var expression = this.ParseExpression(@"false");
            var type = expression.DeriveType(validationContext);

            type.ShouldBe(PrimitiveType.Boolean);
        }

        [Test]
        public void DateTimeLiteral()
        {
            var validationContext = new ValidationContext(GetService<IParserContext>());
            var expression = this.ParseExpression(@"d""2024/12/24 10:05:00""");
            var type = expression.DeriveType(validationContext);

            type.ShouldBe(PrimitiveType.Date);
        }

        [Test]
        public void NumberCalculationLiteral()
        {
            var validationContext = new ValidationContext(GetService<IParserContext>());
            var expression = this.ParseExpression("5 + 5");
            var type = expression.DeriveType(validationContext);

            type.ShouldBe(PrimitiveType.Number);
        }

        [Test]
        public void StringConcatLiteral()
        {
            var validationContext = new ValidationContext(GetService<IParserContext>());
            var expression = this.ParseExpression(@"""abc"" + ""def""");
            var type = expression.DeriveType(validationContext);

            type.ShouldBe(PrimitiveType.String);
        }

        [Test]
        public void BooleanLogicalLiteral()
        {
            var validationContext = new ValidationContext(GetService<IParserContext>());
            var expression = this.ParseExpression(@"true && false");
            var type = expression.DeriveType(validationContext);

            type.ShouldBe(PrimitiveType.Boolean);
        }

        [Test]
        public void StringVariable()
        {
            var validationContext = new ValidationContext(GetService<IParserContext>());
            using var _ = validationContext.CreateCodeContextScope();
            var reference = new SourceReference(new SourceFile("tests.lexy"), 1, 1);
            validationContext.FunctionCodeContext.RegisterVariableAndVerifyUnique(reference, "a", PrimitiveType.String);

            var expression = this.ParseExpression(@"a");
            var type = expression.DeriveType(validationContext);

            type.ShouldBe(PrimitiveType.String);
        }

        [Test]
        public void NumberVariable()
        {
            var validationContext = new ValidationContext(GetService<IParserContext>());
            using var _ = validationContext.CreateCodeContextScope();
            var reference = new SourceReference(new SourceFile("tests.lexy"), 1, 1);
            validationContext.FunctionCodeContext.RegisterVariableAndVerifyUnique(reference, "a", PrimitiveType.Number);

            var expression = this.ParseExpression(@"a");
            var type = expression.DeriveType(validationContext);

            type.ShouldBe(PrimitiveType.Number);
        }

        [Test]
        public void BooleanVariable()
        {
            var validationContext = new ValidationContext(GetService<IParserContext>());
            using var _ = validationContext.CreateCodeContextScope();
            var reference = new SourceReference(new SourceFile("tests.lexy"), 1, 1);
            validationContext.FunctionCodeContext.RegisterVariableAndVerifyUnique(reference, "a", PrimitiveType.Boolean);

            var expression = this.ParseExpression(@"a");
            var type = expression.DeriveType(validationContext);

            type.ShouldBe(PrimitiveType.Boolean);
        }

        [Test]
        public void DateTimeVariable()
        {
            var validationContext = new ValidationContext(GetService<IParserContext>());
            using var _ = validationContext.CreateCodeContextScope();
            var reference = new SourceReference(new SourceFile("tests.lexy"), 1, 1);
            validationContext.FunctionCodeContext.RegisterVariableAndVerifyUnique(reference, "a", PrimitiveType.Date);

            var expression = this.ParseExpression(@"a");
            var type = expression.DeriveType(validationContext);

            type.ShouldBe(PrimitiveType.Date);
        }

        [Test]
        public void StringVariableConcat()
        {
            var validationContext = new ValidationContext(GetService<IParserContext>());
            using var _ = validationContext.CreateCodeContextScope();
            var reference = new SourceReference(new SourceFile("tests.lexy"), 1, 1);
            validationContext.FunctionCodeContext.RegisterVariableAndVerifyUnique(reference, "a", PrimitiveType.String);

            var expression = this.ParseExpression(@"a + ""bc""");
            var type = expression.DeriveType(validationContext);

            type.ShouldBe(PrimitiveType.String);
        }

        [Test]
        public void NumberVariableCalculation()
        {
            var validationContext = new ValidationContext(GetService<IParserContext>());
            using var _ = validationContext.CreateCodeContextScope();
            var reference = new SourceReference(new SourceFile("tests.lexy"), 1, 1);
            validationContext.FunctionCodeContext.RegisterVariableAndVerifyUnique(reference, "a", PrimitiveType.Number);

            var expression = this.ParseExpression(@"a + 20");
            var type = expression.DeriveType(validationContext);

            type.ShouldBe(PrimitiveType.Number);
        }

        [Test]
        public void NumberVariableWithParenthesisCalculation()
        {
            var validationContext = new ValidationContext(GetService<IParserContext>());
            using var _ = validationContext.CreateCodeContextScope();
            var reference = new SourceReference(new SourceFile("tests.lexy"), 1, 1);
            validationContext.FunctionCodeContext.RegisterVariableAndVerifyUnique(reference, "a", PrimitiveType.Number);

            var expression = this.ParseExpression(@"(a + 20.05) * 3");
            var type = expression.DeriveType(validationContext);

            type.ShouldBe(PrimitiveType.Number);
        }
    }
}