using System;
using Lexy.Compiler.Parser;

namespace Lexy.Compiler.Language.Types
{
    public sealed class ImplicitVariableDeclaration : VariableDeclarationType
    {
        public VariableType VariableType { get; private set; }

        public override VariableType CreateVariableType(IValidationContext context) =>
            VariableType ?? throw new InvalidOperationException("Not supported. Nodes should be Validated first.");

        public void Define(VariableType variableType)
        {
            VariableType = variableType;
        }
    }
}