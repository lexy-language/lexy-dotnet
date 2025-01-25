using Lexy.Web.Editor.Api.Models;
using MediatR;

namespace Lexy.Web.Editor.Api.Requests;

public class GetProjectFile : IRequest<ProjectFileDetails>
{
    public string Project { get; }
    public string File { get; }

    public GetProjectFile(string project, string file)
    {
        Project = project;
        File = file;
    }
}