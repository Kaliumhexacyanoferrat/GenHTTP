using GenHTTP.Modules.Webservices;

namespace genhttp;

public sealed class AsyncDbService
{

    // GET /async-db?min=&max=&limit= -> items with price in [min,max], fetched from Postgres via
    // ioxide.pg's per-reactor pool and returned as a typed object GenHTTP serializes.
    [ResourceMethod]
    public async ValueTask<Db.DbResponse> Get(int min = 10, int max = 50, int limit = 50)
    {
        if (limit < 1)
        {
            limit = 1;
        }
        if (limit > 50)
        {
            limit = 50;
        }

        var items = new List<Db.DbItem>();

        // min/max/limit are int-typed (query-bound), so inline interpolation is injection-safe and
        // keeps a stable statement shape — matches the reference async-db endpoint.
        var sql = $"SELECT {Db.Columns} FROM items WHERE price BETWEEN {min} AND {max} LIMIT {limit}";

        await Db.Pool.QueryAsync(sql, default, row => items.Add(Db.MapRow(row)));

        return new Db.DbResponse(items.ToArray(), items.Count);
    }

}
