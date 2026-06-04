using System.Collections.Concurrent;
using System.Text.Json;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Webservices;

using StringContent = GenHTTP.Modules.IO.Strings.StringContent;

namespace GenHTTP.Testing.Acceptance.HttpArena;

public class CrudService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private static readonly ConcurrentDictionary<int, ProcessedItem> Store = LoadStore();

    private static int _nextId = 100001;

    private static readonly ConcurrentDictionary<int, string> Cache = new();

    private static ConcurrentDictionary<int, ProcessedItem> LoadStore()
    {
        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var datasetPath = Path.Combine(HttpArenaProject.DataPath, "dataset.json");
        var dict = new ConcurrentDictionary<int, ProcessedItem>();

        if (File.Exists(datasetPath))
        {
            var items = JsonSerializer.Deserialize<List<DatasetItem>>(File.ReadAllText(datasetPath), jsonOptions) ?? [];

            foreach (var item in items)
            {
                dict[item.Id] = new ProcessedItem
                {
                    Id = item.Id,
                    Name = item.Name,
                    Category = item.Category,
                    Price = item.Price,
                    Quantity = item.Quantity,
                    Active = item.Active,
                    Tags = item.Tags,
                    Rating = item.Rating,
                    Total = (long)item.Price * item.Quantity
                };
            }
        }

        return dict;
    }

    [ResourceMethod]
    public CrudListResponse List(string category = "electronics", int page = 1, int limit = 10)
    {
        if (page < 1) page = 1;
        if (limit is < 1 or > 50) limit = 10;

        var offset = (page - 1) * limit;

        var items = Store.Values
            .Where(i => i.Category == category)
            .OrderBy(i => i.Id)
            .Skip(offset)
            .Take(limit)
            .ToList();

        return new CrudListResponse
        {
            Items = items,
            Total = items.Count,
            Page = page,
            Limit = limit
        };
    }

    [ResourceMethod(":id")]
    public IResponse Get(int id, IRequest request)
    {
        if (Cache.TryGetValue(id, out var cached))
        {
            return request.Respond()
                          .Content(new StringContent(cached, ContentType.ApplicationJson))
                          .Header("X-Cache", "HIT")
                          .Build();
        }

        if (!Store.TryGetValue(id, out var item))
        {
            throw new ProviderException(ResponseStatus.NotFound, $"Item with ID {id} does not exist");
        }

        var json = JsonSerializer.Serialize(item, JsonOptions);

        Cache[id] = json;

        return request.Respond()
                      .Content(new StringContent(json, ContentType.ApplicationJson))
                      .Header("X-Cache", "MISS")
                      .Build();
    }

    [ResourceMethod(Method.Post)]
    public Result<CrudItem> Create(CrudItem item)
    {
        var id = item.Id ?? Interlocked.Increment(ref _nextId);

        var stored = new ProcessedItem
        {
            Id = id,
            Name = item.Name ?? "New Product",
            Category = item.Category ?? "test",
            Price = item.Price,
            Quantity = item.Quantity,
            Active = true,
            Tags = ["bench"],
            Rating = new RatingInfo { Score = 0, Count = 0 },
            Total = (long)item.Price * item.Quantity
        };

        Store[id] = stored;
        Cache.TryRemove(id, out _);

        item.Id = id;

        return new Result<CrudItem>(item).Status(ResponseStatus.Created);
    }

    [ResourceMethod(Method.Put, ":id")]
    public CrudItem Update(int id, CrudItem item)
    {
        if (!Store.TryGetValue(id, out var existing))
        {
            throw new ProviderException(ResponseStatus.NotFound, $"Item with ID {id} does not exist");
        }

        existing.Name = item.Name ?? "Updated";
        existing.Price = item.Price;
        existing.Quantity = item.Quantity;
        existing.Total = (long)item.Price * item.Quantity;

        Store[id] = existing;
        Cache.TryRemove(id, out _);

        return item;
    }
}
