using System.Text;
using Lexy.Compiler.Parser;
using Lexy.Web.Editor.Api.Models;
using Lexy.Web.Editor.Api.Requests;
using MediatR;

namespace Lexy.Web.Editor.Api.Handlers;

public class GetProjectFilesHandler : IRequestHandler<GetProjectFiles, ProjectFolder>
{
    public Task<ProjectFolder> Handle(GetProjectFiles request, CancellationToken cancellationToken)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

        var folder = "/Users/timcools/_/Lexy/git/laws"; // todo get project

        var result = new ProjectFolder(request.Project, string.Empty);
        AddFiles(result.Files, folder, string.Empty);
        AddFolders(result.Folders, folder, string.Empty);

        return Task.FromResult(result);
    }

    private void AddFiles(List<ProjectFile> resultFiles, string folder, string fullFolder)
    {
        var files = Directory.GetFiles(folder, $"*.{LexySourceDocument.FileExtension}");
        foreach (var file in files)
        {
            var fileName = Path.GetFileName(file);
            var identifier = Identifier.Encode(fullFolder, fileName);
            var projectFile = new ProjectFile(fileName, fullFolder, identifier);

            resultFiles.Add(projectFile);
        }
    }

    private void AddFolders(List<ProjectFolder> resultFolders, string folder, string fullFolder)
    {
        var folders = Directory.GetDirectories(folder);
        foreach (var eachFolder in folders)
        {
            AddFolder(resultFolders, eachFolder, fullFolder);
        }
    }

    private void AddFolder(List<ProjectFolder> resultFolders, string eachFolder, string fullFolder)
    {
        var folderName = Path.GetFileName(eachFolder);
        if (folderName.StartsWith(".")) return;

        var relativeFullFolder = Path.Combine(fullFolder, folderName);
        var identifier = Identifier.Encode(relativeFullFolder);

        var folderModel = new ProjectFolder(folderName, identifier);

        AddFiles(folderModel.Files, eachFolder, relativeFullFolder);
        AddFolders(folderModel.Folders, eachFolder, relativeFullFolder);
        if (folderModel.Folders.Count > 0 || folderModel.Files.Count > 0)
        {
            resultFolders.Add(folderModel);
        }
    }
}

internal static class Identifier
{
    public static string Encode(string folder)
    {
        if (folder == null) throw new ArgumentNullException(nameof(folder));

        return folder.Replace(Path.DirectorySeparatorChar, '|');
    }

    public static string Encode(string folder, string fileName)
    {
        var full = Path.Combine(folder, fileName);
        return Encode(full);
    }

    public static string Decode(string requestFile)
    {
        if (requestFile.Contains("..")) throw new InvalidOperationException("Not supported.");

        return requestFile.Replace('|', Path.DirectorySeparatorChar);
    }
}