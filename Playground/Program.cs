using GenHTTP.Engine.Internal;

using GenHTTP.Modules.Practices;

using GenHTTP.Playground.Samples;

var sample = LayoutingSample.Create();

return await Host.Create()
                 .Handler(sample)
                 .Development()
                 .Defaults()
                 .RunAsync();
