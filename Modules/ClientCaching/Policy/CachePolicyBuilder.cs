using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.ClientCaching.Policy;

public sealed class CachePolicyBuilder : IConcernBuilder
{
    private TimeSpan? _Duration;

    private Func<IRequest, IResponse, bool>? _Predicate;

    #region Functionality

    /// <summary>
    /// Instructs the client to cache the content generated
    /// by the server for the given duration.
    /// </summary>
    /// <param name="duration">The duration the content should be cached on the client</param>
    public CachePolicyBuilder Duration(TimeSpan duration)
    {
        _Duration = duration;
        return this;
    }

    /// <summary>
    /// Instructs the client to cache the content generated
    /// by the server for the given number of days.
    /// </summary>
    /// <param name="duration">The number of days the content should be cached on the client</param>
    public CachePolicyBuilder Duration(int days) => Duration(TimeSpan.FromDays(days));

    /// <summary>
    /// Allows to filter the responses which should be cached
    /// by the client.
    /// </summary>
    /// <param name="predicate">The predicate to be evaluated to check, whether content should be cached</param>
    public CachePolicyBuilder Predicate(Func<IRequest, IResponse, bool> predicate)
    {
        _Predicate = predicate;
        return this;
    }

    public IConcern Build(IHandler content)
    {
        var duration = _Duration ?? throw new BuilderMissingPropertyException("Duration");

        return new CachePolicyConcern(content, duration, _Predicate);
    }

    #endregion

}
