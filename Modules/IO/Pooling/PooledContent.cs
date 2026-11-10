namespace GenHTTP.Modules.IO.Pooling;

/// <summary>
/// Base class for response content that is rented from a pool per response
/// and returned to the pool after the server has written it.
/// </summary>
/// <remarks>
/// <para>
/// The server disposes the content of a response after writing it, which returns
/// a rented instance to the pool. Responses that are never written (e.g. HEAD requests
/// or dropped connections) are not disposed, so their content is collected as usual.
/// </para>
/// <para>
/// Only instances obtained via <see cref="FromPool" /> are pooled. Instances created
/// with a constructor are regular objects and disposing them has no effect. A rented
/// instance must be passed to exactly one response and must not be referenced
/// after that response has been written. Disposing a rented instance more than once
/// returns it to the pool only once.
/// </para>
/// </remarks>
/// <typeparam name="TSelf">The type of the content deriving from this class</typeparam>
/// <example>
/// <code>
/// public sealed class CounterContent : PooledContent&lt;CounterContent&gt;, IResponseContent
/// {
///     private int _value;
///
///     public static CounterContent Create(int value)
///     {
///         var content = FromPool(static () => new CounterContent());
///         content._value = value;
///         return content;
///     }
///
///     protected override void Reset() => _value = 0;
///
///     // IResponseContent members ...
/// }
/// </code>
/// </example>
public abstract class PooledContent<TSelf> : IDisposable where TSelf : PooledContent<TSelf>
{
    private bool _rented;

    #region Functionality

    /// <summary>
    /// Fetches an instance from the pool or creates a new one, if the pool is empty.
    /// </summary>
    /// <param name="factory">Creates a new, empty instance (should be a static lambda so it is not allocated per call)</param>
    /// <returns>An empty instance that will be returned to the pool on dispose</returns>
    protected static TSelf FromPool(Func<TSelf> factory)
    {
        var content = ContentPool<TSelf>.TryRent() ?? factory();

        content._rented = true;

        return content;
    }

    /// <summary>
    /// Clears the state of this instance before it is returned to the pool,
    /// so that no references to request data are kept alive.
    /// </summary>
    protected abstract void Reset();

    #endregion

    #region IDisposable Support

    /// <summary>
    /// Returns a rented instance to the pool. Has no effect on instances that
    /// have not been rented or have already been returned.
    /// </summary>
    public void Dispose()
    {
        if (_rented)
        {
            _rented = false;

            Reset();

            ContentPool<TSelf>.Return((TSelf)this);
        }
    }

    #endregion

}
