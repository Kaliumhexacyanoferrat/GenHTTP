using GenHTTP.Api.Content;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Content.Templating;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.DirectoryBrowsing.Provider
{

    public class ListingModel : IBaseModel
    {

        #region Get-/Setters

        public IRequest Request { get; }

        public IHandler Handler { get; }

        public IResourceContainer Container { get; }

        public bool HasParent { get; }

        #endregion

        #region Intialization

        public ListingModel(IRequest request, IHandler handler, IResourceContainer container, bool hasParent)
        {
            Request = request;
            Handler = handler;

            Container = container;

            HasParent = hasParent;
        }

        #endregion

    }

}
