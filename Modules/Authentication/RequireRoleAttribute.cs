using GenHTTP.Modules.Authentication.Roles;
using GenHTTP.Modules.Reflection;

namespace GenHTTP.Modules.Authentication;

/// <summary>
/// When annotated on a service method, requests will only be allowed
/// if the authenticated user has the specified roles.
/// </summary>
/// <param name="roles">The roles which need to be present in order to let the request pass</param>
[AttributeUsage(AttributeTargets.Method)]
public class RequireRoleAttribute(params string[] roles) : InterceptWithAttribute<RoleInterceptor>
{

    /// <summary>
    /// The roles which need to be present in order to let the request pass.
    /// </summary>
    public string[] Roles => roles;

}
