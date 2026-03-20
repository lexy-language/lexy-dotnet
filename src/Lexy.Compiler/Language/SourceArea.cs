
namespace Lexy.Compiler.Language;

public interface IReadonlySourceArea
{
    bool Includes(Position position);
}

public class SourceArea : IReadonlySourceArea
{
    public Position Begin { get; }
    public Position End { get; private set; }

    public SourceArea(SourceReference reference)
    {
        Begin = new Position(reference.LineNumber, reference.Column);
        End = new Position(reference.LineNumber, reference.EndColumn);
    }

    public void Expand(Position position)
    {
        End = position;
    }

    public bool Includes(Position position)
    {
        if (position.LineNumber < Begin.LineNumber) return false;
        if (position.LineNumber > End.LineNumber) return false;

        if (position.LineNumber == Begin.LineNumber)
        {
            if (position.LineNumber == End.LineNumber)
            {
                return position.Column >= Begin.Column && position.Column <= End.Column;
            }
            return position.Column >= Begin.Column;
        }

        if (position.LineNumber == End.LineNumber)
        {
            return position.Column <= End.Column + 1;
        }

        return true;
    }

    public override string ToString() => $"Begin ({Begin}) End ({End})";
}
