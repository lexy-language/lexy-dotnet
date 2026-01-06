using System;
using System.Collections.Generic;
using System.Linq;
using Lexy.Compiler.Generation.CSharp.FunctionCalls;
using Lexy.Compiler.Language.Expressions;
using Lexy.Compiler.Language.Expressions.Functions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Lexy.Compiler.Generation.CSharp.Syntax.BinaryExpressionsSyntax;
using ExpressionStatement = Lexy.Compiler.Generation.CSharp.ExpressionStatements.ExpressionStatement;

namespace Lexy.Compiler.Generation.CSharp.Syntax;

internal static class Expressions
{
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
        var renderExpressionStatementRule = ExpressionStatement.GetCreator(expression);

        return renderExpressionStatementRule != null
            ? renderExpressionStatementRule(expression)
            : DefaultExpressionStatementSyntax(expression);
    }

    private static IEnumerable<StatementSyntax> DefaultExpressionStatementSyntax(Expression expression)
    {
        yield return expression switch
        {
            AssignmentExpression assignment => AssignmentExpressionSyntax(assignment),
            VariableDeclarationExpression variableDeclarationExpression => VariableDeclarationExpressionSyntax(
                variableDeclarationExpression),
            IfExpression ifExpression => IfExpressionSyntax(ifExpression),
            SwitchExpression switchExpression => SwitchExpressionSyntax(switchExpression),
            FunctionCallExpression functionCallExpression => ExpressionStatement(
                FunctionCallExpressionSyntax(functionCallExpression)),
            _ => throw new InvalidOperationException($"Wrong expression type {expression.GetType()}: {expression}")
        };
    }

    private static StatementSyntax SwitchExpressionSyntax(SwitchExpression switchExpression)
    {
        var cases = switchExpression.Cases
            .Select(CaseSyntax)
            .ToList();

        return SwitchStatement(ExpressionSyntax(switchExpression.Condition))
            .WithSections(List(cases));
    }

    private static SwitchSectionSyntax CaseSyntax(CaseExpression expression)
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

    private static StatementSyntax IfExpressionSyntax(IfExpression ifExpression)
    {
        ElseClauseSyntax elseStatement = null;
        for (int index = ifExpression.ElseExpressions.Count - 1; index >= 0; index--)
        {
            var expression = ifExpression.ElseExpressions[index];
            elseStatement = ElseExpressionSyntax(expression, elseStatement);
        }

        var ifStatement = IfStatement(
            ExpressionSyntax(ifExpression.Condition),
            Block(
                List(ExecuteExpressionStatementSyntax(ifExpression.TrueExpressions, true))));

        return elseStatement != null ? ifStatement.WithElse(elseStatement) : ifStatement;
    }

    private static ElseClauseSyntax ElseExpressionSyntax(Expression expression, ElseClauseSyntax elseStatement)
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

    private static ExpressionStatementSyntax AssignmentExpressionSyntax(AssignmentExpression assignment)
    {
        return ExpressionStatement(
            AssignmentExpression(
                SyntaxKind.SimpleAssignmentExpression,
                ExpressionSyntax(assignment.Variable),
                ExpressionSyntax(assignment.Assignment)));
    }

    private static StatementSyntax VariableDeclarationExpressionSyntax(VariableDeclarationExpression expression)
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
            IdentifierExpression expression => VariableReferences.Syntax(expression.Variable),
            MemberAccessExpression expression => VariableReferences.Syntax(expression.Variable),
            BinaryExpression expression => BinaryExpressionSyntax(expression),
            ParenthesizedExpression expression => ParenthesizedExpression(ExpressionSyntax(expression.Expression)),
            FunctionCallExpression expression => FunctionCallExpressionSyntax(expression),
            _ => throw new InvalidOperationException($"Wrong expression type {line.GetType()}: {line}")
        };
    }

    private static ExpressionSyntax FunctionCallExpressionSyntax(FunctionCallExpression expression)
    {
        return FunctionCallSyntax.CreateExpressionSyntax(expression);
    }
}