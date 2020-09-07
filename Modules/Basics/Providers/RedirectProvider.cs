using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Basics.Providers
{

    public class RedirectProvider : IHandler
    {
        private static readonly Regex PROTOCOL_MATCHER = new Regex("^[a-zA-Z_-]+://", RegexOptions.Compiled | RegexOptions.IgnoreCase);

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

        public IEnumerable<ContentElement> GetContent(IRequest request) => Enumerable.Empty<ContentElement>();

        public IResponse? Handle(IRequest request)
        {
            var resolved = ResolveRoute(request, Target);

            var response = request.Respond()
                                  .Header("Location", resolved);

            var status = MapStatus(request, Temporary);

            return response.Status(status)
                           .Build();
        }

        private string ResolveRoute(IRequest request, string route)
        {
            if (PROTOCOL_MATCHER.IsMatch(route))
            {
                return route;
            }

            var resolved = this.Route(request, route, false);

            if (resolved == null)
            {
                throw new InvalidOperationException($"Unable to determine route to '{route}'");
            }

            var protocol = request.EndPoint.Secure ? "https://" : "http://";

            return $"{protocol}{request.Host}{resolved}";
        }

        private ResponseStatus MapStatus(IRequest request, bool temporary)
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
