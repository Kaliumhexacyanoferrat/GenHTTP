namespace genhttp;

public sealed class DatasetItem
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Category { get; set; } = "";
    public int Price { get; set; }
    public int Quantity { get; set; }
    public bool Active { get; set; }
    public List<string>? Tags { get; set; }
    public RatingInfo? Rating { get; set; }
}

public sealed class ProcessedItem
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Category { get; set; } = "";
    public int Price { get; set; }
    public int Quantity { get; set; }
    public bool Active { get; set; }
    public List<string>? Tags { get; set; }
    public RatingInfo? Rating { get; set; }
    public long Total { get; set; }
}

public sealed class RatingInfo
{
    public int Score { get; set; }
    public int Count { get; set; }
}

public sealed class ListWithCount<T>(List<T> items)
{

    public List<T> Items => items;

    public int Count => items.Count;

}


public sealed class CrudListResponse
{
    public List<ProcessedItem> Items { get; set; } = [];
    public long Total { get; set; }
    public int Page { get; set; }
    public int Limit { get; set; }
}

public sealed class CrudItem
{
    public int? Id { get; set; }
    public string? Name { get; set; }
    public string? Category { get; set; }
    public int Price { get; set; }
    public int Quantity { get; set; }
}
