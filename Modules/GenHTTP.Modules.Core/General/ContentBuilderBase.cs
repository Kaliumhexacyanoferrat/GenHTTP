using GenHTTP.Api.Modules;

namespace GenHTTP.Modules.Core.General
{

    public abstract class ContentBuilderBase : IContentBuilder
    {
        protected ResponseModification? _Modification;

        #region Functionality

        public IContentBuilder Modify(ResponseModification modification)
        {
            _Modification = modification;
            return this;
        }

        public abstract IContentProvider Build();

        #endregion

    }

}
