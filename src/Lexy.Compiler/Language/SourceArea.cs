using Lexy.Compiler.Language.Symbols;

namespace Lexy.Compiler.Language;

public class SourceArea
{
    private readonly Position begin;
    private Position end;

    public SourceArea(SourceReference reference)
    {
        begin = new Position(reference.LineNumber, reference.Column);
        end = new Position(reference.LineNumber, reference.EndColumn);
    }

    public void Expand(Position position)
    {
        end = position;
    }

    public bool Includes(Position position)
    {
        if (position.LineNumber < begin.LineNumber) return false;
        if (position.LineNumber > end.LineNumber) return false;

        if (position.LineNumber == begin.LineNumber)
        {
            if (position.LineNumber == end.LineNumber)
            {
                return position.Column >= begin.Column && position.Column <= end.Column;
            }
            return position.Column >= begin.Column;
        }

        if (position.LineNumber == end.LineNumber)
        {
            return position.Column <= end.Column + 1;
        }

        return true;
    }

    public override string ToString()
    {
        return $"Begin ({begin}) End ({end})";
    }
}
