using System.Text.Json;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using genhttp.Infrastructure;

using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Webservices;

using ioxide.pg;

using Microsoft.Extensions.Caching.Memory;
using StringContent = GenHTTP.Modules.IO.Strings.StringContent;

namespace genhttp.Tests;

// Mounted at /crud/items (Crud sublayout):
//   GET  /crud/items?category=&page=&limit=  -> paginated list (load-more semantics)
//   GET  /crud/items/{id}                    -> single item, cache-aside (X-Cache: MISS|HIT, 200ms TTL)
//   POST /crud/items                         -> upsert (201)
//   PUT  /crud/items/{id}                    -> update (200), invalidates the cache entry
public class Crud
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private static readonly IMemoryCache ItemCache = new MemoryCache(new MemoryCacheOptions());

    private static readonly MemoryCacheEntryOptions ItemCacheOptions = new() { AbsoluteExpirationRelativeToNow = TimeSpan.FromMilliseconds(200) };

    [ResourceMethod]
    public async Task<CrudListResponse> List(string category = "electronics", int page = 1, int limit = 10)
    {
        if (page < 1) page = 1;
        if (limit is < 1 or > 50) limit = 10;

        var offset = (page - 1) * limit;

        var items = new List<ProcessedItem>();

        PgParam[] args = [PgParam.Text(category), PgParam.Int(limit), PgParam.Int(offset)];
        await Postgres.Pool.QueryAsync(
            "SELECT " + Postgres.Columns + " FROM items WHERE category = $1 ORDER BY id LIMIT $2 OFFSET $3",
            args,
            row => items.Add(Postgres.MapItem(row)));

        return new CrudListResponse
        {
            Items = items,
            Total = items.Count,
            Page = page,
            Limit = limit
        };
    }

    [ResourceMethod(":id")]
    public async ValueTask<IResponse> Get(int id, IRequest request)
    {
        if (ItemCache.TryGetValue(id, out string? cached) && cached is not null)
        {
            return request.Respond()
                          .Content(new StringContent(cached, ContentType.ApplicationJson))
                          .Header("X-Cache", "HIT")
                          .Build();
        }

        var item = await FetchItemByIdAsync(id);

        if (item is null)
        {
            throw new ProviderException(ResponseStatus.NotFound, $"Item with ID {id} does not exist");
        }

        // Serialize once and cache the string, so a HIT skips both the DB round-trip and re-serialization.
        var json = JsonSerializer.Serialize(item, JsonOptions);

        ItemCache.Set(id, json, ItemCacheOptions);

        return request.Respond()
                      .Content(new StringContent(json, ContentType.ApplicationJson))
                      .Header("X-Cache", "MISS")
                      .Build();
    }

    [ResourceMethod(Method.Post)]
    public async Task<Result<CrudItem>> Create(CrudItem item)
    {
        PgParam[] args =
        [
            PgParam.Int(item.Id ?? 0),
            PgParam.Text(item.Name ?? "New Product"),
            PgParam.Text(item.Category ?? "test"),
            PgParam.Int(item.Price),
            PgParam.Int(item.Quantity)
        ];

        await Postgres.Pool.QueryAsync(
            "INSERT INTO items (id, name, category, price, quantity, active, tags, rating_score, rating_count) " +
            "VALUES ($1, $2, $3, $4, $5, true, '[\"bench\"]', 0, 0) " +
            "ON CONFLICT (id) DO UPDATE SET name = $2, price = $4, quantity = $5",
            args);

        ItemCache.Remove(item.Id ?? 0);

        return new Result<CrudItem>(item).Status(ResponseStatus.Created);
    }

    [ResourceMethod(Method.Put, ":id")]
    public async Task<CrudItem> Update(int id, CrudItem item)
    {
        PgParam[] args =
        [
            PgParam.Text(item.Name ?? "Updated"),
            PgParam.Int(item.Price),
            PgParam.Int(item.Quantity),
            PgParam.Int(id)
        ];

        await Postgres.Pool.QueryAsync(
            "UPDATE items SET name = $1, price = $2, quantity = $3 WHERE id = $4",
            args);

        ItemCache.Remove(id);

        return item;
    }

    private static async Task<ProcessedItem?> FetchItemByIdAsync(int id)
    {
        ProcessedItem? item = null;

        PgParam[] args = [PgParam.Int(id)];
        await Postgres.Pool.QueryAsync(
            "SELECT " + Postgres.Columns + " FROM items WHERE id = $1 LIMIT 1",
            args,
            row => item = Postgres.MapItem(row));

        return item;
    }

}
