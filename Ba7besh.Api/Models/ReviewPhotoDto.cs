using Ba7besh.Application.ReviewManagement;

namespace Ba7besh.Api.Models;

public class ReviewPhotoDto : IReviewPhotoUpload
{
    private readonly byte[] _fileBytes;

    public ReviewPhotoDto(string dataUri, string? fileName, string? description)
    {
        if (string.IsNullOrEmpty(dataUri))
            throw new ArgumentException("Data URI cannot be null or empty.", nameof(dataUri));

        var parts = dataUri.Split([','], 2);
        if (parts.Length != 2 || !parts[0].StartsWith("data:"))
            throw new FormatException("Invalid data URI format.");

        ContentType = parts[0].Split([':'], 2)[1].Split([';'], 2)[0];
        _fileBytes = Convert.FromBase64String(parts[1]);

        FileName = fileName ?? GenerateFileName(ContentType);
        Description = description;
        Length = _fileBytes.LongLength;
    }

    public string? Description { get; }

    public Stream OpenReadStream()
    {
        return new MemoryStream(_fileBytes);
    }

    public string ContentType { get; }

    public string FileName { get; }

    public long Length { get; }
    private static string GenerateFileName(string contentType)
    {
        var extension = contentType.Split('/')[1];
        if (string.IsNullOrEmpty(extension))
            throw new FormatException("Invalid content type format.");

        return $"{Guid.NewGuid()}.{extension}";
    }
}