# GenHTTP.Modules.IO

Provides I/O related functionality to be used within a GenHTTP application.

## Resources

Access to resources such as files are abstracted in GenHTTP via `IResourceTree`
and `IResource`. The I/O module adds common implementations to load resources from
strings, files or an assembly. It also adds useful features such as virtual resource
trees or change tracking for resources.

See [the documentation](https://genhttp.org/documentation/content/concepts/resources/) for more details.

## Content Implementations

Besides the resource implementations, the module adds some `IResponseContent`
implementations that can be used to directly set the content when building a response.

```csharp
using GenHTTP.Api.Protocol;

using GenHTTP.Engine.Internal;

using GenHTTP.Modules.IO;
using GenHTTP.Modules.IO.Streaming;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.Practices;
using GenHTTP.Modules.Webservices;

using StreamContent = GenHTTP.Modules.IO.Streaming.StreamContent;
using StringContent = GenHTTP.Modules.IO.Strings.StringContent;

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
                      .Content(new StringContent("This is a string"))
                      .Type(FlexibleContentType.Get(ContentType.TextPlain));
    }

    [ResourceMethod("get-resource")]
    public IResponseBuilder GetResource(IRequest request)
    {
        var resource = Resource.FromString("This is a string") // or from any other source
                               .Build();

        return request.Respond()
                      .Content(new ResourceContent(resource))
                      .Type(FlexibleContentType.Get(ContentType.TextPlain));
    }

    [ResourceMethod("get-stream")]
    public IResponseBuilder GetStream(IRequest request)
    {
        var stream = new MemoryStream("This is a string"u8.ToArray());

        return request.Respond()
                      .Content(new StreamContent(stream, (ulong)stream.Length, stream.CalculateChecksumAsync))
                      .Type(FlexibleContentType.Get(ContentType.TextPlain));
    }

}
```

To simplify their usage, the module also adds some extensions to the `IResponseBuilder`:

```csharp
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
```

## Providers

The following handlers and concerns are provided by this module.

### Content

Provides a single resource to a requesting client. In contrast to downloads,
the response is expected to be consumed by the client without actually downloading it
to a local file. The `Content-Type` is computed from the given resource. For named
resources, the logic will try to guess the content type from the file extension.

```csharp
using GenHTTP.Engine.Internal;

using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.Practices;

var content = Content.From(Resource.FromString("Hello World!"));

var app = Layout.Create()
                .Add("hello", content);

await Host.Create()
          .Handler(app)
          .Defaults()
          .Console()
          .RunAsync();
```

The module also provides extensions 

### Downloads

Allows clients to download a single resource and to specify the name as
it should be presented to the user. The logic tries to automatically
determine the `Content-Type` of the resource.

```csharp
using GenHTTP.Api.Protocol;

using GenHTTP.Engine.Internal;

using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.Practices;

var file = Resource.FromFile("./my.pdf");

var download = Download.From(file)
                       .FileName("invoice-user-1234.pdf") // optional, would be "my.pdf" otherwise
                       .Type(ContentType.ApplicationPdf); // optional, automatically detected here

var app = Layout.Create()
                .Add("get-invoice", download);

await Host.Create()
          .Handler(app)
          .Defaults()
          .Console()
          .RunAsync();
```

### Range Support

This concern allows clients to request only a specific part of the data provided by the inner handler.
For this to work, the inner handler needs to set a `Content-Length` on the response. Chunked responses
are not supported. Requires the HTTP method to be `GET` or `HEAD`.

The concern will set a `Accept-Ranges: bytes` header to signal range support to the client.

The client can then request a specific byte range using the `Range` header, e.g. `Range: 100-` to skip
the first 100 bytes of the response.

While this concern can safely be applied to the root handler, it is recommended to
apply it only to the handlers where the functionality is needed, as there is some
runtime overhead for each request.

```csharp
using GenHTTP.Engine.Internal;

using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.Practices;

var download = Download.From(Resource.FromFile("my-large.bin"))
                       .AddRangeSupport();

var app = Layout.Create()
                .Add("file", download);

await Host.Create()
          .Handler(app)
          .Defaults()
          .Console()
          .RunAsync();
```
