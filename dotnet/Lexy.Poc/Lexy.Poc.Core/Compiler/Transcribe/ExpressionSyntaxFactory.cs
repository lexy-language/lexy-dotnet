using System;
using System.Collections.Generic;
using System.Linq;
using Lexy.Poc.Core.Language;
using Lexy.Poc.Core.Language.Expressions;
using Lexy.Poc.Core.Parser;
using Lexy.Poc.Core.Parser.Tokens;
using Lexy.Poc.Core.RunTime;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Lexy.Poc.Core.Compiler.Transcribe
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

        public static IEnumerable<StatementSyntax> ExecuteExpressionStatementSyntax(IEnumerable<Expression> lines)
        {
            return lines.SelectMany(ExecuteStatementSyntax).ToList();
        }

        public static StatementSyntax[] ExecuteStatementSyntax(Expression expression)
        {
            return new []{
                ExpressionStatement(
                    InvocationExpression(
                            MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                IdentifierName("context"),
                                IdentifierName(nameof(IExecutionContext.LogDebug))))
                        .WithArgumentList(
                            ArgumentList(
                                SingletonSeparatedList<ArgumentSyntax>(
                                    Argument(
                                        LiteralExpression(
                                            SyntaxKind.StringLiteralExpression,
                                            Literal(expression.Source.Line.ToString()))))))),
                ExpressionStatementSyntax(expression)
            };
        }

        public static StatementSyntax ExpressionStatementSyntax(Expression line)
        {
            return line switch
            {
                AssignmentExpression assignment => TranslateAssignmentExpression(assignment),
                VariableDeclarationExpression variableDeclarationExpression => TranslateVariableDeclarationExpression(variableDeclarationExpression),
                IfExpression ifExpression => TranslateIfExpression(ifExpression),
                SwitchExpression switchExpression => TranslateSwitchExpression(switchExpression),
                _ => throw new InvalidOperationException($"Wrong expression type {line.GetType()}: {line}")
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
                                    : (SwitchLabelSyntax) DefaultSwitchLabel()))
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
            var elseStatement = ifExpression.Else != null ? ElseClause(
                Block(
                    List(
                        ExecuteExpressionStatementSyntax(ifExpression.Else.FalseExpressions)
                        ))) : null;

            var ifStatement = IfStatement(
                    ExpressionSyntax(ifExpression.Condition),
                    Block(
                        List(
                            ExecuteExpressionStatementSyntax(ifExpression.TrueExpressions))));

            return elseStatement != null ? ifStatement.WithElse(elseStatement) : ifStatement;
        }

        private static ExpressionStatementSyntax TranslateAssignmentExpression(AssignmentExpression assignment)
        {
            return ExpressionStatement(
                AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression,
                    IdentifierName(assignment.VariableName),
                    ExpressionSyntax(assignment.Assignment)));
        }

        private static StatementSyntax TranslateVariableDeclarationExpression(VariableDeclarationExpression expression)
        {
            var typeSyntax = MapType(expression.Type);

            var initialize = TypeDefaultExpression(expression.Assignment, expression.Type, typeSyntax);

            var variable = VariableDeclarator(
                Identifier(expression.Name))
                .WithInitializer(EqualsValueClause(initialize));

            return LocalDeclarationStatement(
                VariableDeclaration(typeSyntax)
                    .WithVariables(SingletonSeparatedList(variable)));
        }

        private static ExpressionSyntax TypeDefaultExpression(Expression expressionAssignment,
            VariableDeclarationType variableDeclarationType, TypeSyntax typeSyntax)
        {
            if (expressionAssignment != null) return ExpressionSyntax(expressionAssignment);

            return variableDeclarationType switch
            {
                PrimitiveVariableDeclarationType expression => PrimitiveTypeDefaultExpression(expression),
                CustomVariableDeclarationType expression => DefaultExpression(typeSyntax),
                _ => throw new InvalidOperationException($"Wrong VariableDeclarationType {variableDeclarationType.GetType()}")
            };
        }

        public static ExpressionSyntax PrimitiveTypeDefaultExpression(PrimitiveVariableDeclarationType type)
        {
            switch (type.Type)
            {
                case TypeNames.Number:
                case TypeNames.Boolean:
                    var typeSyntax = MapType(type);
                    return DefaultExpression(typeSyntax);

                case TypeNames.String:
                    return LiteralExpression(
                        SyntaxKind.StringLiteralExpression,
                        Literal(""));

                case TypeNames.DateTime:
                    return TranslateDate(DateTypeDefault.Value);

                default:
                    throw new InvalidOperationException("Invalid type: " + type.Type);
            }
        }

        public static ExpressionSyntax ExpressionSyntax(Expression line)
        {
            return line switch
            {
                LiteralExpression expression => TokenValueExpression(expression.Literal),
                VariableExpression expression => IdentifierName(expression.VariableName),
                MemberAccessExpression expression => TranslateMemberAccessExpression(expression),
                BinaryExpression expression => TranslateBinaryExpression(expression),
                ParenthesizedExpression expression => ParenthesizedExpression(ExpressionSyntax(expression.Expression)),
                _ => throw new InvalidOperationException($"Wrong expression type {line.GetType()}: {line}")
            };
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
                throw new InvalidOperationException("Invalid MemberAccessExpression: " + expression);
            }

            ExpressionSyntax result = SyntaxFactory.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                SyntaxFactory.IdentifierName(parts[0]),
                SyntaxFactory.IdentifierName(parts[1]));

            for (var index = 2; index < parts.Length; index++)
            {
                result = SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    result,
                    SyntaxFactory.IdentifierName(parts[1]));
            }

            return result;
        }

        public static ExpressionSyntax TokenValueExpression(ILiteralToken token)
        {
            if (token == null) return null;

            return token switch
            {
                QuotedLiteralToken _ => LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(token.Value)),
                NumberLiteralToken number => LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal($"{number.NumberValue}m", number.NumberValue)),
                DateTimeLiteral dateTimeLiteral => TranslateDate(dateTimeLiteral),
                BooleanLiteral boolean => LiteralExpression(boolean.BooleanValue ? SyntaxKind.TrueLiteralExpression : SyntaxKind.FalseLiteralExpression),
                MemberAccessLiteral memberAccess => TranslateMemberAccessLiteral(memberAccess),
                _ => throw new InvalidOperationException("Couldn't map type: " + token.GetType())
            };
        }

        private static ExpressionSyntax TranslateMemberAccessLiteral(MemberAccessLiteral memberAccess)
        {
            var parts = memberAccess.GetParts();
            if (parts.Length != 2) throw new InvalidOperationException("Only 2 parts expected.");

            return MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                IdentifierName(parts[0]),
                IdentifierName(parts[1]));
        }

        private static ExpressionSyntax TranslateDate(DateTimeLiteral dateTimeLiteral)
        {
            return TranslateDate(dateTimeLiteral.DateTimeValue);
        }

        private static ExpressionSyntax TranslateDate(DateTime dateTimeValue)
        {
            return ObjectCreationExpression(
                    QualifiedName(
                        IdentifierName("System"),
                        IdentifierName("DateTime")))
                .WithArgumentList(
                    ArgumentList(
                        SeparatedList<ArgumentSyntax>(
                            new SyntaxNodeOrToken[]
                            {
                                Arguments.Numeric(dateTimeValue.Year),
                                Token(SyntaxKind.CommaToken),
                                Arguments.Numeric(dateTimeValue.Month),
                                Token(SyntaxKind.CommaToken),
                                Arguments.Numeric(dateTimeValue.Day),
                                Token(SyntaxKind.CommaToken),
                                Arguments.Numeric(dateTimeValue.Hour),
                                Token(SyntaxKind.CommaToken),
                                Arguments.Numeric(dateTimeValue.Minute),
                                Token(SyntaxKind.CommaToken),
                                Arguments.Numeric(dateTimeValue.Second)
                            })));
        }

        public static TypeSyntax MapType(VariableDefinition variableDefinition) => MapType(variableDefinition.Type);

        public static TypeSyntax MapType(string type)
        {
            return type switch
            {
                TypeNames.String => PredefinedType(Token(SyntaxKind.StringKeyword)),
                TypeNames.Number => PredefinedType(Token(SyntaxKind.DecimalKeyword)),
                TypeNames.DateTime => ParseName("System.DateTime"),
                TypeNames.Boolean => PredefinedType(Token(SyntaxKind.BoolKeyword)),
                _ => throw new InvalidOperationException("Couldn't map type: " + type)
            };
        }

        public static TypeSyntax MapType(VariableDeclarationType type)
        {
            return type switch
            {
                PrimitiveVariableDeclarationType primitive => MapType(primitive.Type),
                CustomVariableDeclarationType enumType => IdentifierName(enumType.Type),
                _ => throw new InvalidOperationException("Couldn't map type: " + type)
            };
        }
    }
}