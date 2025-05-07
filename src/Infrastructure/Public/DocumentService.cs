using Demo.WebApi.Application.Common.Exceptions;
using Demo.WebApi.Application.Common.Persistence;
using Demo.WebApi.Application.Public.Document;
using Demo.WebApi.Application.Storage;
using Demo.WebApi.Domain.Common.Enums;
using Demo.WebApi.Domain.Public;
using Demo.WebApi.Infrastructure.Common.Extensions;
using Demo.WebApi.Infrastructure.FileStorage;
using Demo.WebApi.Shared.Localization;
using DocumentFormat.OpenXml.Drawing.Charts;
using Mapster;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System.Linq;

namespace Demo.WebApi.Infrastructure.Public;
public class DocumentService : IDocumentService
{
    private readonly IRepository<Document> _documentRepository;
    private readonly IAzureStorageService _azureService;
    private readonly IStringLocalizer<DocumentService> _localizer;
    private readonly AzureStorageSettings _azureSettings;

    public DocumentService(IRepository<Document> documentRepository, IAzureStorageService azureService, IStringLocalizer<DocumentService> localizer, IOptions<AzureStorageSettings> azureSettings)
    {
        _documentRepository = documentRepository;
        _azureService = azureService;
        _localizer = localizer;
        _azureSettings = azureSettings.Value;
    }

    public async Task<DocumentResponse> AddDocumentAsync(DocumentRequest request, bool compress = false, int? height = null, int? width = null)
    {
        var mediaStream = new MediaStream()
        {
            OriginalFileName = request.DocumentFile.FileName
        };

        if (request.DocumentFile.Length > _azureSettings.FileSizeLimit)
        {
            throw new ConflictException(_localizer[MessageConstants.FileSizeTooLarge, _azureSettings.FileSizeLimit / 1048576]);
        }

        string extension = Path.GetExtension(mediaStream.OriginalFileName).ToLower();

        if (compress && FileType.Image.GetDescriptionList().Any(x => x == extension))
            mediaStream.InputStream = ImageCompression.ResizeImage(request.DocumentFile.OpenReadStream(), height!.Value, width!.Value);
        else
            mediaStream.InputStream = request.DocumentFile.OpenReadStream();

        string[] allowedExtensions = _azureSettings.DocumentAllowedExtension.Split(',');

        if (!allowedExtensions.Contains(extension))
        {
            throw new ConflictException(_localizer[MessageConstants.MediaExtensionInvalid, _azureSettings.DocumentAllowedExtension]);
        }

        string convertedFileName = $"{Guid.NewGuid()}{Path.GetExtension(mediaStream.OriginalFileName).ToLower()}";

        //string tempPath = UploadFileOnTempPath(mediaStream.InputStream, convertedFileName);

        var document = new Document
        {
            ConvertedFileName = convertedFileName,
            FileType = extension,
            OriginalFileName = mediaStream.OriginalFileName,
        };

        await _documentRepository.AddAsync(document);

        var uploadResponse = await _azureService.UploadAsync(mediaStream.InputStream, request.Path, convertedFileName);

        document.AccessURL = uploadResponse.BlobUrlWithSasToken;
        document.Path = uploadResponse.BlobPath;

        await _documentRepository.UpdateAsync(document);

        return document.Adapt<DocumentResponse>();
    }

    public async Task<DocumentResponse> AddDocumentAsync(DocumentFromStreamRequest request)
    {
        var mediaStream = new MediaStream
        {
            OriginalFileName = request.DocumentName,
            InputStream = new MemoryStream(request.Document)
        };
        string convertedFileName = $"{Guid.NewGuid()}{Path.GetExtension(mediaStream.OriginalFileName).ToLower()}";

        var document = new Document
        {
            ConvertedFileName = convertedFileName,
            FileType = request.Extension,
            OriginalFileName = mediaStream.OriginalFileName,
        };

        await _documentRepository.AddAsync(document);

        var uploadResponse = await _azureService.UploadAsync(mediaStream.InputStream, request.Path, convertedFileName);

        document.AccessURL = uploadResponse.BlobUrlWithSasToken;
        document.Path = uploadResponse.BlobPath;

        await _documentRepository.UpdateAsync(document);

        return document.Adapt<DocumentResponse>();
    }

    public async Task<List<DocumentResponse>> AddMultipleDocumentsAsync(MultipleDocumentRequest request)
    {
        var requestDocuments = request.Documents.Select(rd => new
        {
            MediaStream = new MediaStream
            {
                OriginalFileName = rd.FileName,
                InputStream = rd.OpenReadStream()
            },
            Extension = Path.GetExtension(rd.FileName).ToLower(),
            ConvertedFileName = $"{Guid.NewGuid()}{Path.GetExtension(rd.FileName).ToLower()}",
            FileLength = rd.Length
        }).ToList();

        if (request.Documents.Any(r => r.Length > _azureSettings.FileSizeLimit))
            throw new ConflictException(_localizer[MessageConstants.FileSizeTooLarge, _azureSettings.FileSizeLimit / 1048576]);

        var allowedExtensions = _azureSettings.DocumentAllowedExtension.Split(',').ToHashSet();

        if (requestDocuments.Any(d => !allowedExtensions.Contains(d.Extension)))
            throw new ConflictException(_localizer[MessageConstants.MediaExtensionInvalid, _azureSettings.DocumentAllowedExtension]);

        List<Task<AzureUploadResponse>> uploadTasks = new();

        requestDocuments.ForEach(rd =>
        {
            uploadTasks.Add(_azureService.UploadAsync(
                rd.MediaStream.InputStream,
                request.Path,
                rd.ConvertedFileName));
        });

        var uploads = await Task.WhenAll(uploadTasks);

        var documents = requestDocuments.Select(rd => new Document
        {
            ConvertedFileName = rd.ConvertedFileName,
            FileType = rd.Extension,
            OriginalFileName = rd.MediaStream.OriginalFileName,
            Path = uploads.FirstOrDefault(u => u.FileName == rd.ConvertedFileName).BlobPath,
            AccessURL = uploads.FirstOrDefault(u => u.FileName == rd.ConvertedFileName).BlobUrlWithSasToken
        }).ToList();

        await _documentRepository.AddRangeAsync(documents);

        return documents.Adapt<List<DocumentResponse>>();
    }

    public async Task<bool> DeleteDocumentAsync(int documentId)
    {
        throw new NotImplementedException();
    }
}
