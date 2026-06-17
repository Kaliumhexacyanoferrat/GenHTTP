using GenHTTP.Modules.Webservices;

namespace genhttp;

public sealed class JsonService
{
    // The response shape (camelCase, declaration order) is produced by GenHTTP's JSON serializer.
    public sealed record JsonItem(int Id, string Name, string Category, long Price, long Quantity, bool Active, string[] Tags, Data.Rating Rating, long Total);

    public sealed record JsonResponse(JsonItem[] Items, int Count);

    // GET /json/{count}?m=N -> dataset items with per-item total = price*quantity*m, returned as a
    // typed object that GenHTTP serializes (Conversion module, camelCase). json-comp is the same
    // endpoint with Accept-Encoding: br; the host compression concern handles the encoding.
    [ResourceMethod(":count")]
    public JsonResponse Get(int count, int m = 1)
    {
        if (count < 1)
        {
            count = 1;
        }
        if (count > Data.Count)
        {
            count = Data.Count;
        }

        var items = new JsonItem[count];
        for (var i = 0; i < count; i++)
        {
            var it = Data.Items[i];
            items[i] = new JsonItem(it.Id, it.Name, it.Category, it.Price, it.Quantity, it.Active, it.Tags, it.Rating, it.Price * it.Quantity * m);
        }

        return new JsonResponse(items, count);
    }
}
