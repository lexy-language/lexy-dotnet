using System;
using Lexy.Compiler.Parser;

namespace Lexy.Compiler.Language.Types
{
    public class FunctionParametersType : ComplexTypeType
    {
        public string FunctionName { get; }

        public FunctionParametersType(string functionName) : base(functionName)
        {
            FunctionName = functionName ?? throw new ArgumentNullException(nameof(functionName));
        }

        public override ComplexType GetComplexType(IValidationContext context) =>
            context.Nodes.GetFunction(FunctionName)?.GetParametersType(context);
    }
}