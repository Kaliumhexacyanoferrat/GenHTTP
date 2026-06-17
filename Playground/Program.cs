using System.IO.Pipelines;
using System.Net;

using genhttp;
using genhttp.Infrastructure;

using GenHTTP.Engine.Ioxide;
using GenHTTP.Modules.Compression;

using ioxide;

// Reactor count follows the available CPUs (api-4 / api-16 control this via cpuset pinning);
// override with IOXIDE_REACTORS.
var reactors = int.TryParse(Environment.GetEnvironmentVariable("IOXIDE_REACTORS"), out var rc) ? rc : Environment.ProcessorCount;

// Postgres (async-db / crud) and TLS (json-tls on :8081) are both per-reactor; configure them and
// fold their per-reactor init into one OnStart.
Postgres.Configure(reactors);
var tls = Tls.Configure();

Action<Reactor>? onReactorStart = (!Postgres.Enabled && !tls) ? null : r =>
{
    if (Postgres.Enabled)
    {
        Postgres.Start(r);
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

// The engine buffers a whole response in one write slab; static assets can exceed the 16 KB default.
// Size the slab above the largest asset (plus GenHTTP's 64 KB file-copy buffer) — only when static is
// mounted, so the high-connection profiles keep the small per-connection buffer.
int? writeSlab = null;
var staticRoot = Environment.GetEnvironmentVariable("IOXIDE_STATIC") ?? "/data/static";
if (Directory.Exists(staticRoot))
{
    long largest = 0;
    foreach (var file in Directory.EnumerateFiles(staticRoot, "*", SearchOption.AllDirectories))
    {
        largest = Math.Max(largest, new FileInfo(file).Length);
    }
    writeSlab = (int)largest + 128 * 1024;
}

var host = Host.Create(
        c => c with { ReactorCount = reactors, ExtraPorts = tls ? [Tls.Port] : c.ExtraPorts, WriteSlabSize = writeSlab ?? c.WriteSlabSize },
        onReactorStart,
        connectionFactory)
    .Handler(Project.Create())
    .Compression(CompressedContent.Default());

host.Bind(IPAddress.Any, 8080);

await host.RunAsync();

