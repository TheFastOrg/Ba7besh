namespace Ba7besh.Application.ReviewManagement;

public record ReviewPhoto
{
    public required string Id { get; init; }
    public required string ReviewId { get; init; }
    public required string PhotoUrl { get; init; }
    public string? Description { get; init; }
    public required string MimeType { get; init; }
    public required long SizeBytes { get; init; }
}
