using System;
using Lexy.Poc.Core.Language;
using Lexy.Poc.Core.Language.Expressions;
using NUnit.Framework;
using Shouldly;

namespace Lexy.Poc.Parser.ExpressionParser
{
    public class VariableDeclaration : ScopedServicesTestFixture
    {
        [Test]
        public void Number()
        {
            var expression = this.ParseExpression("number temp");
            expression.ValidateOfType<VariableDeclarationExpression>(assignmentExpression =>
            {
                assignmentExpression.Type.ValidateOfType<PrimitiveVariableDeclarationType>(type =>
                    type.Type.ShouldBe("number"));
                assignmentExpression.Name.ShouldBe("temp");
                assignmentExpression.Assignment.ShouldBeNull();
            });
        }

        [Test]
        public void NumberWithDefaultValue()
        {
            var expression = this.ParseExpression("number temp = 123.45");
            expression.ValidateOfType<VariableDeclarationExpression>(assignmentExpression =>
            {
                assignmentExpression.Type.ValidateOfType<PrimitiveVariableDeclarationType>(type =>
                    type.Type.ShouldBe("number"));
                assignmentExpression.Name.ShouldBe("temp");
                assignmentExpression.Assignment.ValidateNumericLiteralExpression(123.45m);
            });
        }

        [Test]
        public void String()
        {
            var expression = this.ParseExpression("string temp");
            expression.ValidateOfType<VariableDeclarationExpression>(assignmentExpression =>
            {
                assignmentExpression.Type.ValidateOfType<PrimitiveVariableDeclarationType>(type =>
                    type.Type.ShouldBe("string"));
                assignmentExpression.Name.ShouldBe("temp");
                assignmentExpression.Assignment.ShouldBeNull();
            });
        }

        [Test]
        public void StringWithDefaultValue()
        {
            var expression = this.ParseExpression(@"string temp = ""abc""");
            expression.ValidateOfType<VariableDeclarationExpression>(assignmentExpression =>
            {
                assignmentExpression.Type.ValidateOfType<PrimitiveVariableDeclarationType>(type =>
                    type.Type.ShouldBe("string"));
                assignmentExpression.Name.ShouldBe("temp");
                assignmentExpression.Assignment.ValidateQuotedLiteralExpression("abc");
            });
        }


        [Test]
        public void Boolean()
        {
            var expression = this.ParseExpression("boolean temp");
            expression.ValidateOfType<VariableDeclarationExpression>(assignmentExpression =>
            {
                assignmentExpression.Type.ValidateOfType<PrimitiveVariableDeclarationType>(type =>
                    type.Type.ShouldBe("boolean"));
                assignmentExpression.Name.ShouldBe("temp");
                assignmentExpression.Assignment.ShouldBeNull();
            });
        }

        [Test]
        public void BooleanWithDefaultValue()
        {
            var expression = this.ParseExpression(@"boolean temp = true");
            expression.ValidateOfType<VariableDeclarationExpression>(assignmentExpression =>
            {
                assignmentExpression.Type.ValidateOfType<PrimitiveVariableDeclarationType>(type =>
                    type.Type.ShouldBe("boolean"));
                assignmentExpression.Name.ShouldBe("temp");
                assignmentExpression.Assignment.ValidateBooleanLiteralExpression(true);
            });
        }

        [Test]
        public void DateTime()
        {
            var expression = this.ParseExpression("date temp");
            expression.ValidateOfType<VariableDeclarationExpression>(assignmentExpression =>
            {
                assignmentExpression.Type.ValidateOfType<PrimitiveVariableDeclarationType>(type =>
                    type.Type.ShouldBe("date"));
                assignmentExpression.Name.ShouldBe("temp");
                assignmentExpression.Assignment.ShouldBeNull();
            });
        }

        [Test]
        public void DateTimeWithDefaultValue()
        {
            var expression = this.ParseExpression(@"date temp = d""2024/12/16 16:51:12""");
            expression.ValidateOfType<VariableDeclarationExpression>(assignmentExpression =>
            {
                assignmentExpression.Type.ValidateOfType<PrimitiveVariableDeclarationType>(type =>
                    type.Type.ShouldBe("date"));
                assignmentExpression.Name.ShouldBe("temp");
                assignmentExpression.Assignment.ValidateDateTimeLiteralExpression(new DateTime(2024, 12, 16,16,51, 12));
            });
        }

        [Test]
        public void CustomType()
        {
            var expression = this.ParseExpression("Custom temp");
            expression.ValidateOfType<VariableDeclarationExpression>(assignmentExpression =>
            {
                assignmentExpression.Type.ValidateOfType<CustomVariableDeclarationType>(type =>
                    type.Type.ShouldBe("Custom"));
                assignmentExpression.Name.ShouldBe("temp");
                assignmentExpression.Assignment.ShouldBeNull();
            });
        }

        [Test]
        public void CustomTypeWithDefault()
        {
            var expression = this.ParseExpression("Custom temp = Custom.First");
            expression.ValidateOfType<VariableDeclarationExpression>(assignmentExpression =>
            {
                assignmentExpression.Type.ValidateOfType<CustomVariableDeclarationType>(type =>
                    type.Type.ShouldBe("Custom"));
                assignmentExpression.Name.ShouldBe("temp");
                assignmentExpression.Assignment.ValidateMemberAccessExpression("Custom.First");
            });
        }
    }
}