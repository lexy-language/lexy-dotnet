using System.Collections.Generic;
using Lexy.Compiler.Language.Enums;
using Lexy.Compiler.Language.Functions;
using Lexy.Compiler.Language.Scenarios;
using Lexy.Compiler.Language.Tables;
using Lexy.Compiler.Language.Types;
using Lexy.Compiler.Language.VariableTypes;

namespace Lexy.Compiler.Language;

public interface IComponentNodeList : IEnumerable<IComponentNode>
{
    IComponentNode GetNode(string name);
    bool Contains(string name);
    Function GetFunction(string name);
    Table GetTable(string name);
    TypeDefinition GetCustomType(string name);
    IEnumerable<Scenario> GetScenarios();
    EnumDefinition GetEnum(string name);
    TypeWithMembers GetType(string name);
}