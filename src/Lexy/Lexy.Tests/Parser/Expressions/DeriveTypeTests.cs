using System;
using Lexy.Compiler.Language;
using Lexy.Compiler.Language.VariableTypes;
using Lexy.Compiler.Parser;
using Lexy.Tests.Parser.ExpressionParser;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
            context.VariableContext.RegisterVariableAndVerifyUnique(NewReference(), "a", PrimitiveType.String,
                VariableSource.Results);
        });

        type.ShouldBe(PrimitiveType.String);
    }

    [Test]
    public void NumberVariable()
    {
        var type = DeriveType(@"a", context =>
        {
            context.VariableContext.RegisterVariableAndVerifyUnique(NewReference(), "a", PrimitiveType.Number,
                VariableSource.Results);
        });
        type.ShouldBe(PrimitiveType.Number);
    }

    [Test]
    public void BooleanVariable()
    {
        var type = DeriveType(@"a", context =>
        {
            context.VariableContext.RegisterVariableAndVerifyUnique(NewReference(), "a", PrimitiveType.Boolean,
                VariableSource.Results);
        });
        type.ShouldBe(PrimitiveType.Boolean);
    }

    [Test]
    public void DateTimeVariable()
    {
        var type = DeriveType(@"a", context =>
        {
            context.VariableContext.RegisterVariableAndVerifyUnique(NewReference(), "a", PrimitiveType.Date,
                VariableSource.Results);
        });
        type.ShouldBe(PrimitiveType.Date);
    }

    [Test]
    public void StringVariableConcat()
    {
        var type = DeriveType(@"a + ""bc""", context =>
        {
            context.VariableContext.RegisterVariableAndVerifyUnique(NewReference(), "a", PrimitiveType.String,
                VariableSource.Results);
        });
        type.ShouldBe(PrimitiveType.String);
    }

    [Test]
    public void NumberVariableCalculation()
    {
        var type = DeriveType(@"a + 20", context =>
        {
            context.VariableContext.RegisterVariableAndVerifyUnique(NewReference(), "a", PrimitiveType.Number,
                VariableSource.Results);
        });
        type.ShouldBe(PrimitiveType.Number);
    }

    [Test]
    public void NumberVariableWithParenthesisCalculation()
    {
        var type = DeriveType(@"(a + 20.05) * 3", context =>
        {
            context.VariableContext.RegisterVariableAndVerifyUnique(NewReference(), "a", PrimitiveType.Number,
                VariableSource.Results);
        });
        type.ShouldBe(PrimitiveType.Number);
    }

    private static SourceReference NewReference()
    {
        return new SourceReference(new SourceFile("tests.lexy"), 1, 1);
    }

    private VariableType DeriveType(string expressionValue, Action<IValidationContext> validationContextHandler = null)
    {
        var logger = new ParserLogger(ServiceProvider.GetRequiredService<ILogger<LexyParser>>());
        var validationContext = new ValidationContext(logger, new RootNodeList());

        using var _ = validationContext.CreateVariableScope();

        validationContextHandler?.Invoke(validationContext);

        var expression = this.ParseExpression(expressionValue);
        return expression.DeriveType(validationContext);
    }
}