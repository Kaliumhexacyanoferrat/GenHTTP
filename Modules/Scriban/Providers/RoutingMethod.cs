using System;
using System.Threading.Tasks;

using Scriban;
using Scriban.Runtime;
using Scriban.Syntax;

using GenHTTP.Api.Content.Templating;
using GenHTTP.Api.Routing;

using GenHTTP.Modules.Basics;

namespace GenHTTP.Modules.Scriban.Providers
{

    public class RoutingMethod : IScriptCustomFunction
    {

        #region Get-/Setters

        private IBaseModel Model { get; }

        #endregion

        #region Initialization

        public RoutingMethod(IBaseModel model)
        {
            Model = model;
        }

        #endregion

        #region Functionality

        public object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
        {
            if (arguments.Count != 1)
            {
                throw new InvalidOperationException("Routing method expects exactly one argument");
            }

            if (arguments[0] is string strRoute)
            {
                return Model.Handler.Route(Model.Request, strRoute) ?? string.Empty;
            }

            if (arguments[0] is WebPath pthRoute)
            {
                return Model.Handler.Route(Model.Request, pthRoute) ?? string.Empty;
            }

            return string.Empty;
        }

        public ValueTask<object> InvokeAsync(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
        {
            return new ValueTask<object>(Invoke(context, callerContext, arguments, blockStatement));
        }

        #endregion

    }

}
