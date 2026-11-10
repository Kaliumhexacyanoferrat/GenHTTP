namespace GenHTTP.Modules.IO.Pooling;

internal static class ContentPool<T> where T : class
{
    private const int LocalCapacity = 16;

    [ThreadStatic]
    private static T? _fast;

    [ThreadStatic]
    private static LocalStack? _local;

    #region Functionality

    internal static T? TryRent()
    {
        var fast = _fast;

        if (fast != null)
        {
            _fast = null;
            return fast;
        }

        return TryRentFromStack();
    }

    internal static void Return(T item)
    {
        if (_fast == null)
        {
            _fast = item;
            return;
        }

        ReturnToStack(item);
    }

    private static T? TryRentFromStack()
    {
        var local = _local;

        if (local == null || local.Count == 0)
        {
            return null;
        }

        var item = local.Items[--local.Count];
        local.Items[local.Count] = null;

        return item;
    }

    private static void ReturnToStack(T item)
    {
        var local = _local ??= new LocalStack();

        if (local.Count < LocalCapacity)
        {
            local.Items[local.Count++] = item;
        }
    }

    #endregion

    private sealed class LocalStack
    {
        internal readonly T?[] Items = new T?[LocalCapacity];

        internal int Count;
    }

}
