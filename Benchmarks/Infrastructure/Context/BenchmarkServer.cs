using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Benchmarks.Infrastructure.Context;

public class BenchmarkServer : IServer
{

    #region Get-/Setters

    public string Version { get; set; }

    public bool Running { get; set; }

    public bool Development { get; set; }

    public IEndPointCollection EndPoints { get; }

    public IServerCompanion? Companion { get; set; }

    public IHandler Handler => throw new NotSupportedException("Should not be required by handlers");

    #endregion

    #region Initialization

    public BenchmarkServer()
    {
        Running = true;
        Development = false;
        Version = "1.0.0.0";

        Companion = null;

        EndPoints = new EndPointCollection()
        {
            new EndPoint()
        };
    }

    #endregion

    #region Functionality

    public ValueTask StartAsync() => throw new NotSupportedException();

    public ValueTask DisposeAsync() => throw new NotSupportedException();

    #endregion

}
