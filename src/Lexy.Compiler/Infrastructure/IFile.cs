namespace Lexy.Compiler.Infrastructure;

public interface IFile
{
    IProject Project { get; }
    string Name { get; }
    string BaseFolder { get; }
    string FullPath { get; }
}
