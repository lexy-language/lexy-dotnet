using System;
using System.Collections.Generic;
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

        public ImplicitVariableDeclaration(SourceReference reference) : base(reference)
        {
        }

        public override IEnumerable<INode> GetChildren()
        {
            yield break;
        }

        protected override void Validate(IValidationContext context)
        {
        }
    }
}