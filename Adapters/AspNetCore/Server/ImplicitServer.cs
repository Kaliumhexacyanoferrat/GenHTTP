using System.Runtime.InteropServices;

using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;

using GenHTTP.Engine.Shared.Types;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GenHTTP.Adapters.AspNetCore.Server;

/// <summary>
/// Stands in for a GenHTTP <see cref="IServer"/> when handlers are embedded into an existing
/// ASP.NET Core application - the server lifecycle here is owned by that host, not by GenHTTP.
/// </summary>
internal sealed class ImplicitServer : IServer
{

    #region Get-/Setters

    public string Version => RuntimeInformation.FrameworkDescription;

    public bool Running { get; } = true;

    public bool Development { get; }

    public IPropertyBag Properties { get; } = new PropertyBag();

    public ILoggerFactory Logging { get; }

    public IEndPointCollection EndPoints { get; }

    public IHandler Handler { get; }

    #endregion

    #region Initialization

    public ImplicitServer(HttpContext context, IHandler handler)
    {
        Handler = handler;

        EndPoints = new EndpointCollection(context);

        Logging = context.RequestServices.GetRequiredService<ILoggerFactory>();

        Development = context.RequestServices.GetService<IHostEnvironment>()?.IsDevelopment() ?? false;
    }

    #endregion

    #region Functionality

    public ValueTask StartAsync() => throw new InvalidOperationException("Server is managed by ASP.NET Core and cannot be started");

    public ValueTask DisposeAsync() => default;

    #endregion

}
