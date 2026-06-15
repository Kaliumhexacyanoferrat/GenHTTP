using GenHTTP.Engine.Ioxide;
//using GenHTTP.Engine.Internal;
using GenHTTP.Modules.IO;

var app = Content.From(Resource.FromString("Hello World"));


await Host.Create(c => c with
    {
        ReactorCount = Environment.ProcessorCount
    })
    .Handler(app) 
    .Console() 
    .RunAsync();
