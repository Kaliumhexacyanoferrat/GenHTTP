# GenHTTP.Modules.Archives

Provides functionality for archives (e.g. ZIP, RAR, 7z) such as `IResourceTree`
implementations.

```csharp
var archive = Resource.FromFile("./content.zip");

var tree = ArchiveTree.From(archive);

var app = StaticWebsite.From(tree);

await Host.Create()
          .Handler(app)
          .RunAsync();
```

For additional examples,please refer to [the documentation](https://genhttp.org/documentation/content/concepts/resources/#resource-trees).
