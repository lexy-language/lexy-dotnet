using Lexy.Compiler.Compiler.CSharp.Syntax;
using Lexy.Compiler.Language.Expressions.Functions;
using Lexy.RunTime;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Lexy.Compiler.Compiler.CSharp.BuiltInFunctions;

internal class LookUpByFunctionCall : FunctionCall<LookupByFunction>
{
    public override MemberDeclarationSyntax CustomMethodSyntax(LookupByFunction lookupFunction)
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
                            Parameter(Identifier("discriminator"))
                                .WithType(Types.Syntax(lookupFunction.DiscriminatorValueColumnType)),
                            Token(SyntaxKind.CommaToken),
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
                                        IdentifierName(nameof(BuiltInTableFunctions.LookUpBy))))
                                .WithArgumentList(
                                    ArgumentList(
                                        SeparatedList<ArgumentSyntax>(
                                            new SyntaxNodeOrToken[]
                                            {
                                                Arguments.String(lookupFunction.ResultColumn.Member),
                                                Token(SyntaxKind.CommaToken),
                                                Arguments.String(lookupFunction.DiscriminatorValueColumn.Member),
                                                Token(SyntaxKind.CommaToken),
                                                Arguments.String(lookupFunction.SearchValueColumn.Member),
                                                Token(SyntaxKind.CommaToken),
                                                Arguments.String(lookupFunction.TableName),
                                                Token(SyntaxKind.CommaToken),
                                                Arguments.MemberAccess(ClassNames.TableClassName(lookupFunction.TableName),
                                                    "Values"),
                                                Token(SyntaxKind.CommaToken),
                                                Argument(IdentifierName("discriminator")),
                                                Token(SyntaxKind.CommaToken),
                                                Argument(IdentifierName("condition")),
                                                Token(SyntaxKind.CommaToken),
                                                Arguments.MemberAccessLambda("row",
                                                    lookupFunction.DiscriminatorValueColumn.Member),
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

    public override ExpressionSyntax CallExpressionSyntax(LookupByFunction lookupFunction)
    {
        var methodName = MethodName(lookupFunction);

        return InvocationExpression(IdentifierName(methodName))
            .WithArgumentList(
                ArgumentList(
                    SeparatedList<ArgumentSyntax>(
                        new SyntaxNodeOrToken[]
                        {
                            Argument(Expressions.ExpressionSyntax(lookupFunction.DiscriminatorExpression)),
                            Token(SyntaxKind.CommaToken),
                            Argument(Expressions.ExpressionSyntax(lookupFunction.ValueExpression)),
                            Token(SyntaxKind.CommaToken),
                            Argument(IdentifierName(LexyCodeConstants.ContextVariable))
                        })));
    }

    private static string MethodName(LookupByFunction lookupFunction)
    {
        return $"__LookUp{lookupFunction.Table}{lookupFunction.ResultColumn.Member}By{lookupFunction.SearchValueColumn.Member}" +
               $"Discriminator{lookupFunction.DiscriminatorValueColumn.Member}";
    }
}