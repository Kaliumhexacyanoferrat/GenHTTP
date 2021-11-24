using System;
using System.IO;
using System.Threading.Tasks;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Basics;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Conversion.Providers;

namespace GenHTTP.Modules.Reflection
{

    /// <summary>
    /// Converts the result fetched from an invocation using reflection
    /// into a HTTP response.
    /// </summary>
    public class ResponseProvider
    {

        #region Get-/Setters

        private SerializationRegistry Serialization { get; }

        #endregion

        #region Initialization

        public ResponseProvider(SerializationRegistry serialization)
        {
            Serialization = serialization;
        }

        #endregion

        #region Functionality

        public async ValueTask<IResponse?> GetResponse(IRequest request, IHandler _, object? result)
        {
            // no result = 204
            if (result is null)
            {
                return request.Respond()
                              .Status(ResponseStatus.NoContent)
                              .Build();
            }

            var type = result.GetType();

            // response returned by the method
            if (result is IResponseBuilder responseBuilder)
            {
                return responseBuilder.Build();
            }

            if (result is IResponse response)
            {
                return response;
            }

            // stream returned as a download
            if (result is Stream download)
            {
                var downloadResponse = request.Respond()
                                              .Content(download, () => download.CalculateChecksumAsync())
                                              .Type(ContentType.ApplicationForceDownload)
                                              .Build();

                return downloadResponse;
            }

            // basic types should produce a string value
            if (type.IsPrimitive || type == typeof(string) || type.IsEnum || type == typeof(Guid))
            {
                return request.Respond()
                              .Content(result.ToString() ?? string.Empty)
                              .Type(ContentType.TextPlain)
                              .Build();
            }

            // serialize the result
            var serializer = Serialization.GetSerialization(request);

            if (serializer is null)
            {
                throw new ProviderException(ResponseStatus.UnsupportedMediaType, "Requested format is not supported");
            }

            var serializedResult = await serializer.SerializeAsync(request, result);

            return serializedResult.Build();
        }

        #endregion

    }

}
