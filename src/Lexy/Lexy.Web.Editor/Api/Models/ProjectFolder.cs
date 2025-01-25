namespace Lexy.Web.Editor.Api.Models;

public class ProjectFolder
{
    public string Name { get; }
    public string Identifier { get; }

    public List<ProjectFile> Files { get; } = new();
    public List<ProjectFolder> Folders { get; } = new();

    public ProjectFolder(string name, string identifier)
    {
        Name = name;
        Identifier = identifier;
    }
}