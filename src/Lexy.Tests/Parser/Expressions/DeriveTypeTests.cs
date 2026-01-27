using System;
using Lexy.Compiler.Language;
using Lexy.Compiler.Parser;
using Lexy.Compiler.Parser.Context;
using Lexy.Compiler.Parser.Logging;
using Lexy.Tests.Parser.ExpressionParser;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Shouldly;
using Type = Lexy.Compiler.Language.TypeSystem.Type;
using ValueType = Lexy.Compiler.Language.TypeSystem.ValueType;

namespace Lexy.Tests.Parser.Expressions;

public class DeriveTypeTests : ScopedServicesTestFixture
{
    [Test]
    public void NumberLiteral()
    {
        var type = DeriveType("5");
        type.ShouldBe(ValueType.Number);
    }

    [Test]
    public void StringLiteral()
    {
        var type = DeriveType(@"""abc""");
        type.ShouldBe(ValueType.String);
    }

    [Test]
    public void BooleanLiteral()
    {
        var type = DeriveType(@"true");
        type.ShouldBe(ValueType.Boolean);
    }

    [Test]
    public void BooleanLiteralFalse()
    {
        var type = DeriveType(@"false");
        type.ShouldBe(ValueType.Boolean);
    }

    [Test]
    public void DateTimeLiteral()
    {
        var type = DeriveType(@"d""2024-12-24T10:05:00""");
        type.ShouldBe(ValueType.Date);
    }

    [Test]
    public void NumberCalculationLiteral()
    {
        var type = DeriveType(@"5 + 5");
        type.ShouldBe(ValueType.Number);
    }

    [Test]
    public void StringConcatLiteral()
    {
        var type = DeriveType(@"""abc"" + ""def""");
        type.ShouldBe(ValueType.String);
    }

    [Test]
    public void BooleanLogicalLiteral()
    {
        var type = DeriveType(@"true && false");
        type.ShouldBe(ValueType.Boolean);
    }

    [Test]
    public void StringVariable()
    {
        var type = DeriveType(@"a", context =>
        {
            context.VariableContext.RegisterVariableAndVerifyUnique(NewReference(), "a", ValueType.String,
                VariableSource.Results);
        });

        type.ShouldBe(ValueType.String);
    }

    [Test]
    public void NumberVariable()
    {
        var type = DeriveType(@"a", context =>
        {
            context.VariableContext.RegisterVariableAndVerifyUnique(NewReference(), "a", ValueType.Number,
                VariableSource.Results);
        });
        type.ShouldBe(ValueType.Number);
    }

    [Test]
    public void BooleanVariable()
    {
        var type = DeriveType(@"a", context =>
        {
            context.VariableContext.RegisterVariableAndVerifyUnique(NewReference(), "a", ValueType.Boolean,
                VariableSource.Results);
        });
        type.ShouldBe(ValueType.Boolean);
    }

    [Test]
    public void DateTimeVariable()
    {
        var type = DeriveType(@"a", context =>
        {
            context.VariableContext.RegisterVariableAndVerifyUnique(NewReference(), "a", ValueType.Date,
                VariableSource.Results);
        });
        type.ShouldBe(ValueType.Date);
    }

    [Test]
    public void StringVariableConcat()
    {
        var type = DeriveType(@"a + ""bc""", context =>
        {
            context.VariableContext.RegisterVariableAndVerifyUnique(NewReference(), "a", ValueType.String,
                VariableSource.Results);
        });
        type.ShouldBe(ValueType.String);
    }

    [Test]
    public void NumberVariableCalculation()
    {
        var type = DeriveType(@"a + 20", context =>
        {
            context.VariableContext.RegisterVariableAndVerifyUnique(NewReference(), "a", ValueType.Number,
                VariableSource.Results);
        });
        type.ShouldBe(ValueType.Number);
    }

    [Test]
    public void NumberVariableWithParenthesisCalculation()
    {
        var type = DeriveType(@"(a + 20.05) * 3", context =>
        {
            context.VariableContext.RegisterVariableAndVerifyUnique(NewReference(), "a", ValueType.Number,
                VariableSource.Results);
        });
        type.ShouldBe(ValueType.Number);
    }

    private static SourceReference NewReference()
    {
        return new SourceReference("tests.lexy", 1, 1, 1);
    }

    private Type DeriveType(string expressionValue, Action<IValidationContext> validationContextHandler = null)
    {
        var logger = new ParserLogger(ServiceProvider.GetRequiredService<ILogger<LexyParser>>());
        var visitor = new TrackLoggingCurrentNodeVisitor(logger);
        var validationContext = new ValidationContext(logger, new ComponentNodeList(), visitor, new Lexy.Compiler.FunctionLibraries.Libraries());

        using var _ = validationContext.CreateVariableScope();

        validationContextHandler?.Invoke(validationContext);

        var expression = this.ParseExpression(expressionValue);
        return expression.DeriveType(validationContext);
    }
}
