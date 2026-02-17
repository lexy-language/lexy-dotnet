using Lexy.RunTime;

namespace Lexy.Compiler.Infrastructure;

public class Project : IProject
{
    public string BaseFolder { get; }
    public IFileSystem FileSystem { get; }

    public Project(IFileSystem fileSystem)
    {
        FileSystem = Assert.NotNull(fileSystem, nameof(fileSystem));
        BaseFolder = fileSystem.CurrentFolder();
    }

    public Project(string baseFolder, IFileSystem fileSystem)
    {
        FileSystem = Assert.NotNull(fileSystem, nameof(fileSystem));
        BaseFolder = fileSystem.GetFullPath(baseFolder);
    }

    public IFile File(string name)
    {
        return new File(this, name);
    }
}
