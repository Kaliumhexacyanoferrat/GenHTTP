using GenHTTP.Engine.Kestrel;

using GenHTTP.Modules.Mcp;
using GenHTTP.Modules.Practices;

// var app = Content.From(Resource.FromString("Hello World"));

var app = Tools.Create()
               .Add(new Add());

await Host.Create()
          .Handler(app)
          .Defaults()
          .Development()
          .Console()
          .RunAsync();

class Add : ITool<(int A, int B), int>
{

    public string Name => "add";

    public string Description => "Adds to integers and returns the result";

    public int Call((int A, int B) input)
    {
        return input.A + input.B;
    }

}
