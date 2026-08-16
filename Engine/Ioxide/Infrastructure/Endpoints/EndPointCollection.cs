using System.Collections;

using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Engine.Ioxide.Infrastructure.Endpoints;

/// <summary>
/// The endpoints a server is listening on, as <see cref="IServer.EndPoints"/> reports them.
/// </summary>
internal sealed class EndPointCollection(IReadOnlyList<IEndPoint> endPoints) : IEndPointCollection
{
    public IEndPoint this[int index] => endPoints[index];

    public int Count => endPoints.Count;

    public IEnumerator<IEndPoint> GetEnumerator() => endPoints.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
