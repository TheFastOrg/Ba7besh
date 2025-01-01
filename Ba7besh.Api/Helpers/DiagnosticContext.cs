namespace Ba7besh.Api.Helpers;

public class DiagnosticContext
{
    private readonly IDictionary<string, object?> _context = new Dictionary<string, object?>();

    public void Set(string propertyName, object? value)
    {
        _context[propertyName] = value;
    }

    public object? Get(string propertyName)
    {
        return _context.TryGetValue(propertyName, out var value) ? value : null;
    }
}