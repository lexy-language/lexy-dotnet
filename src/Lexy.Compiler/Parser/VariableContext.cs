using System;
using System.Collections.Generic;
using Lexy.Compiler.Language;
using Lexy.Compiler.Language.VariableTypes;

namespace Lexy.Compiler.Parser;

public class VariableContext : IVariableContext
{
    private readonly IParserLogger logger;
    private readonly ComponentNodeList componentNodes;
    private readonly IVariableContext parentContext;
    private readonly IDictionary<string, VariableEntry> variables = new Dictionary<string, VariableEntry>();

    public VariableContext(ComponentNodeList componentNodes, IParserLogger logger, IVariableContext parentContext)
    {
        this.componentNodes = componentNodes ?? throw new ArgumentNullException(nameof(componentNodes));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.parentContext = parentContext;
    }

    public void AddVariable(string name, VariableType type, VariableSource source)
    {
        if (Contains(name)) return;

        var entry = new VariableEntry(type, source);
        variables.Add(name, entry);
    }

    public void RegisterVariableAndVerifyUnique(SourceReference reference, string name, VariableType type,
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

    public bool Contains(VariablePath path, IValidationContext context)
    {
        var parent = GetVariable(path.ParentIdentifier);
        if (parent == null) return false;

        return !path.HasChildIdentifiers ||
               ContainChild(parent.VariableType, path.ChildrenReference(), context);
    }

    public VariableReference CreateVariableReference(SourceReference reference, VariablePath path, IValidationContext validationContext)
    {
        VariableReference ExecuteWithPriority(Func<VariablePath,IValidationContext, VariableReference> firstPriorityHandler,
            Func<VariablePath,IValidationContext, VariableReference> secondPriorityHandler)
        {
            var value1 = firstPriorityHandler(path, validationContext);
            if (value1 != null) return value1;

            var value2 = secondPriorityHandler(path, validationContext);
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

    private VariableReference CreateVariableReferenceFromRegisteredVariables(VariablePath path, IValidationContext validationContext)
    {
        var variable = GetVariable(path.ParentIdentifier);
        if (variable == null) return null;

        var variableType = GetVariableType(path, validationContext);
        if (variableType == null) return null;

        return new VariableReference(path, null, variableType, variable.VariableSource);
    }

    private VariableReference CreateVariableReferenceFromTypeSystem(VariablePath path, IValidationContext validationContext)
    {
        if (path.Parts > 2) return null;

        var rootVariableType = componentNodes.GetType(path.ParentIdentifier);
        if (rootVariableType == null) return null;

        if (path.Parts == 1)
        {
            return new VariableReference(path, rootVariableType, rootVariableType, VariableSource.Type);
        }

        var member = path.LastPart();
        var memberType = rootVariableType.MemberType(member, validationContext.ComponentNodes);
        if (memberType == null) return null;
        return new VariableReference(path, rootVariableType, memberType, VariableSource.Type);
    }

    public VariableType GetVariableType(string name)
    {
        return variables.TryGetValue(name, out var value)
            ? value.VariableType
            : parentContext?.GetVariableType(name);
    }

    public VariableType GetVariableType(VariablePath path, IValidationContext context)
    {
        if (path == null) throw new ArgumentNullException(nameof(path));

        var parent = GetVariableType(path.ParentIdentifier);
        return parent == null || !path.HasChildIdentifiers
            ? parent
            : GetVariableType(parent, path.ChildrenReference(), context);
    }

    public VariableEntry GetVariable(string name)
    {
        return variables.TryGetValue(name, out var value)
            ? value
            : parentContext?.GetVariable(name);
    }

    private bool ContainChild(VariableType parentType, VariablePath path, IValidationContext context)
    {
        var typeWithMembers = parentType as ITypeWithMembers;

        var memberVariableType = typeWithMembers?.MemberType(path.ParentIdentifier, context.ComponentNodes);
        if (memberVariableType == null) return false;

        return !path.HasChildIdentifiers
               || ContainChild(memberVariableType, path.ChildrenReference(), context);
    }

    private VariableType GetVariableType(VariableType parentType, VariablePath path,
        IValidationContext context)
    {
        if (parentType is not ITypeWithMembers typeWithMembers) return null;

        var memberVariableType = typeWithMembers.MemberType(path.ParentIdentifier, context.ComponentNodes);
        if (memberVariableType == null) return null;

        return !path.HasChildIdentifiers
            ? memberVariableType
            : GetVariableType(memberVariableType, path.ChildrenReference(), context);
    }
}