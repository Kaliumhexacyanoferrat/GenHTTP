using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Webservices;

namespace genhttp.Tests;

public class Baseline
{

    [ResourceMethod]
    public int Sum(int a, int b) => a + b;

    [ResourceMethod(Method.Post)]
    public int Sum(int a, int b, [FromBody] int c) => a + b + c;

}
