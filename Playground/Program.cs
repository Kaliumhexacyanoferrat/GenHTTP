using GenHTTP.Engine.Ioxide;

using GenHTTP.Modules.Files;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;

// The namespace and the class share a name, so the class needs an alias to be reachable.
using IoxideFilesModule = GenHTTP.Modules.IoxideFiles.IoxideFiles;

// Two static handlers over the SAME directory, so the difference can be priced rather than argued:
//
//   /ring/*  IoxideFiles - ioxide.file opens every file once, shares the descriptors across
//            reactors and reads them positionally off the io_uring ring. Nothing is cached in
//            memory, so resident size stays flat whatever the asset set weighs.
//   /disk/*  GenHTTP's built-in Files module, for comparison.
//
// GENHTTP_STATIC picks the directory; without it neither route is mounted.
//
//     GENHTTP_STATIC=/srv/www dotnet run -c Release --project Playground
//     wrk -t8 -c64 -d8s http://127.0.0.1:8080/ring/asset.bin
//     wrk -t8 -c64 -d8s http://127.0.0.1:8080/disk/asset.bin

var staticDir = Environment.GetEnvironmentVariable("GENHTTP_STATIC");

var app = Layout.Create()
                .Add("ok", Content.From(Resource.FromString("ok")));

if (staticDir != null && Directory.Exists(staticDir))
{
    app = app.Add("ring", IoxideFilesModule.From(staticDir))
             .Add("disk", Assets.From(staticDir));
}

await Host.Create()
          .Handler(app)
          .RunAsync();
