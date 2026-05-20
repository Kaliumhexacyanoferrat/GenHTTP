using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Security.Providers;

public class SnifferPreventionConcern : IConcern
{
    private static readonly ReadOnlyMemory<byte> ContentTypeOptions = "X-Content-Type-Options"u8.ToArray();

    private static readonly ReadOnlyMemory<byte> NoSniffValue = "nosniff"u8.ToArray();

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
            content.Rebuild().Header(ContentTypeOptions, NoSniffValue);
        }

        return content;
    }

    public ValueTask PrepareAsync() => Content.PrepareAsync();

    #endregion

}
