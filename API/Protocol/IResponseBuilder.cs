using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Api.Protocol
{

    /// <summary>
    /// Allows to configure a HTTP response to be send.
    /// </summary>
    public interface IResponseBuilder : IBuilder<IResponse>, IResponseModification<IResponseBuilder>
    {

        /// <summary>
        /// The request the response belongs to.
        /// </summary>
        IRequest Request { get; }

        /// <summary>
        /// Specifies the content to be sent to the client.
        /// </summary>
        /// <param name="content">The content to be send to the client</param>
        IResponseBuilder Content(IResponseContent content);

        /// <summary>
        /// Specifies the length of the content stream, if known.
        /// </summary>
        /// <param name="length">The length of the content stream</param>
        IResponseBuilder Length(ulong length);

    }

}
