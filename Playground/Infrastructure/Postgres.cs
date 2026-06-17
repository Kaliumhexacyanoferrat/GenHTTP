using System.Buffers.Text;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

using GenHTTP.Engine.Ioxide;

using ioxide;
using ioxide.pg;

namespace genhttp.Infrastructure;

/// <summary>
/// Postgres wiring for the async-db / crud profiles. The pool is per-reactor and ring-native —
/// started in each reactor's OnStart (via the engine's onReactorStart hook) and resolved from a
/// handler with <see cref="Pool" />. Rows arrive as UTF-8 text fields and are mapped into the shared
/// model types, so GenHTTP's serializer renders the JSON rather than a hand-rolled writer.
/// </summary>
public static class Postgres
{
    public const string Columns = "id, name, category, price, quantity, active, tags, rating_score, rating_count";

    public static PgOptions? Options { get; private set; }

    public static bool Enabled => Options is not null;

    public static PgPool Pool => IoxideReactor.Current.GetService<PgPool>();

    public static void Configure(int reactors)
    {
        var url = Environment.GetEnvironmentVariable("DATABASE_URL");
        if (string.IsNullOrEmpty(url))
        {
            Options = null;
            return;
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
    }

    // onReactorStart: start the ring-native pool on this reactor.
    public static void Start(Reactor reactor) => PgPool.Start(reactor, Options!);

    // Map one Postgres row (UTF-8 text fields) into a model item.
    //   0 id, 1 name, 2 category, 3 price, 4 quantity, 5 active(t/f), 6 tags(jsonb), 7 rating_score, 8 rating_count
    public static ProcessedItem MapItem(PgRow row) => new()
    {
        Id = ParseInt(row.Field(0)),
        Name = Encoding.UTF8.GetString(row.Field(1)),
        Category = Encoding.UTF8.GetString(row.Field(2)),
        Price = ParseInt(row.Field(3)),
        Quantity = ParseInt(row.Field(4)),
        Active = row.Field(5).SequenceEqual("t"u8),
        Tags = JsonSerializer.Deserialize<List<string>>(row.Field(6)),
        Rating = new RatingInfo { Score = ParseInt(row.Field(7)), Count = ParseInt(row.Field(8)) },
    };

    public static int ParseInt(ReadOnlySpan<byte> s) => Utf8Parser.TryParse(s, out int v, out _) ? v : 0;

    public static long ParseLong(ReadOnlySpan<byte> s) => Utf8Parser.TryParse(s, out long v, out _) ? v : 0;

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
