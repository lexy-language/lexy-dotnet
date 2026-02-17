using System;
using Lexy.Compiler.Infrastructure;
using Lexy.RunTime;

namespace Lexy.Compiler.Language;

public class SourceReference
{
    public int LineNumber { get; }

    public int Column { get; }
    public int EndColumn { get; }

    public IFile File { get; }

    public string SortIndex
    {
        get
        {
            var value = (LineNumber * 100000000 + Column).ToString().PadLeft(16);
            return $"{File.Name}/{value}";
        }
    }

    public SourceReference(IFile file, int lineNumber, int column, int endColumn)
    {
        File = Assert.NotNull(file, nameof(file));
        LineNumber = lineNumber;
        Column = column;
        EndColumn = endColumn;
    }

    public override string ToString()
    {
        var suffix = Column != EndColumn ? $"-{EndColumn}" : string.Empty;
        return $"{File.Name} ({LineNumber}:{Column}{suffix})";
    }

    public bool Includes(Position position)
    {
        return position.LineNumber == LineNumber
            && position.Column >= Column
            && position.Column <= EndColumn;
    }

    protected bool Equals(SourceReference other)
    {
        return LineNumber == other.LineNumber && Column == other.Column && EndColumn == other.EndColumn && File.Equals(other.File);
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((SourceReference)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(LineNumber, Column, EndColumn, File);
    }
}
