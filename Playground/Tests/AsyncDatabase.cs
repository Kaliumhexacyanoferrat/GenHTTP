using System.Text;
using System.Text.Json;

using genhttp.Infrastructure;

using GenHTTP.Modules.Webservices;

namespace genhttp.Tests;

public class AsyncDatabase
{

    // GET /async-db?min=&max=&limit= -> items with price in [min,max], fetched from Postgres via
    // ioxide.pg's per-reactor pool and returned as objects GenHTTP serializes ({ items, count }).
    [ResourceMethod]
    public async Task<ListWithCount<object>> Compute(int min = 10, int max = 50, int limit = 50)
    {
        var items = new List<object>(limit);

        // min/max/limit are int-typed (query-bound), so inline interpolation is injection-safe and
        // keeps a stable statement shape — matches the reference async-db endpoint.
        var sql = $"SELECT {Postgres.Columns} FROM items WHERE price BETWEEN {min} AND {max} LIMIT {limit}";

        await Postgres.Pool.QueryAsync(sql, default, row => items.Add(new
        {
            id = Postgres.ParseInt(row.Field(0)),
            name = Encoding.UTF8.GetString(row.Field(1)),
            category = Encoding.UTF8.GetString(row.Field(2)),
            price = Postgres.ParseInt(row.Field(3)),
            quantity = Postgres.ParseInt(row.Field(4)),
            active = row.Field(5).SequenceEqual("t"u8),
            tags = JsonSerializer.Deserialize<List<string>>(row.Field(6)),
            rating = new { score = Postgres.ParseInt(row.Field(7)), count = Postgres.ParseInt(row.Field(8)) },
        }));

        return new ListWithCount<object>(items);
    }

}
