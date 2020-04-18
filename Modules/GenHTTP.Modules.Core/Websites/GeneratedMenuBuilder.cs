
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Content.Websites;
using GenHTTP.Api.Content;

namespace GenHTTP.Modules.Core.Websites
{

    public class GeneratedMenuBuilder : IBuilder<IMenuProvider>
    {
        private IHandler? _Handler;

        #region Functionality

        public GeneratedMenuBuilder Handler(IHandler handler)
        {
            _Handler = handler;
            return this;
        }

        public IMenuProvider Build()
        {
            var handler = _Handler ?? throw new BuilderMissingPropertyException("handler");

            return new GeneratedMenuProvider(handler);
        }

        #endregion

    }

}
