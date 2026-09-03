using System.Collections;

using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Engine.Ioxide.Infrastructure.Endpoints;

internal sealed class EndPointCollection(IReadOnlyList<IEndPoint> endPoints) : IEndPointCollection
{
    public IEndPoint this[int index] => endPoints[index];

    public int Count => endPoints.Count;

    public IEnumerator<IEndPoint> GetEnumerator() => endPoints.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
