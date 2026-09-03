using System.Collections;

using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Engine.Ioxide.Infrastructure.Endpoints;

/// <summary>The bound endpoints, as the API exposes them.</summary>
internal sealed class EndPointCollection(IReadOnlyList<IEndPoint> endPoints) : IEndPointCollection
{
    public IEndPoint this[int index] => endPoints[index];

    public int Count => endPoints.Count;

    // Walks the bound endpoints in the order they were configured.
    public IEnumerator<IEndPoint> GetEnumerator() => endPoints.GetEnumerator();

    // The untyped overload, which defers to the typed one.
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
