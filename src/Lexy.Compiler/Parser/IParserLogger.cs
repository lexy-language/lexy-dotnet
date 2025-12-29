using System.Collections.Generic;
using Lexy.Compiler.Language;

namespace Lexy.Compiler.Parser;

public interface IParserLogger
{
    void LogInfo(string message);

    void Log(SourceReference reference, string message);
    void Fail(SourceReference reference, string message);

    bool HasErrors();
    bool HasComponentErrors();

    bool HasErrorMessage(string expectedError);

    public string FormatMessages();

    bool NodeHasErrors(IComponentNode node);

    string[] ErrorMessages();
    string[] ErrorComponentMessages();
    string[] ErrorNodeMessages(IComponentNode node);
    string[] ErrorNodesMessages(IEnumerable<IComponentNode> nodes);

    void AssertNoErrors();

    void SetCurrentNode(IComponentNode node);
    void ResetCurrentNode();
}