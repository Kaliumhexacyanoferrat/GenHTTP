namespace GenHTTP.Api.Protocol;

public interface IResponseContent
{

    ValueTask WriteAsync(IResponseSink sink);

}
