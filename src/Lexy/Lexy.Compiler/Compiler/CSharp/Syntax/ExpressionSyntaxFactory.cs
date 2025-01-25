using System;
using System.Collections.Generic;
using System.Linq;
using Lexy.Compiler.Compiler.CSharp.BuiltInFunctions;
using Lexy.Compiler.Compiler.CSharp.ExpressionStatementExceptions;
using Lexy.Compiler.Language;
using Lexy.Compiler.Language.Expressions;
using Lexy.Compiler.Language.VariableTypes;
using Lexy.RunTime;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Lexy.Compiler.Compiler.CSharp;

internal static class ExpressionSyntaxFactory
{
    private static readonly IDictionary<ExpressionOperator, SyntaxKind> TranslateOperators =
        new Dictionary<ExpressionOperator, SyntaxKind>
        {
            { ExpressionOperator.Addition, SyntaxKind.AddExpression },
            { ExpressionOperator.Subtraction, SyntaxKind.SubtractExpression },
            { ExpressionOperator.Multiplication, SyntaxKind.MultiplyExpression },
            { ExpressionOperator.Division, SyntaxKind.DivideExpression },
            { ExpressionOperator.Modulus, SyntaxKind.ModuloExpression },

            { ExpressionOperator.GreaterThan, SyntaxKind.GreaterThanExpression },
            { ExpressionOperator.GreaterThanOrEqual, SyntaxKind.GreaterThanOrEqualExpression },
            { ExpressionOperator.LessThan, SyntaxKind.LessThanExpression },
            { ExpressionOperator.LessThanOrEqual, SyntaxKind.LessThanOrEqualExpression },

            { ExpressionOperator.And, SyntaxKind.LogicalAndExpression },
            { ExpressionOperator.Or, SyntaxKind.LogicalOrExpression },
            { ExpressionOperator.Equals, SyntaxKind.EqualsExpression },
            { ExpressionOperator.NotEqual, SyntaxKind.NotEqualsExpression }
        };

    private static readonly IEnumerable<IExpressionStatementException> RenderStatementExceptions =
        new IExpressionStatementException[]
        {
            new NewFunctionExpressionStatementException(),
            new FillFunctionExpressionStatementException(),
            new ExtractFunctionExpressionStatementException(),
            new SimpleLexyFunctionFunctionExpressionStatementException()
        };

    private static IEnumerable<StatementSyntax> ExecuteExpressionStatementSyntax(IEnumerable<Expression> lines)
    {
        return lines.SelectMany(ExecuteStatementSyntax).ToList();
    }

    public static StatementSyntax[] ExecuteStatementSyntax(Expression expression)
    {
        var statements = new List<StatementSyntax>
        {
            ExpressionStatement(
                InvocationExpression(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            IdentifierName(LexyCodeConstants.ContextVariable),
                            IdentifierName(nameof(IExecutionContext.LogDebug))))
                    .WithArgumentList(
                        ArgumentList(
                            SingletonSeparatedList(
                                Argument(
                                    LiteralExpression(
                                        SyntaxKind.StringLiteralExpression,
                                        Literal(expression.Source.Line.ToString())))))))
        };

        statements.AddRange(ExpressionStatementSyntax(expression));

        return statements.ToArray();
    }

    private static IEnumerable<StatementSyntax> ExpressionStatementSyntax(Expression expression)
    {
        var renderExpressionStatementException =
            RenderStatementExceptions.FirstOrDefault(exception => exception.Matches(expression));

        return renderExpressionStatementException != null
            ? renderExpressionStatementException.CallExpressionSyntax(expression)
            : DefaultExpressionStatementSyntax(expression);
    }

    private static IEnumerable<StatementSyntax> DefaultExpressionStatementSyntax(Expression expression)
    {
        yield return expression switch
        {
            AssignmentExpression assignment => TranslateAssignmentExpression(assignment),
            VariableDeclarationExpression variableDeclarationExpression => TranslateVariableDeclarationExpression(
                variableDeclarationExpression),
            IfExpression ifExpression => TranslateIfExpression(ifExpression),
            SwitchExpression switchExpression => TranslateSwitchExpression(switchExpression),
            FunctionCallExpression functionCallExpression => ExpressionStatement(
                TranslateFunctionCallExpression(functionCallExpression)),
            _ => throw new InvalidOperationException($"Wrong expression type {expression.GetType()}: {expression}")
        };
    }

    private static StatementSyntax TranslateSwitchExpression(SwitchExpression switchExpression)
    {
        var cases = switchExpression.Cases
            .Select(expression =>
                SwitchSection()
                    .WithLabels(
                        SingletonList(
                            !expression.IsDefault
                                ? CaseSwitchLabel(ExpressionSyntax(expression.Value))
                                : (SwitchLabelSyntax)DefaultSwitchLabel()))
                    .WithStatements(
                        List(
                            new StatementSyntax[]
                            {
                                Block(List(ExecuteExpressionStatementSyntax(expression.Expressions))),
                                BreakStatement()
                            })))
            .ToList();

        return SwitchStatement(ExpressionSyntax(switchExpression.Condition))
            .WithSections(List(cases));
    }

    private static StatementSyntax TranslateIfExpression(IfExpression ifExpression)
    {
        var elseStatement = ifExpression.Else != null
            ? ElseClause(
                Block(
                    List(
                        ExecuteExpressionStatementSyntax(ifExpression.Else.FalseExpressions))))
            : null;

        var ifStatement = IfStatement(
            ExpressionSyntax(ifExpression.Condition),
            Block(
                List(ExecuteExpressionStatementSyntax(ifExpression.TrueExpressions))));

        return elseStatement != null ? ifStatement.WithElse(elseStatement) : ifStatement;
    }

    private static ExpressionStatementSyntax TranslateAssignmentExpression(AssignmentExpression assignment)
    {
        return ExpressionStatement(
            AssignmentExpression(
                SyntaxKind.SimpleAssignmentExpression,
                ExpressionSyntax(assignment.Variable),
                ExpressionSyntax(assignment.Assignment)));
    }

    private static StatementSyntax TranslateVariableDeclarationExpression(VariableDeclarationExpression expression)
    {
        var typeSyntax = Types.Syntax(expression.Type);

        var initialize = expression.Assignment != null
            ? ExpressionSyntax(expression.Assignment)
            : Types.TypeDefaultExpression(expression.Type);

        var variable = VariableDeclarator(Identifier(expression.Name))
            .WithInitializer(EqualsValueClause(initialize));

        return LocalDeclarationStatement(
            VariableDeclaration(typeSyntax)
                .WithVariables(SingletonSeparatedList(variable)));
    }

    public static ExpressionSyntax ExpressionSyntax(Expression line)
    {
        return line switch
        {
            LiteralExpression expression => TokenValuesSyntax.Expression(expression.Literal),
            IdentifierExpression expression => IdentifierNameSyntax(expression),
            MemberAccessExpression expression => TranslateMemberAccessExpression(expression),
            BinaryExpression expression => TranslateBinaryExpression(expression),
            ParenthesizedExpression expression => ParenthesizedExpression(ExpressionSyntax(expression.Expression)),
            FunctionCallExpression expression => TranslateFunctionCallExpression(expression),
            _ => throw new InvalidOperationException($"Wrong expression type {line.GetType()}: {line}")
        };
    }

    private static ExpressionSyntax IdentifierNameSyntax(IdentifierExpression expression)
    {
        return FromSource(expression.VariableSource, IdentifierName(expression.Identifier));
    }

    private static ExpressionSyntax FromSource(VariableSource source, SimpleNameSyntax nameSyntax)
    {
        switch (source)
        {
            case VariableSource.Parameters:
                return MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    IdentifierName(LexyCodeConstants.ParameterVariable),
                    nameSyntax);

            case VariableSource.Results:
                return MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    IdentifierName(LexyCodeConstants.ResultsVariable),
                    nameSyntax);

            case VariableSource.Code:
            case VariableSource.Type:
                return nameSyntax;

            case VariableSource.Unknown:
            default:
                throw new ArgumentOutOfRangeException($"source: {source}");
        }
    }

    private static ExpressionSyntax TranslateFunctionCallExpression(FunctionCallExpression expression)
    {
        return FunctionCallFactory.CallExpressionSyntax(expression.ExpressionFunction);
    }

    private static ExpressionSyntax TranslateBinaryExpression(BinaryExpression expression)
    {
        var kind = Translate(expression.Operator);
        return BinaryExpression(
            kind,
            ExpressionSyntax(expression.Left),
            ExpressionSyntax(expression.Right));
    }

    private static SyntaxKind Translate(ExpressionOperator expressionOperator)
    {
        if (!TranslateOperators.TryGetValue(expressionOperator, out var result))
            throw new ArgumentOutOfRangeException(nameof(expressionOperator), expressionOperator, null);

        return result;
    }

    private static ExpressionSyntax TranslateMemberAccessExpression(MemberAccessExpression expression)
    {
        if (expression.Variable.Parts < 2)
            throw new InvalidOperationException($"Invalid MemberAccessExpression: {expression}");

        var rootType = VariableClassName(expression, expression.Variable);
        var childReference = expression.Variable.ChildrenReference();

        ExpressionSyntax result = MemberAccessExpression(
            SyntaxKind.SimpleMemberAccessExpression,
            FromSource(expression.VariableSource, IdentifierName(rootType)),
            IdentifierName(childReference.ParentIdentifier));

        while (childReference.HasChildIdentifiers)
        {
            childReference = childReference.ChildrenReference();
            result = MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                result,
                IdentifierName(childReference.ParentIdentifier));
        }

        return result;
    }

    private static string VariableClassName(MemberAccessExpression expression, VariableReference reference)
    {
        return expression.ParentVariableType switch
        {
            TableType _ => ClassNames.TableClassName(reference.ParentIdentifier),
            FunctionType _ => ClassNames.FunctionClassName(reference.ParentIdentifier),
            EnumType _ => ClassNames.EnumClassName(reference.ParentIdentifier),
            CustomType _ => ClassNames.TypeClassName(reference.ParentIdentifier),
            _ => reference.ParentIdentifier
        };
    }
}