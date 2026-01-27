using System;
using System.Collections.Generic;
using System.Linq;

namespace Lexy.Compiler.Language.TypeSystem.Objects;

public abstract class ObjectType : Type
{
    private readonly Lazy<IEnumerable<IObjectMember>> members;

    public string Name { get; }

    public IEnumerable<IObjectMember> Members => members.Value;

    protected ObjectType(string name)
    {
        Name = name;
        members = new Lazy<IEnumerable<IObjectMember>>(CreateMembers);
    }

    protected ObjectType(string name, IEnumerable<IObjectMember> members)
    {
        Name = name;
        this.members = new Lazy<IEnumerable<IObjectMember>>(members);
    }

    public Type MemberType(string name) => Members.FirstOrDefault(member => member.Name == name)?.Type;

    public IEnumerable<ObjectVariable> GetVariables() => Members.OfType<ObjectVariable>();

    public IEnumerable<ObjectFunction> GetFunctions() => Members.OfType<ObjectFunction>();

    public ObjectVariable GetVariable(string name) => GetMember(name) as ObjectVariable;

    public ObjectFunction GetFunction(string name) => GetMember(name) as ObjectFunction;

    public IObjectMember GetMember(string name) => Members.FirstOrDefault(member => member.Name == name);

    public bool ContainsMember(string name) => Members.Any(member => member.Name == name);

    public override bool IsAssignableFrom(Type type)
    {
        return type is ObjectType otherObjectType
            && otherObjectType.GetType() == GetType()
            && otherObjectType.Name == Name;
    }

    public override string ToString() => Name;

    public override int GetHashCode() => Name != null ? (GetType().Name + ":" + Name).GetHashCode() : 0;

    public virtual IEnumerable<IComponentNode> GetDependencies(IComponentNodeList componentNodes)
    {
        yield break;
    }

    protected virtual IEnumerable<IObjectMember> CreateMembers()
    {
        throw new InvalidOperationException("Derived classes should provide members by constructor or by overriding this method.");
    }
}
