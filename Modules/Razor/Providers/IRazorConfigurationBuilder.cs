using System;
using System.Reflection;

namespace GenHTTP.Modules.Razor.Providers
{

    public interface IRazorConfigurationBuilder<out T>
    {

        T AddUsing(string nameSpace);

        T AddAssemblyReference<Type>();

        T AddAssemblyReference(Type type);

        T AddAssemblyReference(Assembly assembly);

        T AddAssemblyReference(string assembly);

    }

}
