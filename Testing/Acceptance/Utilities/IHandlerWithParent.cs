using GenHTTP.Api.Content;

namespace GenHTTP.Testing.Acceptance.Utilities;

public interface IHandlerWithParent : IHandler
{

    public new IHandler Parent { get; set; }

}
