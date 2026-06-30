# GenHTTP.Full.Ioxide

This package provides all available GenHTTP modules
as well as the Ioxide-based server engine.

Please refer to [the documentation](https://genhttp.org/documentation/)
for tutorials, template projects and other examples.

```csharp
using GenHTTP.Engine.Ioxide;

using GenHTTP.Modules.IO;
using GenHTTP.Modules.Practices;

var content = Content.From(Resource.FromString("Hello World!"));

await Host.Create(c => c with
            {
                ReactorCount = Environment.ProcessorCount, 
                BufferRingEntries = 256
            })
          .Handler(content)
          .Defaults()
          .RunAsync(); // or StartAsync() for non-blocking
```
