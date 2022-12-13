using System.Collections.Generic;

using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Modules.Reflection.Injectors
{

    public class InjectionRegistryBuilder : IBuilder<InjectionRegistry>
    {
        private readonly List<IParameterInjector> _Injectors = new();

        #region Functionality

        public InjectionRegistryBuilder Add(IParameterInjector injector)
        {
            _Injectors.Add(injector);
            return this;
        }

        public InjectionRegistry Build() => new(_Injectors);

        #endregion

    }

}
