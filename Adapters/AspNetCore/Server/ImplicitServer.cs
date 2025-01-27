﻿using System.Runtime.InteropServices;

using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Adapters.AspNetCore.Server;

public sealed class ImplicitServer : IServer
{

    #region Get-/Setters

    public string Version => RuntimeInformation.FrameworkDescription;

    public bool Running { get; }

    public bool Development
    {
        get
        {
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            return string.Compare(env, "Development", StringComparison.OrdinalIgnoreCase) == 0;
        }
    }

    public IEndPointCollection EndPoints { get; }

    public IServerCompanion? Companion { get; }

    public IHandler Handler { get; }

    #endregion

    #region Initialization

    public ImplicitServer(IHandler handler, IServerCompanion? companion)
    {
        Handler = handler;
        Companion = companion;

        EndPoints = new EmptyEndpoints();

        Running = true;
    }

    #endregion

    #region Functionality

    public ValueTask DisposeAsync() => new();

    public ValueTask StartAsync() => throw new InvalidOperationException("Server is managed by ASP.NET Core and cannot be started");

    #endregion

}
