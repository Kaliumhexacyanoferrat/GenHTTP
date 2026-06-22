using System.Collections;
using System.Net;

using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Engine.Ioxide.Hosting;

public sealed class IoxideEndPoint(IPAddress? address, ushort port, bool dualStack, bool secure) : IEndPoint
{
    public IPAddress? Address => address;

    public ushort Port => port;

    public bool DualStack => dualStack;

    public bool Secure => secure;

    public void Dispose() { }
}

internal sealed class IoxideEndPoints(IReadOnlyList<IEndPoint> eps) : IEndPointCollection
{
    public IEndPoint this[int i] => eps[i];

    public int Count => eps.Count;

    public IEnumerator<IEndPoint> GetEnumerator() => eps.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
