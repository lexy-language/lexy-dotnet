using System;
using System.Collections.Generic;
using System.Linq;
using Lexy.Compiler.Compiler.CSharp.ExpressionStatementExceptions;
using Lexy.Compiler.Language.Expressions;
using Lexy.Compiler.Language.Types;
using Lexy.Compiler.Parser;
using Lexy.RunTime.RunTime;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Lexy.Compiler.Compiler.CSharp
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

        private static readonly IEnumerable<IExpressionStatementException> RenderStatementExceptions = new IExpressionStatementException[]
        {
            new NewFunctionExpressionStatementException(),
            new FillFunctionExpressionStatementException(),
            new ExtractFunctionExpressionStatementException(),
            new SimpleLexyFunctionFunctionExpressionStatementException()
        };

        private static IEnumerable<StatementSyntax> ExecuteExpressionStatementSyntax(IEnumerable<Expression> lines, ICompileFunctionContext context)
        {
            return lines.SelectMany(expression => ExecuteStatementSyntax(expression, context)).ToList();
        }

        public static StatementSyntax[] ExecuteStatementSyntax(Expression expression,
            ICompileFunctionContext context)
        {
            var statements = new List<StatementSyntax>()
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
                                            Literal(expression.Source.Line.ToString()))))))),
            };

            statements.AddRange(ExpressionStatementSyntax(expression, context));

            return statements.ToArray();
        }

        private static IEnumerable<StatementSyntax> ExpressionStatementSyntax(Expression expression, ICompileFunctionContext context)
        {
            var renderExpressionStatementException =
                RenderStatementExceptions.FirstOrDefault(exception => exception.Matches(expression));

            return renderExpressionStatementException != null
                ? renderExpressionStatementException.CallExpressionSyntax(expression, context)
                : DefaultExpressionStatementSyntax(expression, context);
        }

        private static IEnumerable<StatementSyntax> DefaultExpressionStatementSyntax(Expression expression, ICompileFunctionContext context)
        {
            yield return expression switch
            {
                AssignmentExpression assignment => TranslateAssignmentExpression(assignment, context),
                VariableDeclarationExpression variableDeclarationExpression => TranslateVariableDeclarationExpression(
                    variableDeclarationExpression, context),
                IfExpression ifExpression => TranslateIfExpression(ifExpression, context),
                SwitchExpression switchExpression => TranslateSwitchExpression(switchExpression, context),
                FunctionCallExpression functionCallExpression => ExpressionStatement(
                    TranslateFunctionCallExpression(functionCallExpression, context)),
                _ => throw new InvalidOperationException($"Wrong expression type {expression.GetType()}: {expression}")
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
                    List(ExecuteExpressionStatementSyntax(ifExpression.TrueExpressions, context))));

            return elseStatement != null ? ifStatement.WithElse(elseStatement) : ifStatement;
        }

        private static ExpressionStatementSyntax TranslateAssignmentExpression(AssignmentExpression assignment, ICompileFunctionContext context)
        {
            return ExpressionStatement(
                AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression,
                    ExpressionSyntax(assignment.Variable, context),
                    ExpressionSyntax(assignment.Assignment, context)));
        }

        private static StatementSyntax TranslateVariableDeclarationExpression(VariableDeclarationExpression expression,
            ICompileFunctionContext context)
        {
            var typeSyntax = Types.Syntax(expression.Type);

            var initialize = expression.Assignment != null
                ? ExpressionSyntax(expression.Assignment, context)
                : TypeDefaultExpression(expression.Type, typeSyntax);

            var variable = VariableDeclarator(Identifier(expression.Name))
                .WithInitializer(EqualsValueClause(initialize));

            return LocalDeclarationStatement(
                VariableDeclaration(typeSyntax)
                    .WithVariables(SingletonSeparatedList(variable)));
        }

        public static ExpressionSyntax TypeDefaultExpression(VariableDeclarationType variableDeclarationType, TypeSyntax typeSyntax)
        {
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
                IdentifierExpression expression => IdentifierNameSyntax(expression),
                MemberAccessExpression expression => TranslateMemberAccessExpression(expression),
                BinaryExpression expression => TranslateBinaryExpression(expression, context),
                ParenthesizedExpression expression => ParenthesizedExpression(ExpressionSyntax(expression.Expression, context)),
                FunctionCallExpression expression => TranslateFunctionCallExpression(expression, context),
                _ => throw new InvalidOperationException($"Wrong expression type {line.GetType()}: {line}")
            };
        }

        private static ExpressionSyntax IdentifierNameSyntax(IdentifierExpression expression)
        {
            switch (expression.VariableSource)
            {
                case VariableSource.Parameters:
                    return MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName(LexyCodeConstants.ParameterVariable),
                        IdentifierName(expression.Identifier));

                case VariableSource.Results:
                    return MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName(LexyCodeConstants.ResultsVariable),
                        IdentifierName(expression.Identifier));

                case VariableSource.Code:
                    return IdentifierName(expression.Identifier);

                case VariableSource.Unknown:
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static ExpressionSyntax TranslateFunctionCallExpression(FunctionCallExpression expression, ICompileFunctionContext context)
        {
            var functionCall = context.Get(expression.ExpressionFunction);
            if (functionCall == null)
            {
                throw new InvalidOperationException($"Function call not found: {expression.FunctionName}");
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

            var rootType = expression.RootType is TableType ? ClassNames.TableClassName(parts[0]) : parts[0];

            ExpressionSyntax result = MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                IdentifierName(rootType),
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