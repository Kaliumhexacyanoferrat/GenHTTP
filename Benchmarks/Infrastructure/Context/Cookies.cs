using GenHTTP.Api.Protocol;

namespace GenHTTP.Benchmarks.Infrastructure.Context;

public class Cookies : Dictionary<string, Cookie>, ICookieCollection;
