using System;
using Lexy.Compiler.Parser;

namespace Lexy.Compiler.Language.Types
{
    public class FunctionResultsType : ComplexTypeType
    {
        public string FunctionName { get; }

        public FunctionResultsType(string functionName) : base(functionName)
        {
            FunctionName = functionName ?? throw new ArgumentNullException(nameof(functionName));
        }

        public override ComplexType GetComplexType(IValidationContext context) =>
            context.Nodes.GetFunction(FunctionName)?.GetResultsType(context);
    }
}