using System.Text.Json;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Webservices;

namespace GenHTTP.Testing.Acceptance.HttpArena;

public class JsonService
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

    [ResourceMethod(":count")]
    public ListWithCount<ProcessedItem> Compute(int count, int m = 1)
    {
        if (DatasetItems.Count == 0)
        {
            throw new ProviderException(ResponseStatus.InternalServerError, "No dataset");
        }

        if (count > DatasetItems.Count) count = DatasetItems.Count;
        if (count < 0) count = 0;

        var processed = new List<ProcessedItem>(count);

        for (var i = 0; i < count; i++)
        {
            var d = DatasetItems[i];

            processed.Add(new ProcessedItem
            {
                Id = d.Id, Name = d.Name, Category = d.Category,
                Price = d.Price, Quantity = d.Quantity, Active = d.Active,
                Tags = d.Tags, Rating = d.Rating,
                Total = d.Price * d.Quantity * m
            });
        }

        return new(processed);
    }
}
