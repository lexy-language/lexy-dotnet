using Lexy.Poc.Core.Language.Expressions.Functions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Lexy.Poc.Core.Compiler.CSharp.BuiltInFunctions
{
    internal abstract class EndStartDateFunctionCall : MethodFunctionCall
    {
        public EndStartDateFunction Function { get; }

        protected EndStartDateFunctionCall(EndStartDateFunction function) : base(function)
        {
            Function = function;
        }

        public override MemberDeclarationSyntax CustomMethodSyntax(ICompileFunctionContext context) => null;

        protected override SeparatedSyntaxList<ArgumentSyntax> GetArguments(ICompileFunctionContext context)
        {
            return SyntaxFactory.SeparatedList<ArgumentSyntax>(
                new SyntaxNodeOrToken[]
                {
                    SyntaxFactory.Argument(ExpressionSyntaxFactory.ExpressionSyntax(Function.EndDateExpression, context)),
                    SyntaxFactory.Token(SyntaxKind.CommaToken),
                    SyntaxFactory.Argument(ExpressionSyntaxFactory.ExpressionSyntax(Function.StartDateExpression, context))
                });
        }
    }
}