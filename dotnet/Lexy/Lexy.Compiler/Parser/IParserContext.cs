using Lexy.Poc.Core.Language;
using Lexy.Poc.Core.Language.Expressions;
using Lexy.Poc.Core.Parser.Tokens;

namespace Lexy.Poc.Core.Parser
{
    public interface IParserContext
    {
        IParserLogger Logger { get; }

        Line CurrentLine { get; }

        Nodes Nodes { get; }
        ISourceCodeDocument SourceCode { get; }

        void ProcessNode(IRootNode node);

        bool ProcessLine();

        TokenValidator ValidateTokens<T>();
        TokenValidator ValidateTokens(string name);

        SourceReference TokenReference(int tokenIndex);
        SourceReference LineEndReference();
        SourceReference LineStartReference();
        SourceReference DocumentReference();
        SourceReference LineReference(int characterPosition);
        SourceReference TokenReference(Token token);
    }
}