using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Security.Providers;

public class SnifferPreventionConcern : IConcern
{

    #region Get-/Setters

    public IHandler Content { get; }

    #endregion

    #region Initialization

    public SnifferPreventionConcern(IHandler content)
    {
        Content = content;
    }

    #endregion

    #region Functionality

    public async ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        var content = await Content.HandleAsync(request);

        if (content != null)
        {
            content.Headers["X-Content-Type-Options"] = "nosniff";
        }

        return content;
    }

    public ValueTask PrepareAsync() => Content.PrepareAsync();

    #endregion

}
