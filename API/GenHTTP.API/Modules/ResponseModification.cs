using GenHTTP.Api.Protocol;

namespace GenHTTP.Api.Modules
{

    /// <summary>
    /// A method that will apply some modification to the given response.
    /// </summary>
    /// <param name="response">The response to be modified</param>
    public delegate void ResponseModification(IResponseBuilder response);

}
