using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Ba7besh.Application.ReviewManagement;
using Microsoft.Extensions.Options;

namespace Ba7besh.Infrastructure;

public class PhotoStorageOptions
{
    public required string ConnectionString { get; init; }
    public required string ContainerName { get; init; }
    public string? CdnEndpoint { get; init; }

    public long MaxFileSizeBytes { get; init; } = 5 * 1024 * 1024; // 5MB
    public string[]? AllowedContentTypes { get; init; } = ["image/jpeg", "image/png"];
}

public class AzurePhotoStorageService : IPhotoStorageService
{
    private readonly BlobContainerClient _containerClient;
    private readonly PhotoStorageOptions _options;

    public AzurePhotoStorageService(IOptions<PhotoStorageOptions> options)
    {
        _options = options.Value;
        _containerClient = new BlobContainerClient(_options.ConnectionString, _options.ContainerName);
    }

    public async Task<string> UploadPhotoAsync(string fileName, Stream content, string contentType)
    {
        if (!_options.AllowedContentTypes?.Contains(contentType) ?? true)
            throw new InvalidOperationException($"Content type {contentType} is not allowed");

        var blobName = $"{Guid.NewGuid():N}{Path.GetExtension(fileName)}";
        var blobClient = _containerClient.GetBlobClient(blobName);

        await blobClient.UploadAsync(content, new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders { ContentType = contentType }
        });
        return !string.IsNullOrWhiteSpace(_options.CdnEndpoint) ?
            $"{_options.CdnEndpoint}/{blobName}" :
            blobClient.Uri.ToString();
    }

    public async Task DeletePhotoAsync(string photoUrl)
    {
        var uri = new Uri(photoUrl);
        var blobName = uri.Segments.Last();
        var blobClient = _containerClient.GetBlobClient(blobName);
        await blobClient.DeleteIfExistsAsync();
    }
}