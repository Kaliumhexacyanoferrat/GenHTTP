using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Authentication;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Reflection.Operations;

namespace GenHTTP.Modules.Authentication.Roles;

public class RoleInterceptor : IOperationInterceptor
{
    private string[]? _roles;

    public void Configure(object attribute)
    {
        if (attribute is RequireRoleAttribute roleAttribute)
        {
            _roles = roleAttribute.Roles;
        }
    }

    public ValueTask<InterceptionResult?> InterceptAsync(IRequest request, Operation operation, IReadOnlyDictionary<string, object?> arguments)
    {
        if (_roles?.Length > 0)
        {
            var user = request.GetUser<IUser>();

            if (user == null)
            {
                throw new ProviderException(ResponseStatus.Unauthorized, "Authorization required to access this endpoint");
            }

            var userRoles = user.Roles;

            var missing = new List<string>(_roles.Length);

            if (userRoles != null)
            {
                foreach (var role in _roles)
                {
                    if (!userRoles.Contains(role, StringComparer.OrdinalIgnoreCase))
                    {
                        missing.Add(role);
                    }
                }
            }
            else
            {
                missing.AddRange(_roles);
            }

            if (missing.Count > 0)
            {
                throw new ProviderException(ResponseStatus.Forbidden, "User is not authorized to access this endpoint.");
            }
        }

        return default;
    }

}
