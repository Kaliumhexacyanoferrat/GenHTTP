using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Webservices;

namespace GenHTTP.Benchmarks.Benchmarks.Webservices;

public class Baseline
{

    [ResourceMethod]
    public int Sum(int a, int b) => a + b;

    [ResourceMethod(Method.Post)]
    public int Sum(int a, int b, [FromBody] int c) => a + b + c;

}
