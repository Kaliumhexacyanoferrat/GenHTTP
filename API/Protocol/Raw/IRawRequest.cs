namespace GenHTTP.Api.Protocol.Raw;


public interface IRawRequest
{

    IRawRequestHeader Header { get;}

    IRawRequestBody? GetBody(HeaderAccess headerAccess);
    
    // todo: wrap body (e.g. content decoding)

}

public enum HeaderAccess
{
    Retain,
    Release
}
