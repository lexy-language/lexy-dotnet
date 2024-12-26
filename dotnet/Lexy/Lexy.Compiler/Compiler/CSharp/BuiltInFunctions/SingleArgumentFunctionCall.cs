using Lexy.Poc.Core.Language.Expressions.Functions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Lexy.Poc.Core.Compiler.CSharp.BuiltInFunctions
{
    internal abstract class SingleArgumentFunctionCall : MethodFunctionCall
    {
        public SingleArgumentFunction SingleArgumentFunction { get; }

        protected SingleArgumentFunctionCall(SingleArgumentFunction function) : base(function)
        {
            SingleArgumentFunction = function;
        }

        public override MemberDeclarationSyntax CustomMethodSyntax(ICompileFunctionContext context) => null;

        protected override SeparatedSyntaxList<ArgumentSyntax> GetArguments(ICompileFunctionContext context)
        {
            return SyntaxFactory.SingletonSeparatedList(
                SyntaxFactory.Argument(
                    ExpressionSyntaxFactory.ExpressionSyntax(SingleArgumentFunction.ValueExpression, context)));
        }
    }

    internal abstract class MethodFunctionCall : BuiltInFunctionCall
    {
        public BuiltInFunction Function { get; }

        protected abstract string ClassName { get; }
        protected abstract string MethodName { get; }

        protected MethodFunctionCall(BuiltInFunction function) : base(function)
        {
            Function = function;
        }

        public override MemberDeclarationSyntax CustomMethodSyntax(ICompileFunctionContext context) => null;

        public override ExpressionSyntax CallExpressionSyntax(ICompileFunctionContext context)
        {
            var arguments = GetArguments(context);

            return SyntaxFactory.InvocationExpression(
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.IdentifierName(ClassName),
                        SyntaxFactory.IdentifierName(MethodName)))
                .WithArgumentList(SyntaxFactory.ArgumentList(arguments));
        }

        protected abstract SeparatedSyntaxList<ArgumentSyntax> GetArguments(ICompileFunctionContext context);
    }
}