using System.Collections.Generic;
namespace Lexy.Compiler.Language.VariableTypes;

public abstract class ComplexType : VariableType, IComplexType
{
    public abstract VariableType MemberType(string name, IComponentNodeList componentNodes);

    public IEnumerable<IComplexTypeVariable> GetVariables()
    {
        return new IComplexTypeVariable[] { };
    }

    public IEnumerable<IComplexTypeFunction> GetFunctions()
    {
        return new IComplexTypeFunction[] { };
    }

    public abstract IComplexTypeVariable GetVariable(string name);
    public abstract IComplexTypeFunction GetFunction(string name);

    public override bool IsAssignableFrom(VariableType type)
    {
        if (type is not IComplexType otherComplexType) return false;

        return VariablesAssignableFrom(otherComplexType);
    }

    private bool VariablesAssignableFrom(IComplexType otherComplexType)
    {
        var neededVariables = otherComplexType.GetVariables();
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