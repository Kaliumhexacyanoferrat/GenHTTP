using GenHTTP.Engine.Internal;

using GenHTTP.Modules.DependencyInjection;
using GenHTTP.Modules.Practices;

using GenHTTP.Playground.Browser;
using GenHTTP.Playground.Samples;

/*
 *
 * Serves all samples, open http://localhost:8080/ to browse them and call them
 * from your browser. To run a single sample, pass it to the host instead, e.g.
 *
 *   .Handler(WebserviceSample.Create())
 *
 * The samples for the ioxide engine create their own host, see Samples/Ioxide/README.md.
 *
 */

return await Host.Create()
                 .Handler(SampleBrowser.Create())
                 .AddDependencyInjection(DependencyInjectionSample.Services())
                 .Development()
                 .Defaults()
                 .RunAsync();
