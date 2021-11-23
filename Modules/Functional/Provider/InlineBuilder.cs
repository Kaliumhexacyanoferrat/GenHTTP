using System.Collections.Generic;
using System.Linq.Expressions;
using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Modules.Conversion;
using GenHTTP.Modules.Conversion.Providers;

namespace GenHTTP.Modules.Functional.Provider
{

    public class InlineBuilder : IHandlerBuilder<InlineBuilder>
    {
        private readonly List<IConcernBuilder> _Concerns = new();

        private readonly List<InlineFunction> _Functions = new();

        private IBuilder<SerializationRegistry>? _Formats;

        #region Functionality

        public InlineBuilder Formats(IBuilder<SerializationRegistry> registry)
        {
            _Formats = registry;
            return this;
        }

        public InlineBuilder Any(LambdaExpression function)
        {
            _Functions.Add(new InlineFunction(function));
            return this;
        }

        public InlineBuilder Add(IConcernBuilder concern)
        {
            _Concerns.Add(concern);
            return this;
        }

        public IHandler Build(IHandler parent)
        {
            var formats = (_Formats ?? Serialization.Default()).Build();

            return Concerns.Chain(parent, _Concerns, (p) => new InlineHandler(p, _Functions, formats));
        }

        #endregion

    }

}
