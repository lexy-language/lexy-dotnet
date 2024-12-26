using System;
using System.Collections.Generic;
using System.Linq;
using Lexy.Poc.Core.Language;
using Lexy.Poc.Core.Language.Expressions;
using Lexy.Poc.Core.Parser;
using Lexy.Poc.Core.RunTime;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Lexy.Poc.Core.Compiler.CSharp
{
    internal static class ExpressionSyntaxFactory
    {
        private static readonly IDictionary<ExpressionOperator, SyntaxKind> TranslateOperators =
            new Dictionary<ExpressionOperator, SyntaxKind>()
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
                { ExpressionOperator.NotEqual, SyntaxKind.NotEqualsExpression },
            };

        private static IEnumerable<StatementSyntax> ExecuteExpressionStatementSyntax(IEnumerable<Expression> lines, ICompileFunctionContext context)
        {
            return lines.SelectMany(expression => ExecuteStatementSyntax(expression, context)).ToList();
        }

        public static StatementSyntax[] ExecuteStatementSyntax(Expression expression,
            ICompileFunctionContext context)
        {
            return new[]
            {
                ExpressionStatement(
                    InvocationExpression(
                            MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                IdentifierName("context"),
                                IdentifierName(nameof(IExecutionContext.LogDebug))))
                        .WithArgumentList(
                            ArgumentList(
                                SingletonSeparatedList(
                                    Argument(
                                        LiteralExpression(
                                            SyntaxKind.StringLiteralExpression,
                                            Literal(expression.Source.Line.ToString()))))))),
                ExpressionStatementSyntax(expression, context)
            };
        }

        public static StatementSyntax ExpressionStatementSyntax(Expression line, ICompileFunctionContext context)
        {
            return line switch
            {
                AssignmentExpression assignment => TranslateAssignmentExpression(assignment, context),
                VariableDeclarationExpression variableDeclarationExpression => TranslateVariableDeclarationExpression(
                    variableDeclarationExpression, context),
                IfExpression ifExpression => TranslateIfExpression(ifExpression, context),
                SwitchExpression switchExpression => TranslateSwitchExpression(switchExpression, context),
                _ => throw new InvalidOperationException($"Wrong expression type {line.GetType()}: {line}")
            };
        }

        private static StatementSyntax TranslateSwitchExpression(SwitchExpression switchExpression,
            ICompileFunctionContext context)
        {
            var cases = switchExpression.Cases
                .Select(expression =>
                    SwitchSection()
                        .WithLabels(
                            SingletonList(
                                !expression.IsDefault
                                    ? CaseSwitchLabel(ExpressionSyntax(expression.Value, context))
                                    : (SwitchLabelSyntax)DefaultSwitchLabel()))
                        .WithStatements(
                            List(
                                new StatementSyntax[]
                                {
                                    Block(List(ExecuteExpressionStatementSyntax(expression.Expressions, context))),
                                    BreakStatement()
                                })))
                .ToList();

            return SwitchStatement(ExpressionSyntax(switchExpression.Condition, context))
                .WithSections(List(cases));
        }

        private static StatementSyntax TranslateIfExpression(IfExpression ifExpression,
            ICompileFunctionContext context)
        {
            var elseStatement = ifExpression.Else != null
                ? ElseClause(
                    Block(
                        List(
                            ExecuteExpressionStatementSyntax(ifExpression.Else.FalseExpressions, context))))
                : null;

            var ifStatement = IfStatement(
                ExpressionSyntax(ifExpression.Condition, context),
                Block(
                    List(
                        ExecuteExpressionStatementSyntax(ifExpression.TrueExpressions, context))));

            return elseStatement != null ? ifStatement.WithElse(elseStatement) : ifStatement;
        }

        private static ExpressionStatementSyntax TranslateAssignmentExpression(AssignmentExpression assignment, ICompileFunctionContext context)
        {
            return ExpressionStatement(
                AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression,
                    IdentifierName(assignment.VariableName),
                    ExpressionSyntax(assignment.Assignment, context)));
        }

        private static StatementSyntax TranslateVariableDeclarationExpression(VariableDeclarationExpression expression,
            ICompileFunctionContext context)
        {
            var typeSyntax = Types.Syntax(expression.Type);

            var initialize = TypeDefaultExpression(expression.Assignment, expression.Type, typeSyntax, context);

            var variable = VariableDeclarator(
                    Identifier(expression.Name))
                .WithInitializer(EqualsValueClause(initialize));

            return LocalDeclarationStatement(
                VariableDeclaration(typeSyntax)
                    .WithVariables(SingletonSeparatedList(variable)));
        }

        private static ExpressionSyntax TypeDefaultExpression(Expression expressionAssignment,
            VariableDeclarationType variableDeclarationType, TypeSyntax typeSyntax, ICompileFunctionContext context)
        {
            if (expressionAssignment != null) return ExpressionSyntax(expressionAssignment, context);

            return variableDeclarationType switch
            {
                PrimitiveVariableDeclarationType expression => Types.PrimitiveTypeDefaultExpression(expression),
                CustomVariableDeclarationType _ => DefaultExpression(typeSyntax),
                _ => throw new InvalidOperationException(
                    $"Wrong VariableDeclarationType {variableDeclarationType.GetType()}")
            };
        }

        public static ExpressionSyntax ExpressionSyntax(Expression line, ICompileFunctionContext context)
        {
            return line switch
            {
                LiteralExpression expression => TokenValuesSyntax.Expression(expression.Literal),
                IdentifierExpression expression => IdentifierName(expression.Identifier),
                MemberAccessExpression expression => TranslateMemberAccessExpression(expression),
                BinaryExpression expression => TranslateBinaryExpression(expression, context),
                ParenthesizedExpression expression => ParenthesizedExpression(ExpressionSyntax(expression.Expression, context)),
                FunctionCallExpression expression => TranslateFunctionCallExpression(expression, context),
                _ => throw new InvalidOperationException($"Wrong expression type {line.GetType()}: {line}")
            };
        }

        private static ExpressionSyntax TranslateFunctionCallExpression(FunctionCallExpression expression, ICompileFunctionContext context)
        {
            var functionCall = context.Get(expression.BuiltInFunction);
            if (functionCall == null)
            {
                throw new InvalidOperationException("Function all not found: " + expression.FunctionName);
            }

            return functionCall.CallExpressionSyntax(context);
        }

        private static ExpressionSyntax TranslateBinaryExpression(BinaryExpression expression, ICompileFunctionContext context)
        {
            var kind = Translate(expression.Operator);
            return BinaryExpression(
                kind,
                ExpressionSyntax(expression.Left, context),
                ExpressionSyntax(expression.Right, context));
        }

        private static SyntaxKind Translate(ExpressionOperator expressionOperator)
        {
            if (!TranslateOperators.TryGetValue(expressionOperator, out var result))
            {
                throw new ArgumentOutOfRangeException(nameof(expressionOperator), expressionOperator, null);
            }

            return result;
        }

        private static ExpressionSyntax TranslateMemberAccessExpression(MemberAccessExpression expression)
        {
            var parts = expression.Value.Split(TokenValues.MemberAccess);
            if (parts.Length < 2)
            {
                throw new InvalidOperationException($"Invalid MemberAccessExpression: {expression}");
            }

            ExpressionSyntax result = MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                IdentifierName(parts[0]),
                IdentifierName(parts[1]));

            for (var index = 2; index < parts.Length; index++)
            {
                result = MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    result,
                    IdentifierName(parts[1]));
            }

            return result;
        }
    }
}