namespace Lexy.Compiler.Infrastructure;

public interface IProject
{
    string BaseFolder { get; }
    IFileSystem FileSystem { get; }

    IFile File(string fileName);
}
