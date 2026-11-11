using GenHTTP.Engine.Internal;
using GenHTTP.Modules.Practices;

using GenHTTP.Playground.Samples;
using GenHTTP.Playground.Samples.Ioxide;

var sample = FunctionalSample.Create();

return await Host.Create()
                 .Handler(sample)
                 .Development()
                 .Defaults()
                 .RunAsync();
