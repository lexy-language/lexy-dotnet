using System;
using Lexy.Compiler.Language.Expressions;
using Lexy.Compiler.Language.VariableTypes;
using Lexy.Compiler.Language.VariableTypes.Declaration;
using NUnit.Framework;
using Shouldly;

namespace Lexy.Tests.Parser.ExpressionParser;

public class VariableDeclaration : ScopedServicesTestFixture
{
    [Test]
    public void Number()
    {
        var expression = this.ParseExpression("number temp");
        expression.ValidateOfType<VariableDeclarationExpression>(variableDeclarationExpression =>
        {
            variableDeclarationExpression.Type.ValidateOfType<PrimitiveVariableTypeDeclaration>(type =>
                type.Type.ShouldBe("number"));
            variableDeclarationExpression.Name.ShouldBe("temp");
            variableDeclarationExpression.Assignment.ShouldBeNull();
        });
    }

    [Test]
    public void NumberWithDefaultValue()
    {
        var expression = this.ParseExpression("number temp = 123.45");
        expression.ValidateOfType<VariableDeclarationExpression>(variableDeclarationExpression =>
        {
            variableDeclarationExpression.Type.ValidateOfType<PrimitiveVariableTypeDeclaration>(type =>
                type.Type.ShouldBe("number"));
            variableDeclarationExpression.Name.ShouldBe("temp");
            variableDeclarationExpression.Assignment.ValidateNumericLiteralExpression(123.45m);
        });
    }

    [Test]
    public void String()
    {
        var expression = this.ParseExpression("string temp");
        expression.ValidateOfType<VariableDeclarationExpression>(variableDeclarationExpression =>
        {
            variableDeclarationExpression.Type.ValidateOfType<PrimitiveVariableTypeDeclaration>(type =>
                type.Type.ShouldBe("string"));
            variableDeclarationExpression.Name.ShouldBe("temp");
            variableDeclarationExpression.Assignment.ShouldBeNull();
        });
    }

    [Test]
    public void StringWithDefaultValue()
    {
        var expression = this.ParseExpression(@"string temp = ""abc""");
        expression.ValidateOfType<VariableDeclarationExpression>(variableDeclarationExpression =>
        {
            variableDeclarationExpression.Type.ValidateOfType<PrimitiveVariableTypeDeclaration>(type =>
                type.Type.ShouldBe("string"));
            variableDeclarationExpression.Name.ShouldBe("temp");
            variableDeclarationExpression.Assignment.ValidateQuotedLiteralExpression("abc");
        });
    }


    [Test]
    public void Boolean()
    {
        var expression = this.ParseExpression("boolean temp");
        expression.ValidateOfType<VariableDeclarationExpression>(variableDeclarationExpression =>
        {
            variableDeclarationExpression.Type.ValidateOfType<PrimitiveVariableTypeDeclaration>(type =>
                type.Type.ShouldBe("boolean"));
            variableDeclarationExpression.Name.ShouldBe("temp");
            variableDeclarationExpression.Assignment.ShouldBeNull();
        });
    }

    [Test]
    public void BooleanWithDefaultValue()
    {
        var expression = this.ParseExpression(@"boolean temp = true");
        expression.ValidateOfType<VariableDeclarationExpression>(variableDeclarationExpression =>
        {
            variableDeclarationExpression.Type.ValidateOfType<PrimitiveVariableTypeDeclaration>(type =>
                type.Type.ShouldBe("boolean"));
            variableDeclarationExpression.Name.ShouldBe("temp");
            variableDeclarationExpression.Assignment.ValidateBooleanLiteralExpression(true);
        });
    }

    [Test]
    public void DateTime()
    {
        var expression = this.ParseExpression("date temp");
        expression.ValidateOfType<VariableDeclarationExpression>(variableDeclarationExpression =>
        {
            variableDeclarationExpression.Type.ValidateOfType<PrimitiveVariableTypeDeclaration>(type =>
                type.Type.ShouldBe("date"));
            variableDeclarationExpression.Name.ShouldBe("temp");
            variableDeclarationExpression.Assignment.ShouldBeNull();
        });
    }

    [Test]
    public void DateTimeWithDefaultValue()
    {
        var expression = this.ParseExpression(@"date temp = d""2024-12-16T16:51:12""");
        expression.ValidateOfType<VariableDeclarationExpression>(variableDeclarationExpression =>
        {
            variableDeclarationExpression.Type.ValidateOfType<PrimitiveVariableTypeDeclaration>(type =>
                type.Type.ShouldBe("date"));
            variableDeclarationExpression.Name.ShouldBe("temp");
            variableDeclarationExpression.Assignment.ValidateDateTimeLiteralExpression(new DateTime(2024, 12, 16, 16, 51, 12));
        });
    }

    [Test]
    public void DeclaredType()
    {
        var expression = this.ParseExpression("Custom temp");
        expression.ValidateOfType<VariableDeclarationExpression>(variableDeclarationExpression =>
        {
            variableDeclarationExpression.Type.ValidateOfType<ComplexVariableTypeDeclaration>(type =>
                type.Type.ShouldBe("Custom"));
            variableDeclarationExpression.Name.ShouldBe("temp");
            variableDeclarationExpression.Assignment.ShouldBeNull();
        });
    }

    [Test]
    public void CustomTypeWithDefault()
    {
        var expression = this.ParseExpression("Custom temp = Custom.First");
        expression.ValidateOfType<VariableDeclarationExpression>(variableDeclarationExpression =>
        {
            variableDeclarationExpression.Type.ValidateOfType<ComplexVariableTypeDeclaration>(type =>
                type.Type.ShouldBe("Custom"));
            variableDeclarationExpression.Name.ShouldBe("temp");
            variableDeclarationExpression.Assignment.ValidateMemberAccessExpression("Custom.First");
        });
    }
}