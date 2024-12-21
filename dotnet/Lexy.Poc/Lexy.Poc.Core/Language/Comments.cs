using System.Collections.Generic;
using Lexy.Poc.Core.Parser;
using Lexy.Poc.Core.Parser.Tokens;

namespace Lexy.Poc.Core.Language
{
    public class Comments : IComponent
    {
        public IList<string> Lines { get; } = new List<string>();

        public IComponent Parse(IParserContext context)
        {
            var valid = context.ValidateTokens<Comments>()
                .Count(1)
                .Comment(0)
                .IsValid;

            if (!valid) return null;

            var comment = context.CurrentLine.Tokens.Token<CommentToken>(0);
            Lines.Add(comment.Value);
            return this;
        }
    }
}