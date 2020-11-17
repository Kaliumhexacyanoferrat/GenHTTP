using System;
using System.Collections.Generic;
using System.Linq;

using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Modules.Compression.Providers
{

    public class CompressionConcernBuilder : IConcernBuilder
    {
        private readonly List<ICompressionAlgorithm> _Algorithms = new();

        #region Functionality

        public CompressionConcernBuilder Add(IBuilder<ICompressionAlgorithm> algorithm) => Add(algorithm.Build());

        public CompressionConcernBuilder Add(ICompressionAlgorithm algorithm)
        {
            _Algorithms.Add(algorithm);
            return this;
        }

        public IConcern Build(IHandler parent, Func<IHandler, IHandler> contentFactory)
        {
            var algorithms = _Algorithms.ToDictionary(a => a.Name);

            return new CompressionConcern(parent, contentFactory, algorithms);
        }

        #endregion

    }

}
