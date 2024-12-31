using Ba7besh.Application.ReviewManagement;

namespace Ba7besh.Api.Models;

public class ReviewPhotoDto(IFormFile file, string? description = null) : IReviewPhotoUpload
{
    public string? Description { get; init; } = description;

    public Stream OpenReadStream() => file.OpenReadStream();
    public string ContentType => file.ContentType;
    public string FileName => file.FileName;
    public long Length => file.Length;
}