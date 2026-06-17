using System.IO.Pipelines;
using System.Net;

using genhttp;

using GenHTTP.Engine.Ioxide;
using GenHTTP.Modules.Compression;

using ioxide;
using ioxide.pg;

// Reactor count follows the available CPUs (api-4 / api-16 control this via cpuset pinning);
// override with IOXIDE_REACTORS.
var reactors = int.TryParse(Environment.GetEnvironmentVariable("IOXIDE_REACTORS"), out var rc) ? rc : Environment.ProcessorCount;

// Shared benchmark dataset for the json / json-comp profiles (harness mounts /data/dataset.json).
Data.LoadDataset(Environment.GetEnvironmentVariable("IOXIDE_DATASET") ?? "/data/dataset.json");

// Postgres (async-db / crud) and TLS (json-tls on :8081) are both per-reactor; configure them and
// fold their per-reactor init into one OnStart.
var pg = Db.Configure(reactors);
var tls = Tls.Configure();

Action<Reactor>? onReactorStart = (pg is null && !tls) ? null : r =>
{
    if (pg != null)
    {
        PgPool.Start(r, pg);
    }
    if (tls)
    {
        Tls.StartService(r);
    }
};

// json-tls adds a second, TLS-terminating listener on :8081 (kTLS TX); :8080 stays plaintext.
Func<Connection, ValueTask<IDuplexPipe>>? connectionFactory = null;
if (tls)
{
    connectionFactory = Tls.CreatePipe;
}

var host = Host.Create(
        c => c with { ReactorCount = reactors, ExtraPorts = tls ? [Tls.Port] : c.ExtraPorts },
        onReactorStart,
        connectionFactory)
    .Handler(Project.Create())
    .Compression(CompressedContent.Default());

host.Bind(IPAddress.Any, 8080);

await host.RunAsync();

