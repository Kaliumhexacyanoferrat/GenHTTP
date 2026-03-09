using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Playground;

public sealed class JsonResult
{

    public string? Message { get; set; }
}

public sealed class JsonHandler : IHandler
{
    private static readonly ReadOnlyMemory<byte> OkPhrase = "OK"u8.ToArray();
    private static readonly ReadOnlyMemory<byte> ContentTypeName = "Content-Type"u8.ToArray();
    private static readonly ReadOnlyMemory<byte> ContentTypeValue = "application/json; charset=utf-8"u8.ToArray();
    
    public ValueTask PrepareAsync() => new();

    public ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        var result = new JsonResult()
        {
            Message = "Hello, World!"
        };

        var response = request.Respond()
                              .Raw()
                              .Content(new FixedLengthJsonContent(result))
                              .Header(ContentTypeName, ContentTypeValue)
                              .Status(200, OkPhrase)
                              .Build();

        return new(response);
    }

}
