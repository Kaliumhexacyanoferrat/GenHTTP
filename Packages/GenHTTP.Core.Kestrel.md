# GenHTTP.Core.Kestrel

This package provides the basic dependencies
required to run a GenHTTP application using
Kestrel as a server engine.

Please refer to [the documentation](https://genhttp.org/documentation/)
for tutorials, template projects and other examples.

```csharp
using GenHTTP.Engine.Kestrel;

using GenHTTP.Modules.IO;
using GenHTTP.Modules.Practices;

var content = Content.From(Resource.FromString("Hello World!"));

await Host.Create()
          .Handler(content)
          .Defaults()
          .RunAsync(); // or StartAsync() for non-blocking
```
