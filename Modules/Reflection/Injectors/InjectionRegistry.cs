using System.Collections.Generic;

namespace GenHTTP.Modules.Reflection.Injectors
{

    public class InjectionRegistry : List<IParameterInjector>
    {

        #region Initialization

        public InjectionRegistry(IEnumerable<IParameterInjector> injectors) : base(injectors)
        {

        }

        #endregion

    }

}
