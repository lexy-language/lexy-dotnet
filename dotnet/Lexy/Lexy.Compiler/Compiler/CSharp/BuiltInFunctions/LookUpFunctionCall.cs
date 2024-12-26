using Lexy.Poc.Core.Language.Expressions.Functions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Lexy.Poc.Core.Compiler.CSharp.BuiltInFunctions
{
    internal class LookUpFunctionCall : BuiltInFunctionCall
    {
        private readonly string methodName;

        public LookupFunction LookupFunction { get; }

        public LookUpFunctionCall(LookupFunction lookupFunction) : base(lookupFunction)
        {
            LookupFunction = lookupFunction;
            methodName =
                $"__LookUp{lookupFunction.TableType}{lookupFunction.ResultColumnHeaderName}By{lookupFunction.SearchValueColumnHeaderName}";
        }

        public override MemberDeclarationSyntax CustomMethodSyntax(ICompileFunctionContext context)
        {
            return MethodDeclaration(
                    Types.Syntax(LookupFunction.ResultColumnType),
                    Identifier(methodName))
                .WithModifiers(Modifiers.Private())
                .WithParameterList(
                    ParameterList(
                        SeparatedList<ParameterSyntax>(
                            new SyntaxNodeOrToken[]
                            {
                                Parameter(Identifier("condition"))
                                    .WithType(Types.Syntax(LookupFunction.SearchValueColumnType)),
                                Token(SyntaxKind.CommaToken),
                                Parameter(Identifier("context"))
                                    .WithType(IdentifierName("IExecutionContext"))
                            })))
                .WithBody(
                    Block(
                        SingletonList<StatementSyntax>(
                            ReturnStatement(
                                InvocationExpression(
                                        MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            IdentifierName("BuiltInTableFunctions"),
                                            IdentifierName("LookUp")))
                                    .WithArgumentList(
                                        ArgumentList(
                                            SeparatedList<ArgumentSyntax>(
                                                new SyntaxNodeOrToken[]
                                                {
                                                    Arguments.String(LookupFunction.ResultColumnHeaderName),
                                                    Token(SyntaxKind.CommaToken),
                                                    Arguments.String(LookupFunction.SearchValueColumnHeaderName),
                                                    Token(SyntaxKind.CommaToken),
                                                    Arguments.String(LookupFunction.TableType),
                                                    Token(SyntaxKind.CommaToken),
                                                    Arguments.MemberAccess(LookupFunction.TableType, "Values"),
                                                    Token(SyntaxKind.CommaToken),
                                                    Argument(IdentifierName("condition")),
                                                    Token(SyntaxKind.CommaToken),
                                                    Arguments.MemberAccessLambda("row",
                                                        LookupFunction.SearchValueColumnHeaderName),
                                                    Token(SyntaxKind.CommaToken),
                                                    Arguments.MemberAccessLambda("row",
                                                        LookupFunction.ResultColumnHeaderName),
                                                    Token(SyntaxKind.CommaToken),
                                                    Argument(IdentifierName("context"))
                                                })))))));
        }

        public override ExpressionSyntax CallExpressionSyntax(ICompileFunctionContext context)
        {
            return InvocationExpression(IdentifierName(methodName))
                .WithArgumentList(
                    ArgumentList(
                        SeparatedList<ArgumentSyntax>(
                            new SyntaxNodeOrToken[]
                            {
                                Argument(ExpressionSyntaxFactory.ExpressionSyntax(LookupFunction.ValueExpression,
                                    context)),
                                Token(SyntaxKind.CommaToken),
                                Argument(IdentifierName("context"))
                            })));
        }
    }
}