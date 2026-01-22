using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.IO.Streaming;

namespace GenHTTP.Modules.IO.Web;

public class WebResource : IResource
{
    private readonly HttpClient _client = new();

    private DateTime? _actualModificationDate;

    private FlexibleContentType? _actualContentType;

    private ulong? _actualContentLength;

    #region Get-/Setters

    public Uri Source { get; }

    public string? Name { get; }

    public DateTime? Modified => field ?? _actualModificationDate;

    public FlexibleContentType? ContentType => field ?? _actualContentType;

    public ulong? Length => _actualContentLength;

    #endregion

    #region Initialization

    public WebResource(Uri source, string? name, DateTime? modified, FlexibleContentType? contentType)
    {
        Source = source;

        Name = name ?? Path.GetFileName(source.LocalPath);

        Modified = modified;
        ContentType = contentType;
    }

    #endregion

    #region Functionality

    public async ValueTask<ulong> CalculateChecksumAsync()
    {
        using var request = new HttpRequestMessage(HttpMethod.Head, Source);

        using var response = await _client.SendAsync(request);

        response.EnsureSuccessStatusCode();

        UpdateFields(response);

        if (response.Headers.ETag != null)
        {
            return (ulong)response.Headers.ETag.Tag.GetHashCode();
        }

        return Checksum.Calculate(this);
    }

    public async ValueTask<Stream> GetContentAsync()
    {
        var response = await _client.GetAsync(Source);

        try
        {
            response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException)
        {
            response.Dispose();
            throw;
        }

        UpdateFields(response);

        var content = await response.Content.ReadAsStreamAsync();

        return new StreamWithDependency(content, response);
    }

    private void UpdateFields(HttpResponseMessage response)
    {
        var contentHeaders = response.Content.Headers;

        if (contentHeaders.LastModified != null)
        {
            _actualModificationDate = contentHeaders.LastModified.Value.UtcDateTime;
        }
        else
        {
            _actualModificationDate = null;
        }

        if (contentHeaders.ContentType != null)
        {
            _actualContentType = FlexibleContentType.Parse(contentHeaders.ContentType.ToString());
        }
        else
        {
            _actualContentType = null;
        }

        if (contentHeaders.ContentLength != null)
        {
            _actualContentLength = (ulong)contentHeaders.ContentLength;
        }
        else
        {
            _actualContentLength = null;
        }
    }

    #endregion

}
