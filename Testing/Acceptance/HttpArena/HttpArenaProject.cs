using GenHTTP.Api.Content;

using GenHTTP.Modules.Compression.Algorithms;
using GenHTTP.Modules.Files;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.Layouting.Provider;
using GenHTTP.Modules.Webservices;

namespace GenHTTP.Testing.Acceptance.HttpArena;

public static class HttpArenaProject
{
    public static string DataPath { get; } = FindDataPath();

    public static IHandlerBuilder Create()
    {
        var crud = Layout.Create()
                         .AddService<CrudService>("items");

        var app = Layout.Create()
                        .Add("pipeline", Content.From(Resource.FromString("ok")))
                        .AddService<BaselineService>("baseline11")
                        .AddService<UploadService>("upload")
                        .AddService<JsonService>("json")
                        .AddService<AsyncDbService>("async-db")
                        .Add("crud", crud)
                        .AddStaticFiles();

        return app;
    }

    private static LayoutBuilder AddStaticFiles(this LayoutBuilder app)
    {
        var staticPath = Path.Combine(DataPath, "static");

        if (Directory.Exists(staticPath))
        {
            var handler = Assets.From(staticPath)
                                .AllowPrecompressed(new BrotliAlgorithm());

            app.Add("static", handler);
        }

        return app;
    }

    private static string FindDataPath()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);

        while (dir?.Parent != null)
        {
            var candidate = Path.Combine(dir.Parent.FullName, "HttpArena", "data");

            if (Directory.Exists(candidate))
            {
                return candidate;
            }

            dir = dir.Parent;
        }

        return Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../../HttpArena/data"));
    }
}
