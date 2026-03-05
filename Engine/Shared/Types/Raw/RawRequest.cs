using GenHTTP.Api.Protocol.Raw;
using Glyph11.Protocol;

namespace GenHTTP.Engine.Shared.Types;

public sealed class RawRequest : IRawRequest
{
    private bool _bodyObtained;

    private readonly RawRequestHeader _header;

    private IRawRequestHeader? _retainedHeader;

    public IRawRequestHeader Header
    {
        get
        {
            if (!_bodyObtained)
            {
                return _header;
            }

            if (_retainedHeader == null)
            {
                throw new InvalidOperationException("Header information can no longer be accessed");
            }

            return _retainedHeader;
        }
    }

    internal BinaryRequest Source { get; }

    public RawRequest()
    {
        Source = new();

        _header = new(this);
    }

    public IRawRequestBody? GetBody(HeaderAccess headerAccess)
    {
        if (_bodyObtained)
        {
            throw new InvalidOperationException("Request body can only be fetched once.");
        }

        if (headerAccess == HeaderAccess.Retain)
        {
            _retainedHeader = new RetainedRequestHeader(_header);
        }

        _bodyObtained = true;

        // todo
        return null;
    }

    public void Apply()
    {
        _header.Apply();

        _bodyObtained = false;
        _retainedHeader = null;
    }
    
}
