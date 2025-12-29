using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

using GenHTTP.Api.Protocol;
using GenHTTP.Api.Content;

using GenHTTP.Modules.Conversion.Serializers.Forms;
using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Reflection.Operations;
using GenHTTP.Modules.IO;

public static class Invoker
{
    public static async ValueTask<IResponse?> Invoke(Delegate logic, Operation operation, IRequest request, IHandler handler, MethodRegistry registry, RequestInterception interception)
    {
        var pathArgs = ArgumentProvider.MatchPath(operation, request);

        int? arg1 = null;

        if (request.Query.TryGetValue("q", out var queryArg1))
        {
            if (!string.IsNullOrEmpty(queryArg1))
            {
                arg1 = (int?)registry.Formatting.Read(queryArg1, typeof(int));
            }
        }

        var interceptionArgs = new Dictionary<string, object?>(1)
        {
            { "q", arg1 }
        };

        var interceptionResult = await interception(request, interceptionArgs);

        if (interceptionResult != null)
        {
            return interceptionResult;
        }

        var typedLogic = (Func<int?, int>)logic;

        var result = typedLogic(
            arg1
        );

        var response = request.Respond()
                              .Content(registry.Formatting.Write(result, result.GetType()) ?? string.Empty)
                              .Build();

        return response;
    }
}
