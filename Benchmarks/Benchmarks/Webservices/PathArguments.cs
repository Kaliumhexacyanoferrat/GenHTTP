using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Webservices;

namespace GenHTTP.Benchmarks.Benchmarks.Webservices;

public class PathArguments
{

    [ResourceMethod(":id")]
    public int Get(int id) => id;

}
