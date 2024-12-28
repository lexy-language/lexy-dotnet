using Lexy.Compiler.Language;

namespace Lexy.Compiler.Parser
{
    public interface IParserContext
    {
        IParserLogger Logger { get; }

        Line CurrentLine { get; }

        RootNodeList Nodes { get; }
        SourceCodeNode RootNode { get; }

        ISourceCodeDocument SourceCode { get; }

        void ProcessNode(IRootNode node);

        bool ProcessLine();

        TokenValidator ValidateTokens<T>();
        TokenValidator ValidateTokens(string name);

        SourceReference TokenReference(int tokenIndex);
        SourceReference LineEndReference();
        SourceReference LineStartReference();
        SourceReference LineReference(int characterPosition);

        void AddFileIncluded(string fileName);
        bool IsFileIncluded(string fileName);
    }
}