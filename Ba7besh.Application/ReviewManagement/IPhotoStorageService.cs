namespace Ba7besh.Application.ReviewManagement;

public interface IPhotoStorageService
{
    Task<string> UploadPhotoAsync(string fileName, Stream content, string contentType);
    Task DeletePhotoAsync(string photoUrl);
}