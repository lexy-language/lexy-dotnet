using System;
using Lexy.RunTime;

namespace Lexy.Compiler.Infrastructure;

public class File : IFile
{
    public IProject Project { get; }

    public string Name { get; }

    public string BaseFolder => Project.BaseFolder;
    public string FullPath => Project.FileSystem.Combine(Project.BaseFolder, Name);

    public File(Project project, string name)
    {
        Project = Assert.NotNull(project, nameof(project));
        Name = name;
    }

    protected bool Equals(File other)
    {
        return BaseFolder == other.BaseFolder && FullPath == other.FullPath && Name == other.Name;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((File)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(BaseFolder, Name);
    }

    public override string ToString() => Name;
}
