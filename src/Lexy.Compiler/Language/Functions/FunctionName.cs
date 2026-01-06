using System;
using System.Collections.Generic;
using Lexy.Compiler.Infrastructure;
using Lexy.Compiler.Parser;
using Lexy.RunTime;
using Microsoft.CodeAnalysis.CSharp;

namespace Lexy.Compiler.Language.Functions;

public class FunctionName : Node
{
    public string Value { get; private set; }

    public FunctionName(SourceReference reference) : base(reference)
    {
    }

    public void ParseName(string name)
    {
        Value = Assert.NotNull(name, nameof(name));
    }

    public override IEnumerable<INode> GetChildren()
    {
        yield break;
    }

    protected override void Validate(IValidationContext context)
    {
        if (string.IsNullOrEmpty(Value))
        {
            context.Logger.Fail(Reference, $"Invalid function name: '{Value}'. Name should not be empty.");
        }
        else if (!SyntaxFacts.IsValidIdentifier(Value))
        {
            context.Logger.Fail(Reference, $"Invalid function name: '{Value}'.");
        }
    }

    public override string ToString() => Value;
}