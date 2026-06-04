using System.Text.Json;

using GenHTTP.Modules.Webservices;

namespace GenHTTP.Testing.Acceptance.HttpArena;

public class AsyncDbService
{
    private static readonly List<DatasetItem> DatasetItems = LoadItems();

    private static List<DatasetItem> LoadItems()
    {
        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var datasetPath = Path.Combine(HttpArenaProject.DataPath, "dataset.json");

        if (File.Exists(datasetPath))
        {
            return JsonSerializer.Deserialize<List<DatasetItem>>(File.ReadAllText(datasetPath), jsonOptions) ?? [];
        }

        return [];
    }

    [ResourceMethod]
    public ListWithCount<object> Compute(int min = 10, int max = 50, int limit = 50)
    {
        var filtered = DatasetItems
            .Where(item => item.Price >= min && item.Price <= max)
            .Take(limit)
            .Select(item => (object)new
            {
                id = item.Id,
                name = item.Name,
                category = item.Category,
                price = item.Price,
                quantity = item.Quantity,
                active = item.Active,
                tags = item.Tags,
                rating = item.Rating == null ? null : new { score = item.Rating.Score, count = item.Rating.Count }
            })
            .ToList();

        return new ListWithCount<object>(filtered);
    }
}
