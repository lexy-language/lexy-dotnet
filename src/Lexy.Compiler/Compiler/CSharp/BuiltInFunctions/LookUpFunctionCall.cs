using Lexy.Compiler.Language.Expressions.Functions;
using Lexy.RunTime;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Lexy.Compiler.Compiler.CSharp.BuiltInFunctions;

internal class LookUpFunctionCall : FunctionCall<LookupFunction>
{
    public override MemberDeclarationSyntax CustomMethodSyntax(LookupFunction lookupFunction)
    {
        var methodName = MethodName(lookupFunction);

        return MethodDeclaration(
                Types.Syntax(lookupFunction.ResultColumnType),
                Identifier(methodName))
            .WithModifiers(Modifiers.PrivateStatic())
            .WithParameterList(
                ParameterList(
                    SeparatedList<ParameterSyntax>(
                        new SyntaxNodeOrToken[]
                        {
                            Parameter(Identifier("condition"))
                                .WithType(Types.Syntax(lookupFunction.SearchValueColumnType)),
                            Token(SyntaxKind.CommaToken),
                            Parameter(Identifier(LexyCodeConstants.ContextVariable))
                                .WithType(IdentifierName("IExecutionContext"))
                        })))
            .WithBody(
                Block(
                    SingletonList<StatementSyntax>(
                        ReturnStatement(
                            InvocationExpression(
                                    MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        IdentifierName(nameof(BuiltInTableFunctions)),
                                        IdentifierName(nameof(BuiltInTableFunctions.LookUp))))
                                .WithArgumentList(
                                    ArgumentList(
                                        SeparatedList<ArgumentSyntax>(
                                            new SyntaxNodeOrToken[]
                                            {
                                                Arguments.String(lookupFunction.ResultColumn.Member),
                                                Token(SyntaxKind.CommaToken),
                                                Arguments.String(lookupFunction.SearchValueColumn.Member),
                                                Token(SyntaxKind.CommaToken),
                                                Arguments.String(lookupFunction.Table),
                                                Token(SyntaxKind.CommaToken),
                                                Arguments.MemberAccess(ClassNames.TableClassName(lookupFunction.Table),
                                                    "Values"),
                                                Token(SyntaxKind.CommaToken),
                                                Argument(IdentifierName("condition")),
                                                Token(SyntaxKind.CommaToken),
                                                Arguments.MemberAccessLambda("row",
                                                    lookupFunction.SearchValueColumn.Member),
                                                Token(SyntaxKind.CommaToken),
                                                Arguments.MemberAccessLambda("row",
                                                    lookupFunction.ResultColumn.Member),
                                                Token(SyntaxKind.CommaToken),
                                                Argument(IdentifierName(LexyCodeConstants.ContextVariable))
                                            })))))));
    }

    public override ExpressionSyntax CallExpressionSyntax(LookupFunction lookupFunction)
    {
        var methodName = MethodName(lookupFunction);

        return InvocationExpression(IdentifierName(methodName))
            .WithArgumentList(
                ArgumentList(
                    SeparatedList<ArgumentSyntax>(
                        new SyntaxNodeOrToken[]
                        {
                            Argument(ExpressionSyntaxFactory.ExpressionSyntax(lookupFunction.ValueExpression)),
                            Token(SyntaxKind.CommaToken),
                            Argument(IdentifierName(LexyCodeConstants.ContextVariable))
                        })));
    }

    private static string MethodName(LookupFunction lookupFunction)
    {
        return $"__LookUp{lookupFunction.Table}{lookupFunction.ResultColumn.Member}By{lookupFunction.SearchValueColumn.Member}";
    }
}