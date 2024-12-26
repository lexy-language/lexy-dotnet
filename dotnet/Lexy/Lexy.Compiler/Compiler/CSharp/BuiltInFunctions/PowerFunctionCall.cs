using Lexy.Poc.Core.Language.Expressions.Functions;
using Lexy.Poc.Core.RunTime;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Lexy.Poc.Core.Compiler.CSharp.BuiltInFunctions
{
    internal class PowerFunctionCall : MethodFunctionCall
    {
        public PowerFunction PowerFunction { get; }

        protected override string ClassName => nameof(BuiltInNumberFunctions);
        protected override string MethodName => nameof(BuiltInNumberFunctions.Power);

        public PowerFunctionCall(PowerFunction function) : base(function)
        {
            PowerFunction = function;
        }

        protected override SeparatedSyntaxList<ArgumentSyntax> GetArguments(ICompileFunctionContext context)
        {
            return SyntaxFactory.SeparatedList<ArgumentSyntax>(
                new SyntaxNodeOrToken[]
                {
                    SyntaxFactory.Argument(ExpressionSyntaxFactory.ExpressionSyntax(PowerFunction.NumberExpression, context)),
                    SyntaxFactory.Token(SyntaxKind.CommaToken),
                    SyntaxFactory.Argument(ExpressionSyntaxFactory.ExpressionSyntax(PowerFunction.PowerExpression, context))
                });
        }
    }

    internal class RoundFunctionCall : MethodFunctionCall
    {
        public RoundFunction RoundFunction { get; }

        protected override string ClassName => nameof(BuiltInNumberFunctions);
        protected override string MethodName => nameof(BuiltInNumberFunctions.Round);

        public RoundFunctionCall(RoundFunction function) : base(function)
        {
            RoundFunction = function;
        }

        protected override SeparatedSyntaxList<ArgumentSyntax> GetArguments(ICompileFunctionContext context)
        {
            return SyntaxFactory.SeparatedList<ArgumentSyntax>(
                new SyntaxNodeOrToken[]
                {
                    SyntaxFactory.Argument(ExpressionSyntaxFactory.ExpressionSyntax(RoundFunction.NumberExpression, context)),
                    SyntaxFactory.Token(SyntaxKind.CommaToken),
                    SyntaxFactory.Argument(ExpressionSyntaxFactory.ExpressionSyntax(RoundFunction.DigitsExpression, context))
                });
        }
    }
}