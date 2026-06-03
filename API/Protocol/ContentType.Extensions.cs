namespace GenHTTP.Api.Protocol;

public static class ContentTypeExtensions
{
    
    public static ContentType WithoutOptions(this ContentType type)
    {
        var span = type.Bytes.Span;
        var idx = span.IndexOf((byte)';');

        if (idx < 0)
        {
            return type;
        }

        var trimmed = span[..idx];

        while (!trimmed.IsEmpty && trimmed[^1] == (byte)' ')
        {
            trimmed = trimmed[..^1];
        }

        return new(trimmed.ToArray());
    }
    
}
