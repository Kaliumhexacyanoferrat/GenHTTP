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
        System.String? arg1 = null;

        if (request.Query.TryGetValue("x", out var queryArg1))
        {
            arg1 = queryArg1;
        }

        var typedLogic = (Func<System.String, System.String>)logic;

        var result = typedLogic(
            arg1 ?? string.Empty
        );

        var response = request.Respond()
                              .Content(result)
                              .Build();

        return new(response);
    }
}
