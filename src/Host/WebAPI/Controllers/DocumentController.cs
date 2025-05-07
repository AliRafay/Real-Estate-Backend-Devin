using Demo.WebApi.Application.Public.Document;
using Demo.WebApi.Infrastructure.Common.Extensions;

namespace Demo.WebApi.Host.Controllers;

[Authorize]
public class DocumentController : VersionNeutralApiController
{
    private readonly IDocumentService _documentService;

    public DocumentController(IDocumentService documentService)
    {
        _documentService = documentService;
    }

    [HttpPost]
    [OpenApiOperation("Upload a document.", "")]
    public async Task<HttpResponseDto<DocumentResponse>> AddOrUpdateAdminImageAsync([FromForm] DocumentRequest request)
    {
        var response = await _documentService.AddDocumentAsync(request);
        return response.ToInformationResponse();
    }

    [HttpPost("documents")]
    [OpenApiOperation("Upload multiple documents", "")]

    public async Task<HttpResponseDto<List<DocumentResponse>>> UploadMultipleDocumentsAsync([FromForm] MultipleDocumentRequest request)
    {
        var response = await _documentService.AddMultipleDocumentsAsync(request);
        return response.ToInformationResponse();
    }
}
