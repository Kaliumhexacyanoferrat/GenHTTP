using GenHTTP.Api.Content;

using GenHTTP.Modules.Files;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.Webservices;

using genhttp.Tests;

namespace genhttp;

public static class Project
{

    // HTTP/1.1 endpoints exercised by this entry's profiles:
    //   baseline / limited-conn -> /baseline11   (Baseline webservice: GET/POST sum)
    //   pipelined               -> /pipeline     (fixed "ok")
    //   json / json-comp        -> /json/{count}?m=N   (json-comp = json + Accept-Encoding: br)
    //   upload                  -> /upload       (streamed request-body byte count)
    //   static                  -> /static/...   (files from IOXIDE_STATIC, when the dir exists)
    public static IHandlerBuilder Create()
    {
        var layout = Layout.Create()
                           .Add("pipeline", Content.From(Resource.FromString("ok")))
                           .AddService<Baseline>("baseline11")
                           .AddService<Baseline>("baseline2")
                           .AddService<JsonService>("json")
                           .AddService<UploadService>("upload");

        // async-db and crud require a configured Postgres (DATABASE_URL).
        if (Db.Enabled)
        {
            layout = layout.AddService<AsyncDbService>("async-db")
                           .AddService<CrudService>("crud");
        }

        var staticDir = Environment.GetEnvironmentVariable("IOXIDE_STATIC") ?? "/data/static";

        if (Directory.Exists(staticDir))
        {
            layout = layout.Add("static", Assets.From(ResourceTree.FromDirectory(staticDir)));
        }

        return layout;
    }

}
