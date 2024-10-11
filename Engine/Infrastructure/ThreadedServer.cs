﻿using System.Reflection;
using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Engine.Infrastructure.Endpoints;

namespace GenHTTP.Engine.Infrastructure;

internal sealed class ThreadedServer : IServer
{
    private readonly EndPointCollection _EndPoints;

    #region Get-/Setters

    public string Version { get; }

    public bool Running => !disposed;

    public bool Development => Configuration.DevelopmentMode;

    public IHandler Handler { get; }

    public IServerCompanion? Companion { get; }

    public IEndPointCollection EndPoints => _EndPoints;

    internal ServerConfiguration Configuration { get; }

    #endregion

    #region Constructors

    internal ThreadedServer(IServerCompanion? companion, ServerConfiguration configuration, IHandler handler)
    {
        Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "(n/a)";

        Companion = companion;
        Configuration = configuration;

        Handler = handler;

        Task.Run(() => PrepareHandlerAsync(handler, companion));

        _EndPoints = new EndPointCollection(this, configuration.EndPoints, configuration.Network);
    }

    private static async ValueTask PrepareHandlerAsync(IHandler handler, IServerCompanion? companion)
    {
        try
        {
            await handler.PrepareAsync();
        }
        catch (Exception e)
        {
            companion?.OnServerError(ServerErrorScope.General, null, e);
        }
    }

    #endregion

    #region IDisposable Support

    private bool disposed;

    public void Dispose()
    {
        if (!disposed)
        {
            _EndPoints.Dispose();

            disposed = true;
        }

        GC.SuppressFinalize(this);
    }

    #endregion

}
