using System.Text.RegularExpressions;
using System.Threading.Tasks;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Basics.Providers
{

    public sealed class RedirectProvider : IHandler
    {
        private static readonly Regex PROTOCOL_MATCHER = new("^[a-zA-Z_-]+://", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        #region Get-/Setters

        public string Target { get; }

        public bool Temporary { get; }

        public IHandler Parent { get; }

        #endregion

        #region Initialization

        public RedirectProvider(IHandler parent, string location, bool temporary)
        {
            Parent = parent;

            Target = location;
            Temporary = temporary;
        }

        #endregion

        #region Functionality

        public ValueTask PrepareAsync() => ValueTask.CompletedTask;

        public ValueTask<IResponse?> HandleAsync(IRequest request)
        {
            var resolved = ResolveRoute(request, Target);

            var response = request.Respond()
                                  .Header("Location", resolved);

            var status = MapStatus(request, Temporary);

            return new ValueTask<IResponse?>(response.Status(status).Build());
        }

        private static string ResolveRoute(IRequest request, string route)
        {
            if (PROTOCOL_MATCHER.IsMatch(route))
            {
                return route;
            }

            var protocol = request.EndPoint.Secure ? "https://" : "http://";

            return $"{protocol}{request.Host}{route}";
        }

        private static ResponseStatus MapStatus(IRequest request, bool temporary)
        {
            if (request.HasType(RequestMethod.GET, RequestMethod.HEAD))
            {
                return (temporary) ? ResponseStatus.TemporaryRedirect : ResponseStatus.MovedPermanently;
            }
            else
            {
                return (temporary) ? ResponseStatus.SeeOther : ResponseStatus.PermanentRedirect;
            }
        }

        #endregion

    }

}
