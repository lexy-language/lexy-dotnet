using Lexy.Web.Editor.Api.Models;
using MediatR;

namespace Lexy.Web.Editor.Api.Requests
{
    public class GetProjectFiles : IRequest<ProjectFolder>
    {
        public string Project { get; }

        public GetProjectFiles(string project)
        {
            Project = project;
        }
    }
}