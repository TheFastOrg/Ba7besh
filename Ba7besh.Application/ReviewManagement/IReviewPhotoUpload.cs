namespace Ba7besh.Application.ReviewManagement;

public interface IReviewPhotoUpload 
{
    string? Description { get; }
    Stream OpenReadStream();
    string ContentType { get; }
    string FileName { get; }
    long Length { get; }
}