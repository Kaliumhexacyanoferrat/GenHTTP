using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Webservices;

namespace GenHTTP.Testing.Acceptance.HttpArena;

public class BaselineService
{
    [ResourceMethod]
    public int Sum(int a, int b) => a + b;

    [ResourceMethod(Method.Post)]
    public int Sum(int a, int b, [FromBody] int c) => a + b + c;
}
