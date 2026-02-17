using Lexy.Compiler.Infrastructure;

namespace Lexy.Tests;

public class TestFile : IFile
{
    private static readonly IProject project = new Project(new FileSystem());

    public IProject Project => project;
    public string Name => "tests.lexy";
    public string BaseFolder => "/";
    public string FullPath => "/tests.lexy";

    public static IFile Instance => new TestFile();
}
