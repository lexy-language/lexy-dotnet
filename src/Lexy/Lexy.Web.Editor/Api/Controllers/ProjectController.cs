using Lexy.Web.Editor.Api.Models;
using Lexy.Web.Editor.Api.Requests;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Lexy.Web.Editor.Api.Controllers;

[ApiController]
public class ProjectController : ControllerBase
{
    private readonly IMediator mediator;

    public ProjectController(IMediator mediator)
    {
        this.mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    [HttpGet("api/project/{project}/file")]
    public async Task<ProjectFolder> GetFiles(string project)
    {
        var request = new GetProjectFiles(project);
        return await mediator.Send(request);
    }

    [HttpGet("api/project/{project}/file/{file}")]
    public async Task<ProjectFileDetails> GetFiles(string project, string file)
    {
        var request = new GetProjectFile(project, file);
        return await mediator.Send(request);
    }
}