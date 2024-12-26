namespace Ba7besh.Application.Exceptions;

public class BusinessNotFoundException(string businessId)
    : Exception($"Business with ID '{businessId}' was not found")
{
    public string BusinessId { get; } = businessId;
}