using System.Text.Json;

using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Conversion.Serializers.Json;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Webservices;

using ioxide.pg;

using Microsoft.Extensions.Caching.Memory;

namespace genhttp;

// Mounted at /crud:
//   GET  /crud/items?category=&page=&limit=  -> paginated list
//   GET  /crud/items/{id}                    -> single item, cache-aside (X-Cache: MISS|HIT, 1s TTL)
//   POST /crud/items                         -> upsert (201)
//   PUT  /crud/items/{id}                    -> update (200), invalidates the cache entry
// JSON bodies are produced by GenHTTP's serializer (DTO return values, or JsonContent for the
// cached single-item read where a response header is needed).
public sealed class CrudService
{
    private const string SelectGet = "SELECT " + Db.Columns + " FROM items WHERE id = $1";

    private const string SelectList = "SELECT " + Db.Columns + " FROM items WHERE category = $1 ORDER BY id LIMIT $2 OFFSET $3";

    private const string Upsert = "INSERT INTO items (id, name, category, price, quantity, active, tags, rating_score, rating_count) " +
                                  "VALUES ($1, $2, $3, $4, $5, true, '[]'::jsonb, 0, 0) " +
                                  "ON CONFLICT (id) DO UPDATE SET name = $2, category = $3, price = $4, quantity = $5";

    private const string UpdateSql = "UPDATE items SET name = $2, category = $3, price = $4, quantity = $5 WHERE id = $1";

    // Crud list envelope: total == count == rows on this page (load-more semantics, no count(*)),
    // with the requested page echoed back. Field order matches the reference: items, total, page, count.
    public sealed record ListResponse(Db.DbItem[] Items, int Total, int Page, int Count);

    [ResourceMethod("items")]
    public async ValueTask<ListResponse> ListItems(string? category = null, int page = 1, int limit = 10)
    {
        if (page < 1)
        {
            page = 1;
        }
        if (limit < 1)
        {
            limit = 1;
        }

        var items = new List<Db.DbItem>();

        PgParam[] args = [PgParam.Text(category ?? ""), PgParam.Int(limit), PgParam.Int((long)(page - 1) * limit)];
        await Db.Pool.QueryAsync(SelectList, args, row => items.Add(Db.MapRow(row)));

        return new ListResponse(items.ToArray(), items.Count, page, items.Count);
    }

    [ResourceMethod("items/:id")]
    public async ValueTask<IResponse?> GetItem(int id, IRequest request)
    {
        var key = CacheKey(id);

        if (Db.Cache.TryGetValue(key, out byte[]? cached) && cached is not null)
        {
            return request.Respond().Content(cached, ContentType.ApplicationJson).Header("X-Cache", "HIT").Build();
        }

        Db.DbItem? item = null;

        PgParam[] args = [PgParam.Int(id)];
        await Db.Pool.QueryAsync(SelectGet, args, row => item = Db.MapRow(row));

        if (item is null)
        {
            return request.Respond().Status(ResponseStatus.NotFound).Build();
        }

        // Serialize once with GenHTTP's JSON options and cache the bytes, so a HIT skips both the
        // DB round-trip and the re-serialization.
        var bytes = JsonSerializer.SerializeToUtf8Bytes(item, JsonFormat.GetDefaultOptions());
        Db.Cache.Set(key, bytes, new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(1) });

        return request.Respond().Content(bytes, ContentType.ApplicationJson).Header("X-Cache", "MISS").Build();
    }

    [ResourceMethod(Method.Post, "items")]
    public async ValueTask<IResponse?> CreateItem(IRequest request)
    {
        var item = await ReadItem(request);

        PgParam[] args = [PgParam.Int(item.Id), PgParam.Text(item.Name), PgParam.Text(item.Category), PgParam.Int(item.Price), PgParam.Int(item.Quantity)];
        await Db.Pool.QueryAsync(Upsert, args);

        Db.Cache.Remove(CacheKey(item.Id));
        return request.Respond().Status(ResponseStatus.Created).Build();
    }

    [ResourceMethod(Method.Put, "items/:id")]
    public async ValueTask<IResponse?> UpdateItem(int id, IRequest request)
    {
        var item = await ReadItem(request);

        PgParam[] args = [PgParam.Int(id), PgParam.Text(item.Name), PgParam.Text(item.Category), PgParam.Int(item.Price), PgParam.Int(item.Quantity)];
        await Db.Pool.QueryAsync(UpdateSql, args);

        Db.Cache.Remove(CacheKey(id));
        return request.Respond().Status(ResponseStatus.Ok).Build();
    }

    private static string CacheKey(int id) => "item:" + id;

    private static async ValueTask<CrudItem> ReadItem(IRequest request)
    {
        var body = request.GetBody();
        if (body is null)
        {
            return default;
        }

        var data = await body.AsMemoryAsync();
        using var doc = JsonDocument.Parse(data);
        var root = doc.RootElement;

        return new CrudItem(GetInt(root, "id"), GetString(root, "name"), GetString(root, "category"), GetInt(root, "price"), GetInt(root, "quantity"));
    }

    private static int GetInt(JsonElement e, string name)
        => e.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.Number ? v.GetInt32() : 0;

    private static string GetString(JsonElement e, string name)
        => e.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() ?? "" : "";

    private readonly record struct CrudItem(int Id, string Name, string Category, long Price, long Quantity);
}
