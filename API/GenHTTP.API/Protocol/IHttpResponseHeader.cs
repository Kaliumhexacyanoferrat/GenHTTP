using System;
using System.Text;

namespace GenHTTP.Api.Protocol
{
    
    public interface IHttpResponseHeader
    {

        /// <summary>
        /// Specify, whether the connection should get closed after this request or not.
        /// </summary>
        bool CloseConnection { get; }

        /// <summary>
        /// Retrieve or set the value of a header field.
        /// </summary>
        /// <param name="field">The name of the header field</param>
        /// <returns>The value of the header field</returns>
        string this[string field] { get; set; }

        /// <summary>
        /// The HTTP respnse code.
        /// </summary>
        ResponseType Type { get; set; }

        /// <summary>
        /// The type of the content.
        /// </summary>
        ContentType ContentType { get; set; }

        /// <summary>
        /// The charset used to encode the data.
        /// </summary>
        Encoding? ContentEncoding { get; set; }
        
        /// <summary>
        /// The version of the HTTP protocol.
        /// </summary>
        ProtocolType ProtocolType { get; }

        /// <summary>
        /// Define, when this resource will expire.
        /// </summary>
        DateTime? Expires { get; set; }
        
        /// <summary>
        /// Define, when this ressource has been changed the last time.
        /// </summary>
        DateTime? Modified { get; set; }

        /// <summary>
        /// Retrieve the status code of this response (Type).
        /// </summary>
        int Status { get; }
        
        /// <summary>
        /// Add a cookie to send to the client.
        /// </summary>
        /// <param name="cookie">The cookie to send</param>
        void AddCookie(HttpCookie cookie);
        
    }

}
