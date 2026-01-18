using System;
using System.Collections.Generic;
using Lexy.Compiler.Language;
using Lexy.Compiler.Language.TypeSystem.Objects;
using Lexy.RunTime;
using Type = Lexy.Compiler.Language.TypeSystem.Type;

namespace Lexy.Compiler.Parser;

public class VariableContext : IVariableContext
{
    private readonly IParserLogger logger;
    private readonly ComponentNodeList componentNodes;
    private readonly IVariableContext parentContext;
    private readonly IDictionary<string, VariableEntry> variables = new Dictionary<string, VariableEntry>();

    public VariableContext(ComponentNodeList componentNodes, IParserLogger logger, IVariableContext parentContext)
    {
        this.componentNodes = Assert.NotNull(componentNodes, nameof(componentNodes));
        this.logger = Assert.NotNull(logger, nameof(logger));
        this.parentContext = parentContext;
    }

    public void AddVariable(string name, Type type, VariableSource source)
    {
        if (Contains(name)) return;

        var entry = new VariableEntry(type, source);
        variables.Add(name, entry);
    }

    public void RegisterVariableAndVerifyUnique(SourceReference reference, string name, Type type,
        VariableSource source)
    {
        if (Contains(name))
        {
            logger.Fail(reference, $"Duplicated variable name: '{name}'");
            return;
        }

        var entry = new VariableEntry(type, source);
        variables.Add(name, entry);
    }

    public bool Contains(string name)
    {
        return variables.ContainsKey(name) || parentContext != null && parentContext.Contains(name);
    }

    public bool Contains(IdentifierPath path)
    {
        var parent = GetVariable(path.RootIdentifier);
        if (parent == null) return false;

        return !path.HasChildIdentifiers ||
               ContainsChild(parent.Type, path.ChildrenReference());
    }

    public VariableReference CreateVariableReference(SourceReference reference, IdentifierPath path)
    {
        VariableReference ExecuteWithPriority(Func<IdentifierPath, VariableReference> firstPriorityHandler,
            Func<IdentifierPath, VariableReference> secondPriorityHandler)
        {
            var value1 = firstPriorityHandler(path);
            if (value1 != null) return value1;

            var value2 = secondPriorityHandler(path);
            if (value2 != null) return value2;

            return null;
        };

        var containsMemberAccess = path.Parts > 1;
        var fromTypeSystem = CreateVariableReferenceFromTypeSystem;
        var fromVariables = CreateVariableReferenceFromRegisteredVariables;
        return containsMemberAccess
            ? ExecuteWithPriority(fromTypeSystem, fromVariables)
            : ExecuteWithPriority(fromVariables, fromTypeSystem);
    }

    private VariableReference CreateVariableReferenceFromRegisteredVariables(IdentifierPath path)
    {
        var variable = GetVariable(path.RootIdentifier);
        if (variable == null) return null;

        var variableType = GetVariableType(path);
        if (variableType == null) return null;

        return new VariableReference(path, null, variableType, variable.VariableSource);
    }

    private VariableReference CreateVariableReferenceFromTypeSystem(IdentifierPath path)
    {
        if (path.Parts > 2) return null;

        var rootVariableType = componentNodes.GetType(path.RootIdentifier);
        if (rootVariableType == null) return null;

        if (path.Parts == 1)
        {
            return new VariableReference(path, rootVariableType, rootVariableType, VariableSource.Type);
        }

        var member = path.LastPart();
        var memberType = rootVariableType.MemberType(member);
        if (memberType == null) return null;
        return new VariableReference(path, rootVariableType, memberType, VariableSource.Type);
    }

    public Type GetVariableType(string name)
    {
        return variables.TryGetValue(name, out var value)
            ? value.Type
            : parentContext?.GetVariableType(name);
    }

    public Type GetVariableType(IdentifierPath path)
    {
        Assert.NotNull(path, nameof(path));

        var parent = GetVariableType(path.RootIdentifier);
        return parent == null || !path.HasChildIdentifiers
            ? parent
            : GetVariableType(parent, path.ChildrenReference());
    }

    public VariableEntry GetVariable(string name)
    {
        return variables.TryGetValue(name, out var value)
            ? value
            : parentContext?.GetVariable(name);
    }

    private static bool ContainsChild(Type parentType, IdentifierPath path)
    {
        var objectType = parentType as IObjectType;

        var memberVariableType = objectType?.MemberType(path.RootIdentifier);
        if (memberVariableType == null) return false;

        return !path.HasChildIdentifiers
               || ContainsChild(memberVariableType, path.ChildrenReference());
    }

    private Type GetVariableType(Type parentType, IdentifierPath path)
    {
        if (parentType is not IObjectType objectType) return null;

        var memberVariableType = objectType.MemberType(path.RootIdentifier);
        if (memberVariableType == null) return null;

        return !path.HasChildIdentifiers
            ? memberVariableType
            : GetVariableType(memberVariableType, path.ChildrenReference());
    }
}
