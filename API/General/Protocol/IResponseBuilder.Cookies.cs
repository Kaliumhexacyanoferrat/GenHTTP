namespace GenHTTP.Api.Protocol;

/// <summary>
/// Allows cookies to be added to a response via the "Set-Cookie" header.
/// </summary>
public static class CookieResponseExtensions
{
    private const int DateLength = 29; // RFC1123, e.g. "Tue, 03 Jan 2017 04:05:06 GMT"

    private static ReadOnlySpan<byte> ExpiresPrefix => "; Expires="u8;

    private static ReadOnlySpan<byte> MaxAgePrefix => "; Max-Age="u8;

    private static ReadOnlySpan<byte> DomainPrefix => "; Domain="u8;

    private static ReadOnlySpan<byte> PathPrefix => "; Path="u8;

    private static ReadOnlySpan<byte> SecureFlag => "; Secure"u8;

    private static ReadOnlySpan<byte> HttpOnlyFlag => "; HttpOnly"u8;

    private static ReadOnlySpan<byte> SameSitePrefix => "; SameSite="u8;

    #region Functionality

    /// <summary>
    /// Adds a cookie with the given name, value and attributes to the response.
    /// </summary>
    /// <param name="builder">The response the cookie should be added to</param>
    /// <param name="name">The name of the cookie</param>
    /// <param name="value">The value of the cookie</param>
    /// <param name="options">The attributes to be set on the cookie</param>
    public static IResponseBuilder Cookie(this IResponseBuilder builder, ByteString name, ByteString value, CookieOptions options = default)
        => builder.Header(KnownHeaders.SetCookie, Build(name, value, options));

    /// <summary>
    /// Adds a cookie with the given name, value and attributes to the response.
    /// </summary>
    /// <param name="builder">The response the cookie should be added to</param>
    /// <param name="name">The name of the cookie</param>
    /// <param name="value">The value of the cookie</param>
    /// <param name="options">The attributes to be set on the cookie</param>
    public static IResponseBuilder Cookie(this IResponseBuilder builder, string name, string value, CookieOptions options = default)
        => builder.Cookie(new ByteString(name), new ByteString(value), options);

    #endregion

    #region Building

    private static ByteString Build(ByteString name, ByteString value, CookieOptions options)
    {
        var length = name.Bytes.Length + 1 + value.Bytes.Length;

        if (options.Expires is not null) length += ExpiresPrefix.Length + DateLength;
        if (options.MaxAge is not null) length += MaxAgePrefix.Length + CountDigits((long)options.MaxAge.Value.TotalSeconds);
        if (options.Domain is not null) length += DomainPrefix.Length + options.Domain.Value.Bytes.Length;
        if (options.Path is not null) length += PathPrefix.Length + options.Path.Value.Bytes.Length;
        if (options.Secure) length += SecureFlag.Length;
        if (options.HttpOnly) length += HttpOnlyFlag.Length;
        if (options.SameSite is not null) length += SameSitePrefix.Length + GetSameSite(options.SameSite.Value).Length;

        var buffer = new byte[length];
        var position = 0;

        name.Bytes.Span.CopyTo(buffer);
        position += name.Bytes.Length;

        buffer[position++] = (byte)'=';

        value.Bytes.Span.CopyTo(buffer.AsSpan(position));
        position += value.Bytes.Length;

        if (options.Expires is not null)
        {
            position += WriteSpan(buffer.AsSpan(position), ExpiresPrefix);

            options.Expires.Value.UtcDateTime.TryFormat(buffer.AsSpan(position), out var written, "r");
            position += written;
        }

        if (options.MaxAge is not null)
        {
            position += WriteSpan(buffer.AsSpan(position), MaxAgePrefix);

            ((long)options.MaxAge.Value.TotalSeconds).TryFormat(buffer.AsSpan(position), out var written);
            position += written;
        }

        if (options.Domain is not null)
        {
            position += WriteSpan(buffer.AsSpan(position), DomainPrefix);
            position += WriteSpan(buffer.AsSpan(position), options.Domain.Value.Bytes.Span);
        }

        if (options.Path is not null)
        {
            position += WriteSpan(buffer.AsSpan(position), PathPrefix);
            position += WriteSpan(buffer.AsSpan(position), options.Path.Value.Bytes.Span);
        }

        if (options.Secure)
        {
            position += WriteSpan(buffer.AsSpan(position), SecureFlag);
        }

        if (options.HttpOnly)
        {
            position += WriteSpan(buffer.AsSpan(position), HttpOnlyFlag);
        }

        if (options.SameSite is not null)
        {
            position += WriteSpan(buffer.AsSpan(position), SameSitePrefix);
            WriteSpan(buffer.AsSpan(position), GetSameSite(options.SameSite.Value));
        }

        return new ByteString(buffer);
    }

    private static int WriteSpan(Span<byte> target, ReadOnlySpan<byte> value)
    {
        value.CopyTo(target);
        return value.Length;
    }

    private static int CountDigits(long value)
    {
        var sign = 0;

        if (value < 0)
        {
            sign = 1;
            value = -value;
        }

        var digits = 1;

        value /= 10;

        while (value > 0)
        {
            digits++;
            value /= 10;
        }

        return digits + sign;
    }

    private static ReadOnlySpan<byte> GetSameSite(SameSite value) => value switch
    {
        SameSite.Strict => "Strict"u8,
        SameSite.Lax => "Lax"u8,
        _ => "None"u8
    };

    #endregion

}
