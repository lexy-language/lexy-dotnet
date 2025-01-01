using System;
using Lexy.Compiler.Language.Types;
using Lexy.Compiler.Parser;
using Lexy.Tests.Parser.ExpressionParser;
using NUnit.Framework;
using Shouldly;

namespace Lexy.Tests.Parser.Expressions;

public class DeriveTypeTests : ScopedServicesTestFixture
{
    [Test]
    public void NumberLiteral()
    {
        var type = DeriveType("5");
        type.ShouldBe(PrimitiveType.Number);
    }

    [Test]
    public void StringLiteral()
    {
        var type = DeriveType(@"""abc""");
        type.ShouldBe(PrimitiveType.String);
    }

    [Test]
    public void BooleanLiteral()
    {
        var type = DeriveType(@"true");
        type.ShouldBe(PrimitiveType.Boolean);
    }

    [Test]
    public void BooleanLiteralFalse()
    {
        var type = DeriveType(@"false");
        type.ShouldBe(PrimitiveType.Boolean);
    }

    [Test]
    public void DateTimeLiteral()
    {
        var type = DeriveType(@"d""2024-12-24T10:05:00""");
        type.ShouldBe(PrimitiveType.Date);
    }

    [Test]
    public void NumberCalculationLiteral()
    {
        var type = DeriveType(@"5 + 5");
        type.ShouldBe(PrimitiveType.Number);
    }

    [Test]
    public void StringConcatLiteral()
    {
        var type = DeriveType(@"""abc"" + ""def""");
        type.ShouldBe(PrimitiveType.String);
    }

    [Test]
    public void BooleanLogicalLiteral()
    {
        var type = DeriveType(@"true && false");
        type.ShouldBe(PrimitiveType.Boolean);
    }

    [Test]
    public void StringVariable()
    {
        var type = DeriveType(@"a", context =>
        {
            var reference = new SourceReference(new SourceFile("tests.lexy"), 1, 1);
            context.VariableContext.RegisterVariableAndVerifyUnique(reference, "a", PrimitiveType.String,
                VariableSource.Results);
        });

        type.ShouldBe(PrimitiveType.String);
    }

    [Test]
    public void NumberVariable()
    {
        var type = DeriveType(@"a", context =>
        {
            var reference = new SourceReference(new SourceFile("tests.lexy"), 1, 1);
            context.VariableContext.RegisterVariableAndVerifyUnique(reference, "a", PrimitiveType.Number,
                VariableSource.Results);
        });
        type.ShouldBe(PrimitiveType.Number);
    }

    [Test]
    public void BooleanVariable()
    {
        var type = DeriveType(@"a", context =>
        {
            var reference = new SourceReference(new SourceFile("tests.lexy"), 1, 1);
            context.VariableContext.RegisterVariableAndVerifyUnique(reference, "a", PrimitiveType.Boolean,
                VariableSource.Results);
        });
        type.ShouldBe(PrimitiveType.Boolean);
    }

    [Test]
    public void DateTimeVariable()
    {
        var type = DeriveType(@"a", context =>
        {
            var reference = new SourceReference(new SourceFile("tests.lexy"), 1, 1);
            context.VariableContext.RegisterVariableAndVerifyUnique(reference, "a", PrimitiveType.Date,
                VariableSource.Results);
        });
        type.ShouldBe(PrimitiveType.Date);
    }

    [Test]
    public void StringVariableConcat()
    {
        var type = DeriveType(@"a + ""bc""", context =>
        {
            var reference = new SourceReference(new SourceFile("tests.lexy"), 1, 1);
            context.VariableContext.RegisterVariableAndVerifyUnique(reference, "a", PrimitiveType.String,
                VariableSource.Results);
        });
        type.ShouldBe(PrimitiveType.String);
    }

    [Test]
    public void NumberVariableCalculation()
    {
        var type = DeriveType(@"a + 20", context =>
        {
            var reference = new SourceReference(new SourceFile("tests.lexy"), 1, 1);
            context.VariableContext.RegisterVariableAndVerifyUnique(reference, "a", PrimitiveType.Number,
                VariableSource.Results);
        });
        type.ShouldBe(PrimitiveType.Number);
    }

    [Test]
    public void NumberVariableWithParenthesisCalculation()
    {
        var type = DeriveType(@"(a + 20.05) * 3", context =>
        {
            var reference = new SourceReference(new SourceFile("tests.lexy"), 1, 1);
            context.VariableContext.RegisterVariableAndVerifyUnique(reference, "a", PrimitiveType.Number,
                VariableSource.Results);
        });
        type.ShouldBe(PrimitiveType.Number);
    }

    private VariableType DeriveType(string expressionValue, Action<IValidationContext> validationContextHandler = null)
    {
        var parserContext = GetService<IParserContext>();
        var validationContext = new ValidationContext(parserContext.Logger, parserContext.Nodes);
        using var _ = validationContext.CreateVariableScope();

        validationContextHandler?.Invoke(validationContext);

        var expression = this.ParseExpression(expressionValue);
        return expression.DeriveType(validationContext);
    }
}