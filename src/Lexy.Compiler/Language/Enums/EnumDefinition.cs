using System.Collections.Generic;
using System.Linq;
using Lexy.Compiler.Language.Symbols;
using Lexy.Compiler.Language.TypeSystem;
using Lexy.Compiler.Parser;
using Lexy.Compiler.Parser.Context;
using Microsoft.CodeAnalysis.CSharp;

namespace Lexy.Compiler.Language.Enums;

public class EnumDefinition : ComponentNode, INestedNode, INodeWithType
{
    public IList<EnumMember> Members { get; } = new List<EnumMember>();

    public bool Nested { get; }

    internal EnumDefinition(string name, bool nested, NodeReference parentReference, SourceReference reference) :
        base(name, parentReference, reference)
    {
        Nested = nested;
    }

    internal static EnumDefinition Parse(string name, bool nested, INode parent, SourceReference reference)
    {
        return new EnumDefinition(name, nested, new NodeReference(parent), reference);
    }

    public Type CreateType()
    {
        return new EnumType(this);
    }

    public override IParsableNode Parse(IParseLineContext context)
    {
        var lastIndex = Members.LastOrDefault()?.NumberValue ?? -1;
        var member = EnumMember.Parse(context, this, lastIndex);
        if (member != null)
        {
            Members.Add(member);
        }
        return this;
    }

    public override IEnumerable<INode> GetChildren() => Members;

    protected override void Validate(IValidationContext context)
    {
        if (Members.Count == 0)
        {
            context.Logger.Fail(Reference, "Enum has no members defined.");
            return;
        }

        DuplicateChecker.Validate(
            context,
            member => member.Reference,
            member => member.Name,
            member => $"Enum member name should be unique. Duplicate name: '{member.Name}'",
            Members);

        if (string.IsNullOrEmpty(Name))
        {
            context.Logger.Fail(Reference, $"Invalid enum name: {Name}. Name should not be empty.");
        }
        else if (!SyntaxFacts.IsValidIdentifier(Name))
        {
            context.Logger.Fail(Reference, $"Invalid enum name: {Name}.");
        }
    }

    public bool ContainsMember(string name)
    {
        return Members.Any(member => member.Name == name);
    }

    public override Symbol GetSymbol()
    {
        return new Symbol(Reference, $"enum: {Name}", string.Empty, SymbolKind.Enum);
    }
}
