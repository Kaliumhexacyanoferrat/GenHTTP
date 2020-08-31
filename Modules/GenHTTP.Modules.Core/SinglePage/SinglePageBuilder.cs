using System.Collections.Generic;

using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Modules.Core.SinglePage
{

    public class SinglePageBuilder : IHandlerBuilder<SinglePageBuilder>
    {
        private string? _Directory;

        private readonly List<IConcernBuilder> _Concerns = new List<IConcernBuilder>();

        #region Functionality

        public SinglePageBuilder Add(IConcernBuilder concern)
        {
            _Concerns.Add(concern);
            return this;
        }

        public SinglePageBuilder Directory(string directory)
        {
            _Directory = directory;
            return this;
        }

        public IHandler Build(IHandler parent)
        {
            var directory = _Directory ?? throw new BuilderMissingPropertyException("directory");

            return Concerns.Chain(parent, _Concerns, (p) => new SinglePageProvider(p, directory));
        }

        #endregion

    }

}
