using System;

using GenHTTP.Api.Modules.Templating;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Core.Websites
{

    public class ErrorModel : PageModel
    {

        public ResponseStatus Status { get; }

        public Exception? Cause { get; }

        public ErrorModel(IRequest request, ResponseStatus status, Exception? cause) : base(request)
        {
            Status = status;
            Cause = cause;
        }

    }

}
