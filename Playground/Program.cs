using GenHTTP.Api.Protocol;

using GenHTTP.Engine.Internal;

using GenHTTP.Modules.IO;
using GenHTTP.Modules.IO.Streaming;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.Practices;
using GenHTTP.Modules.Webservices;

var app = Layout.Create()
                .AddService<ContentExamples>("content");

await Host.Create()
          .Handler(app)
          .Defaults()
          .Console()
          .RunAsync();

class ContentExamples
{

    [ResourceMethod("get-string")]
    public IResponseBuilder GetString(IRequest request)
    {
        return request.Respond()
                      .Content("This is a string");
    }

    [ResourceMethod("get-resource")]
    public IResponseBuilder GetResource(IRequest request)
    {
        var resource = Resource.FromString("This is a string") // or from any other source
                               .Build();

        return request.Respond()
                      .Content(resource);
    }

    [ResourceMethod("get-stream")]
    public IResponseBuilder GetStream(IRequest request)
    {
        var stream = new MemoryStream("This is a string"u8.ToArray());

        return request.Respond()
                      .Content(stream, (ulong)stream.Length, stream.CalculateChecksumAsync)
                      .Type(FlexibleContentType.Get(ContentType.TextPlain));
    }

}
