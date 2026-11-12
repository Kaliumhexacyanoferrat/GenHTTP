using GenHTTP.Engine.Internal;

using GenHTTP.Modules.IO;
using GenHTTP.Modules.Practices;

var app = Content.From(Resource.FromString("Hello World!"));

return await Host.Create()
                 .Handler(app)
                 .Development()
                 .Defaults()
                 .RunAsync();
