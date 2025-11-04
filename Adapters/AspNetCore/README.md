# GenHTTP.Adapters.AspNetCore

This package provides an adapter that allows
an ASP.NET Core app to use the handlers of
the GenHTTP framework.

The following example will display a file listing
on `/files` when opened in the browser, using the
capabilities provided by the `DirectoryBrowsing` module.

```csharp
using GenHTTP.Adapters.AspNetCore;

using GenHTTP.Modules.DirectoryBrowsing;
using GenHTTP.Modules.IO;

using Microsoft.AspNetCore.Builder;

var builder = WebApplication.CreateBuilder();

var app = builder.Build();

var listing = Listing.From(ResourceTree.FromDirectory("."))
                     .Defaults();

app.Map("/files", listing);

await app.RunAsync();
```
