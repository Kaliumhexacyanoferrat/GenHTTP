using System.Text.Json;

namespace genhttp;

/// <summary>
/// Loads the shared benchmark dataset (the same /data/dataset.json the HttpArena entries use). The
/// json / json-comp profiles return these items (plus a per-request total) as typed objects that
/// GenHTTP's own serialization renders — exercising the real response pipeline, not a hand-rolled writer.
/// </summary>
public static class Data
{
    public sealed record Rating(int Score, int Count);

    public sealed record Item(int Id, string Name, string Category, long Price, long Quantity, bool Active, string[] Tags, Rating Rating);

    public static Item[] Items { get; private set; } = [];

    public static int Count => Items.Length;

    public static void LoadDataset(string path)
    {
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        Items = JsonSerializer.Deserialize<Item[]>(File.ReadAllBytes(path), options) ?? [];
    }
}
