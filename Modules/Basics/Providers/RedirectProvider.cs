using System.Collections.Generic;
using System.Linq;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Basics.Providers
{

    public class RedirectProvider : IHandler
    {

        #region Get-/Setters

        public string Location { get; }

        public bool Temporary { get; }

        public IHandler Parent { get; }

        #endregion

        #region Initialization

        public RedirectProvider(IHandler parent, string location, bool temporary)
        {
            Parent = parent;

            Location = location;
            Temporary = temporary;
        }

        #endregion

        #region Functionality

        public IResponse? Handle(IRequest request)
        {
            var response = request.Respond()
                                  .Header("Location", Location);

            if (Temporary)
            {
                return response.Status(ResponseStatus.TemporaryRedirect).Build();
            }
            else
            {
                if (request.Method.KnownMethod == RequestMethod.GET)
                {
                    return response.Status(ResponseStatus.MovedPermanently).Build();
                }
                else
                {
                    return response.Status(ResponseStatus.PermanentRedirect).Build();
                }
            }
        }

        public IEnumerable<ContentElement> GetContent(IRequest request) => Enumerable.Empty<ContentElement>();

        #endregion

    }

}
