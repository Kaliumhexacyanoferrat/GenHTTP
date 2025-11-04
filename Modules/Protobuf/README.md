# GenHTTP.Modules.Protobuf

This package adds protobuf serialization support
to GenHTTP applications.

```csharp
using GenHTTP.Engine.Internal;

using GenHTTP.Modules.Conversion;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.Practices;
using GenHTTP.Modules.Protobuf;
using GenHTTP.Modules.Webservices;

var serialization = Serialization.Default()
                                 .AddProtobuf();

var app = Layout.Create()
                .AddService<...>("...", serializers: serialization);

await Host.Create()
          .Handler(app)
          .Defaults()
          .Console()
          .RunAsync();
```
