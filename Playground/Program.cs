using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Engine.Internal;
using GenHTTP.Modules.Functional;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.Reflection;

var handler = Handler.From((request) =>
{
    /*var i = int.Parse(request.Target.Current?.Value ?? throw new InvalidOperationException());

    var j = int.Parse(request.Query["j"]);

    return request.Respond()
                  .Content((i + j).ToString())
                  .Type(ContentType.TextPlain)
                  .Build();*/

    int? arg1 = null;

    if (request.Query.TryGetValue("i", out var queryArg1))
    {
        if (!string.IsNullOrEmpty(queryArg1))
        {
            if (int.TryParse(queryArg1, out var queryArg1Typed))
            {
                arg1 = queryArg1Typed;
            }
            else
            {
                throw new ProviderException(ResponseStatus.BadRequest, "Invalid format for input parameter 'i'");
            }
        }
    }

    int? arg2 = null;

    if (request.Query.TryGetValue("j", out var queryArg2))
    {
        if (!string.IsNullOrEmpty(queryArg2))
        {
            if (int.TryParse(queryArg2, out var queryArg2Typed))
            {
                arg2 = queryArg2Typed;
            }
            else
            {
                throw new ProviderException(ResponseStatus.BadRequest, "Invalid format for input parameter 'j'");
            }
        }
    }

    var result = arg1 + arg2;

    var formattedResult = result.ToString() ?? string.Empty;

    var response = request.Respond()
                          .Content(formattedResult)
                          .Build();

    return response;
});

var withCodeGen = Inline.Create().Get((int i, int j) => i + j).ExecutionMode(ExecutionMode.Auto);

var withReflection = Inline.Create().Get((int i, int j) => i + j).ExecutionMode(ExecutionMode.Reflection);

var test = Inline.Create()
                 .Get(() => 42)
                 .ExecutionMode(ExecutionMode.Auto);

var app = Layout.Create()
                .Add("handler", handler)
                .Add("codegen", withCodeGen)
                .Add("reflection", withReflection);

await Host.Create()
          .Handler(app)
          .RunAsync(); // or StartAsync() for non-blocking
