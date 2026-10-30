using System.Diagnostics;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.ServerSentEvents;

namespace GenHTTP.Playground.Samples;

public static class ServerSentEventsSample
{

    public static IHandlerBuilder Create()
    {
        /*
         *
         * A live dashboard: the server pushes its current memory usage to
         * the browser every second, without the client polling for it.
         *
         * See https://genhttp.org/documentation/content/handlers/server-sent-events/
         *
         */

        var stats = EventSource.Create()
                               .Generator(SendStatsAsync);

        var client = Content.From(Resource.FromString(ClientPage).Type(ContentType.TextHtml));

        return Layout.Create()
                     .Index(client) // GET http://localhost:8080/sse/
                     .Add("stats", stats); // GET http://localhost:8080/sse/stats/
    }

    public record ServerStats(long WorkingSet, long ManagedMemory, int Collections);

    private static async ValueTask SendStatsAsync(IEventConnection connection)
    {
        var id = 0;

        while (connection.Connected)
        {
            using var process = Process.GetCurrentProcess();

            var stats = new ServerStats(process.WorkingSet64, GC.GetTotalMemory(false), GC.CollectionCount(0));

            // serialized to JSON, the browser listens for the "stats" event type
            await connection.DataAsync(stats, eventType: "stats", eventId: $"{id++}");

            await Task.Delay(1000);
        }
    }

    private const string ClientPage =
        """
        <!DOCTYPE html>
        <html lang="en">
        <head>
            <meta charset="utf-8">
            <title>Server Stats</title>
        </head>
        <body>
            <h1>Server Stats</h1>
            <dl>
                <dt>Working set</dt><dd id="workingSet">-</dd>
                <dt>Managed memory</dt><dd id="managedMemory">-</dd>
                <dt>Gen 0 collections</dt><dd id="collections">-</dd>
            </dl>
            <script>
                const megabytes = (bytes) => `${(bytes / 1024 / 1024).toFixed(1)} MB`;

                // reconnects automatically if the connection drops
                const events = new EventSource('stats/');

                events.addEventListener('stats', (event) => {
                    const stats = JSON.parse(event.data);

                    document.getElementById('workingSet').textContent = megabytes(stats.workingSet);
                    document.getElementById('managedMemory').textContent = megabytes(stats.managedMemory);
                    document.getElementById('collections').textContent = stats.collections;
                });
            </script>
        </body>
        </html>
        """;

}
