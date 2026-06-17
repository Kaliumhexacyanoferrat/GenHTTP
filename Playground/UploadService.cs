using System.Buffers;

using GenHTTP.Api.Protocol;

using GenHTTP.Modules.IO;
using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Webservices;

namespace genhttp;

public sealed class UploadService
{

    // POST /upload -> streams the request body to its end and returns the byte count as text.
    // The body is never buffered whole; it is read in chunks so memory stays bounded regardless
    // of upload size (the upload profile sends 500KB..20MB bodies).
    [ResourceMethod(Method.Post)]
    public async ValueTask<IResponse?> Post(IRequest request)
    {
        long total = 0;

        var body = request.GetBody();

        if (body is not null)
        {
            var stream = body.AsStream();
            var buffer = ArrayPool<byte>.Shared.Rent(65536);

            try
            {
                int read;
                while ((read = await stream.ReadAsync(buffer)) > 0)
                {
                    total += read;
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        return request.Respond()
                      .Content(total.ToString(), ContentType.TextPlain)
                      .Build();
    }

}
