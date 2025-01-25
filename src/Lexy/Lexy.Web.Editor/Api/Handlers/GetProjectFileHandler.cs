using Lexy.Compiler.Parser;
using Lexy.Web.Editor.Api.Models;
using Lexy.Web.Editor.Api.Requests;
using MediatR;

namespace Lexy.Web.Editor.Api.Handlers;

public class GetProjectFileHandler : IRequestHandler<GetProjectFile, ProjectFileDetails>
{
    public Task<ProjectFileDetails> Handle(GetProjectFile request, CancellationToken cancellationToken)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

        var folder = "/Users/timcools/_/Lexy/git/laws"; // todo get project
        var fileName = Identifier.Decode(request.File);

        var fullPath = Path.Combine(folder, fileName);
        if (!File.Exists(fullPath)) throw new InvalidOperationException("File not found.");

        var code = File.ReadAllText(fullPath);
        var result = new ProjectFileDetails(Path.GetFileName(fullPath), request.File, code);

        return Task.FromResult(result);
    }
}