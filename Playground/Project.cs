using GenHTTP.Api.Content;

using GenHTTP.Modules.Compression.Algorithms;
using GenHTTP.Modules.Files;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.Layouting.Provider;
using GenHTTP.Modules.Webservices;

using genhttp.Infrastructure;
using genhttp.Tests;

namespace genhttp;

public static class Project
{

    // HTTP/1.1 endpoints exercised by this entry's profiles:
    //   baseline / limited-conn -> /baseline11   (Baseline webservice: GET/POST sum)
    //   pipelined               -> /pipeline     (fixed "ok")
    //   json / json-comp        -> /json/{count}?m=N   (json-comp = json + Accept-Encoding: br)
    //   upload                  -> /upload       (streamed request-body byte count)
    //   async-db                -> /async-db     (Postgres range query, when DATABASE_URL is set)
    //   crud                    -> /crud/items   (list/read/create/update, when DATABASE_URL is set)
    //   static                  -> /static/...   (files from IOXIDE_STATIC, when the dir exists)
    public static IHandlerBuilder Create()
    {
        var app = Layout.Create()
                        .Add("pipeline", Content.From(Resource.FromString("ok")))
                        .AddService<Baseline>("baseline11")
                        .AddService<Baseline>("baseline2")
                        .AddService<Upload>("upload")
                        .AddService<Json>("json");

        // async-db and crud require a configured Postgres (DATABASE_URL).
        if (Postgres.Enabled)
        {
            var crud = Layout.Create()
                             .AddService<Crud>("items");

            app = app.AddService<AsyncDatabase>("async-db")
                     .Add("crud", crud);
        }

        return app.AddStaticFiles();
    }

    private static LayoutBuilder AddStaticFiles(this LayoutBuilder app)
    {
        var staticDir = Environment.GetEnvironmentVariable("IOXIDE_STATIC") ?? "/data/static";

        if (Directory.Exists(staticDir))
        {
            var handler = Assets.From(ResourceTree.FromDirectory(staticDir))
                                .AllowPrecompressed(new BrotliAlgorithm());

            app.Add("static", handler);
        }

        return app;
    }

}
