using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using GenHTTP.Api.Routing;
using Scriban;
using Scriban.Runtime;
using Scriban.Syntax;

namespace GenHTTP.Content.Templating.Functions
{

    public class RoutingMethod : IScriptCustomFunction
    {

        #region Get-/Setters

        public IRoutingContext? Context { get; }

        #endregion

        #region Initialization

        public RoutingMethod(IRoutingContext? context)
        {
            Context = context;
        }

        #endregion

        #region Functionality

        public object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
        {
            var route = (string)arguments[0];
            return Context?.Route(route) ?? "";
        }

        public ValueTask<object> InvokeAsync(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
        {
            return new ValueTask<object>(Invoke(context, callerContext, arguments, blockStatement));
        }

        #endregion

    }

}
