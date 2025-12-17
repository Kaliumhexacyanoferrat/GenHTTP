using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

using GenHTTP.Api.Protocol;
using GenHTTP.Api.Content;

using GenHTTP.Modules.Conversion.Serializers.Forms;
using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.IO;

public static class Invoker
{
    public static  ValueTask<IResponse?> Invoke(Delegate logic, IRequest request, MethodRegistry registry)
    {
        System.Int32? arg1 = null;

        if (request.Query.TryGetValue("i", out var queryArg1))
        {
            arg1 = (System.Int32)Convert.ChangeType(queryArg1, typeof(System.Int32), CultureInfo.InvariantCulture);
        }
        var typedLogic = (Func<System.Int32, System.Int32>)logic;

        var result = typedLogic(
            arg1 ?? default
        );

        var response = request.Respond()
                              .Content(registry.Formatting.Write(result, result.GetType()) ?? string.Empty)
                              .Build();

        return new(response);
    }
}
