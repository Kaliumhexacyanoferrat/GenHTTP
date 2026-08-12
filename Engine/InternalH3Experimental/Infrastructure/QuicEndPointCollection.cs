using System.Collections;

using GenHTTP.Api.Infrastructure;

using GenHTTP.Engine.Shared.Infrastructure;

namespace GenHTTP.Engine.InternalH3Experimental.Infrastructure;

internal sealed class QuicEndPointCollection : IEndPointCollection, IDisposable
{
    private readonly List<QuicEndPoint> _endPoints;

    internal QuicEndPointCollection(IServer server, IEnumerable<EndPointConfiguration> configuration)
    {
        _endPoints = configuration.Select(c => new QuicEndPoint(server, c)).ToList();
    }

    internal async ValueTask StartAsync()
    {
        foreach (QuicEndPoint endPoint in _endPoints)
        {
            await endPoint.StartAsync();
        }
    }

    public IEndPoint this[int index] => _endPoints[index];

    public int Count => _endPoints.Count;

    public IEnumerator<IEndPoint> GetEnumerator() => _endPoints.Cast<IEndPoint>().GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void Dispose()
    {
        foreach (QuicEndPoint endPoint in _endPoints)
        {
            endPoint.Dispose();
        }
    }
}
