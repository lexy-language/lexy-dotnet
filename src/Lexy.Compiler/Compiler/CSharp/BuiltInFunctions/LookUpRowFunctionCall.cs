using Lexy.Compiler.Language.Expressions.Functions;
using Lexy.RunTime;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Lexy.Compiler.Compiler.CSharp.BuiltInFunctions;

internal class LookUpRowFunctionCall : FunctionCall<LookupRowFunction>
{
    public override MemberDeclarationSyntax CustomMethodSyntax(LookupRowFunction lookupFunction)
    {
        var methodName = MethodName(lookupFunction);
        return MethodDeclaration(
                Types.Syntax(lookupFunction.RowType),
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
                                        IdentifierName(nameof(BuiltInTableFunctions.LookUpRow))))
                                .WithArgumentList(
                                    ArgumentList(
                                        SeparatedList<ArgumentSyntax>(
                                            new SyntaxNodeOrToken[]
                                            {
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
                                                Argument(IdentifierName(LexyCodeConstants.ContextVariable))
                                            })))))));
    }

    public override ExpressionSyntax CallExpressionSyntax(LookupRowFunction lookupFunction)
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

    private static string MethodName(LookupRowFunction lookupFunction)
    {
        return $"__LookUp{lookupFunction.Table}RowBy{lookupFunction.SearchValueColumn.Member}";
    }
}