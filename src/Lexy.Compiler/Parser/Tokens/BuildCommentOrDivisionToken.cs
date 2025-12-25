using System;

namespace Lexy.Compiler.Parser.Tokens;

public class BuildCommentOrDivisionToken : ParsableToken
{
    public BuildCommentOrDivisionToken(TokenCharacter character) : base(character)
    {
    }

    public override ParseTokenResult Parse(TokenCharacter character)
    {
        if (ValueLength != 1) throw new InvalidOperationException("Length should not exceed 1");

        return character.Value != TokenValues.DivisionOrComment
            ? ParseTokenResult.Finished(false, new OperatorToken(FirstCharacter))
            : ParseTokenResult.InProgress(new CommentToken(FirstCharacter, Value));
    }

    public override ParseTokenResult Finalize()
    {
        return ParseTokenResult.Invalid(
            "Unexpected end of line. Can't end with a single '/'.");
    }
}