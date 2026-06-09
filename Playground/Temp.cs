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
using GenHTTP.Modules.Reflection.Routing;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.IO.Formattable;
using GenHTTP.Modules.IO.Strings;

public static class Invoker
{
    private static readonly ByteString QueryArg1Name = new("a");
    private static readonly ByteString QueryArg2Name = new("b");


    public static  ValueTask<IResponse?> Invoke(Delegate logic, Operation operation, IRequest request, IHandler handler, MethodRegistry registry, RoutingMatch routingMatch, RequestInterception interception)
    {
        int? arg1 = null;

        var queryArg1 = request.Header.Query.GetEntry(QueryArg1Name);
        if (queryArg1 != null)
        {
            if (!queryArg1.Value.Bytes.IsEmpty)
            {
               try
               {
                arg1 = registry.Formatting.Read<int>(queryArg1.Value);
               }
               catch (Exception e)
               {
                  throw new ProviderException(ResponseStatus.BadRequest, "Failed read 'a'", e);
               }
            }
        }

        int? arg2 = null;

        var queryArg2 = request.Header.Query.GetEntry(QueryArg2Name);
        if (queryArg2 != null)
        {
            if (!queryArg2.Value.Bytes.IsEmpty)
            {
               try
               {
                arg2 = registry.Formatting.Read<int>(queryArg2.Value);
               }
               catch (Exception e)
               {
                  throw new ProviderException(ResponseStatus.BadRequest, "Failed read 'b'", e);
               }
            }
        }

        var typedLogic = (Func<int, int, int>)logic;

        var result = typedLogic(
            arg1 ?? default,
            arg2 ?? default
        );

        var content = new FormattableContent(result);

        var response = request.Respond()
                              .Content(content)
                              .Build();

        return new(response);
    }
}
