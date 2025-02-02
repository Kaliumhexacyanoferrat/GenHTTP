﻿using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Authentication;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Reflection.Operations;

namespace GenHTTP.Modules.Authentication.Roles;

public class RoleInterceptor : IOperationInterceptor
{
    private string[]? _Roles;

    public void Configure(object attribute)
    {
        if (attribute is RequireRoleAttribute roleAttribute)
        {
            _Roles = roleAttribute.Roles;
        }
    }

    public ValueTask<InterceptionResult?> InterceptAsync(IRequest request, Operation operation, IReadOnlyDictionary<string, object?> arguments)
    {
        if (_Roles?.Length > 0)
        {
            var user = request.GetUser<IUser>();

            if (user == null)
            {
                throw new ProviderException(ResponseStatus.Unauthorized, "Authorization required to access this endpoint");
            }

            var userRoles = user.Roles;

            var missing = new List<string>(_Roles.Length);

            if (userRoles != null)
            {
                foreach (var role in _Roles)
                {
                    if (!userRoles.Contains(role, StringComparer.OrdinalIgnoreCase))
                    {
                        missing.Add(role);
                    }
                }
            }
            else
            {
                missing.AddRange(_Roles);
            }

            if (missing.Count > 0)
            {
                throw new ProviderException(ResponseStatus.Forbidden, "User is not authorized to access this endpoint.");
            }
        }

        return default;
    }

}
