using System.Buffers.Text;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

using GenHTTP.Engine.Ioxide;

using ioxide.pg;

using Microsoft.Extensions.Caching.Memory;

namespace genhttp;

/// <summary>
/// Postgres wiring for the async-db / crud profiles. The pool is per-reactor and ring-native —
/// started in each reactor's OnStart (via the engine's onReactorStart hook) and resolved from a
/// handler with <see cref="Pool" />. Rows are mapped into typed objects (<see cref="DbItem" />) so
/// GenHTTP's serializer renders the JSON, rather than hand-writing bytes.
/// </summary>
public static class Db
{
    public const string Columns = "id, name, category, price, quantity, active, tags, rating_score, rating_count";

    public sealed record DbItem(int Id, string Name, string Category, long Price, long Quantity, bool Active, string[] Tags, Data.Rating Rating);

    public sealed record DbResponse(DbItem[] Items, int Count);

    public static PgOptions? Options { get; private set; }

    public static bool Enabled => Options is not null;

    // Single shared in-process cache (crud cache-aside, CRUD_CACHE=inproc semantics).
    public static readonly IMemoryCache Cache = new MemoryCache(new MemoryCacheOptions());

    public static PgPool Pool => IoxideReactor.Current.GetService<PgPool>();

    public static PgOptions? Configure(int reactors)
    {
        var url = Environment.GetEnvironmentVariable("DATABASE_URL");
        if (string.IsNullOrEmpty(url))
        {
            Options = null;
            return null;
        }

        var uri = new Uri(url);
        var userInfo = uri.UserInfo.Split(':', 2);
        var maxConn = int.TryParse(Environment.GetEnvironmentVariable("DATABASE_MAX_CONN"), out var mc) ? mc : 256;

        Options = new PgOptions
        {
            Host = ResolveIPv4(uri.Host),
            Port = (ushort)(uri.Port > 0 ? uri.Port : 5432),
            User = userInfo[0],
            Password = userInfo.Length > 1 ? userInfo[1] : null,
            Database = uri.AbsolutePath.TrimStart('/'),
            PoolSize = Math.Clamp(maxConn / Math.Max(reactors, 1), 1, 8),
        };

        return Options;
    }

    // Map one Postgres row (UTF-8 text fields) into a typed item.
    //   0 id, 1 name, 2 category, 3 price, 4 quantity, 5 active(t/f), 6 tags(jsonb), 7 rating_score, 8 rating_count
    public static DbItem MapRow(PgRow row) => new(
        ParseInt(row.Field(0)),
        Encoding.UTF8.GetString(row.Field(1)),
        Encoding.UTF8.GetString(row.Field(2)),
        ParseLong(row.Field(3)),
        ParseLong(row.Field(4)),
        row.Field(5).SequenceEqual("t"u8),
        JsonSerializer.Deserialize<string[]>(row.Field(6)) ?? [],
        new Data.Rating(ParseInt(row.Field(7)), ParseInt(row.Field(8))));

    private static int ParseInt(ReadOnlySpan<byte> s) => Utf8Parser.TryParse(s, out int v, out _) ? v : 0;

    private static long ParseLong(ReadOnlySpan<byte> s) => Utf8Parser.TryParse(s, out long v, out _) ? v : 0;

    private static string ResolveIPv4(string host)
    {
        if (IPAddress.TryParse(host, out _))
        {
            return host;
        }

        foreach (var addr in Dns.GetHostAddresses(host))
        {
            if (addr.AddressFamily == AddressFamily.InterNetwork)
            {
                return addr.ToString();
            }
        }

        return host;
    }
}
