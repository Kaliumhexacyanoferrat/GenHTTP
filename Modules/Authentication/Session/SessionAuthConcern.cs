using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Authentication.Session
{

    public class SessionAuthConcern : IConcern
    {

        private const string HANDLER_SEGMENT = "login";

        #region Get-/Setters

        public IHandler Parent { get; }

        public IHandler Content { get; }

        private Func<IRequest, ValueTask<ISession?>> SessionLoader { get; }

        #endregion

        #region Initialization

        public SessionAuthConcern(IHandler parent, Func<IHandler, IHandler> contentFactory, Func<IRequest, ValueTask<ISession?>> sessionLoader)
        {
            Parent = parent;
            Content = contentFactory(this);

            SessionLoader = sessionLoader;
        }

        #endregion

        #region Functionality

        public async ValueTask<IResponse?> HandleAsync(IRequest request)
        {
            /*
             * sessionloader
sessiongenerator
login
anonymous

isession? uuid?

request.getsession? mit IUser?

IdentifiedSession

returnto*/

            var current = request.Target.Current;

            if (current == HANDLER_SEGMENT)
            {

            }
            else
            {


            }
            var session = await SessionLoader(request);

            return await Content.HandleAsync(request);
        }

        public IEnumerable<ContentElement> GetContent(IRequest request) => Content.GetContent(request);

        public ValueTask PrepareAsync() => Content.PrepareAsync();

        #endregion

    }

}
