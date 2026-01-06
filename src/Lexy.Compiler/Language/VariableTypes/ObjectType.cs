using System.Collections.Generic;
namespace Lexy.Compiler.Language.VariableTypes;

public abstract class ObjectType : VariableType, IObjectType
{
    public abstract VariableType MemberType(string name, IComponentNodeList componentNodes);

    public virtual IEnumerable<IObjectTypeVariable> GetVariables() => new IObjectTypeVariable[] { };

    public virtual IEnumerable<IObjectTypeFunction> GetFunctions() => new IObjectTypeFunction[] { };

    public abstract IObjectTypeVariable GetVariable(string name);
    public abstract IObjectTypeFunction GetFunction(string name);

    public override bool IsAssignableFrom(VariableType type)
    {
        if (type is not IObjectType otherObjectType) return false;

        return VariablesAssignableFrom(otherObjectType);
    }

    private bool VariablesAssignableFrom(IObjectType otherObjectType)
    {
        var neededVariables = otherObjectType.GetVariables();
        foreach (var neededVariable in neededVariables)
        {
            var ownVariable = GetVariable(neededVariable.Name);
            if (ownVariable == null || !neededVariable.Type.IsAssignableFrom(ownVariable.Type))
            {
                return false;
            }
        }
        return true;
    }
}