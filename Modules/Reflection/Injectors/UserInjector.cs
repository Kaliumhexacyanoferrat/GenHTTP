using System;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Authentication;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Reflection.Injectors
{

    public class UserInjector<T> : IParameterInjector where T : IUser
    {

        #region Get-/Setters

        public bool Supports(Type type) => type == typeof(T);

        #endregion

        #region Functionality

        public object? GetValue(IHandler handler, IRequest request, Type targetType)
        {
            if (request.Properties.TryGet<T>("__AUTH_USER", out var user))
            {
                return user;
            }

            throw new ProviderException(ResponseStatus.Unauthorized, "Authentication required to invoke this endpoint");
        }

        #endregion

    }

}
