namespace Ba7besh.Application.Helpers;

public static class EnumExtensions
{
    public static string? ToLowerString<TEnum>(this TEnum enumValue) where TEnum : Enum
    {
        return Enum.GetName(typeof(TEnum), enumValue)?.ToLower();
    }

    public static TEnum FromLowerString<TEnum>(this string stringValue) where TEnum : Enum
    {
        return Enum.TryParse(typeof(TEnum), stringValue, true, out var result)
            ? (TEnum)result
            : throw new ArgumentException($"Invalid value '{stringValue}' for enum type {typeof(TEnum).Name}");
    }
}