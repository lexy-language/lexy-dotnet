using System;
using System.Collections.Generic;
using System.Linq;
using Lexy.Compiler.Compiler.CSharp.BuiltInFunctions;
using Lexy.Compiler.Compiler.CSharp.ExpressionStatementExceptions;
using Lexy.Compiler.Language.Expressions;
using Lexy.Compiler.Language.Expressions.Functions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Lexy.Compiler.Compiler.CSharp.Syntax;

internal static class Expressions
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

    public static IEnumerable<StatementSyntax> ExecuteExpressionStatementSyntax(IEnumerable<Expression> lines, bool createScope)
    {
        var result = new List<StatementSyntax>();
        if (createScope)
        {
            result.Add(LogCalls.UseLastNodeAsScope());
        }

        result.AddRange(lines.SelectMany(ExecuteStatementSyntax));

        if (createScope)
        {
            result.Add(LogCalls.RevertToParentScope());
        }

        return result;
    }

    private static StatementSyntax[] ExecuteStatementSyntax(Expression expression)
    {
        var statements = new List<StatementSyntax>()
        {
            LogCalls.LogLineAndVariables(expression)
        };

        statements.AddRange(ExpressionStatementSyntax(expression));
        statements.Add(LogCalls.LogAssignmentVariables(expression));

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
            .Select(TranslateCase)
            .ToList();

        return SwitchStatement(ExpressionSyntax(switchExpression.Condition))
            .WithSections(List(cases));
    }

    private static SwitchSectionSyntax TranslateCase(CaseExpression expression)
    {
        var statements = new List<StatementSyntax> { LogCalls.LogLineAndVariables(expression) };
        statements.AddRange(ExecuteExpressionStatementSyntax(expression.Expressions, true));
        return SwitchSection()
            .WithLabels(
                SingletonList(
                    !expression.IsDefault
                        ? CaseSwitchLabel(ExpressionSyntax(expression.Value))
                        : (SwitchLabelSyntax)DefaultSwitchLabel()))
            .WithStatements(
                List(
                    new StatementSyntax[]
                    {
                        Block(List(statements)),
                        BreakStatement()
                    }));
    }

    private static StatementSyntax TranslateIfExpression(IfExpression ifExpression)
    {
        ElseClauseSyntax elseStatement = null;
        for (int index = ifExpression.ElseExpressions.Count - 1; index >= 0; index--)
        {
            var expression = ifExpression.ElseExpressions[index];
            elseStatement = TranslateElseExpression(expression, elseStatement);
        }

        var ifStatement = IfStatement(
            ExpressionSyntax(ifExpression.Condition),
            Block(
                List(ExecuteExpressionStatementSyntax(ifExpression.TrueExpressions, true))));

        return elseStatement != null ? ifStatement.WithElse(elseStatement) : ifStatement;
    }

    private static ElseClauseSyntax TranslateElseExpression(Expression expression, ElseClauseSyntax elseStatement)
    {
        switch (expression)
        {
            case ElseExpression elseExpression:
            {
                var statements = LogAndExecuteStatement(elseExpression, elseExpression.FalseExpressions);
                return ElseClause(Block(List(statements)));
            }
            case ElseIfExpression elseifExpression:
            {
                var statements = LogAndExecuteStatement(elseifExpression, elseifExpression.TrueExpressions);
                var ifStatement = IfStatement(
                    ExpressionSyntax(elseifExpression.Condition),
                    Block(List(statements)));

                return ElseClause(elseStatement != null
                    ? ifStatement.WithElse(elseStatement)
                    : ifStatement);
            }
            default:
                throw new InvalidOperationException($"Invalid ElseExpression type: '{expression.GetType().Name}'");
        }
    }

    private static List<StatementSyntax> LogAndExecuteStatement(Expression expression, IEnumerable<Expression> statementsExpressions)
    {
        var statements = new List<StatementSyntax> { LogCalls.LogLineAndVariables(expression) };
        statements.AddRange(ExecuteExpressionStatementSyntax(statementsExpressions, true));
        return statements;
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
            IdentifierExpression expression => VariableReferences.Translate(expression.Variable),
            MemberAccessExpression expression => VariableReferences.Translate(expression.Variable),
            BinaryExpression expression => TranslateBinaryExpression(expression),
            ParenthesizedExpression expression => ParenthesizedExpression(ExpressionSyntax(expression.Expression)),
            FunctionCallExpression expression => TranslateFunctionCallExpression(expression),
            _ => throw new InvalidOperationException($"Wrong expression type {line.GetType()}: {line}")
        };
    }

    private static ExpressionSyntax TranslateFunctionCallExpression(FunctionCallExpression expression)
    {
        return FunctionCallFactory.CallExpressionSyntax(expression);
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
}