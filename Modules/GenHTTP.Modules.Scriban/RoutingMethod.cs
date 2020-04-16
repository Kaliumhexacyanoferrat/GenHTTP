using System.Threading.Tasks;

using Scriban;
using Scriban.Runtime;
using Scriban.Syntax;

using GenHTTP.Api.Content.Templating;
using GenHTTP.Modules.Core;

namespace GenHTTP.Modules.Scriban
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
            var route = (string)arguments[0];
            return Model.Handler.Route(Model.Request, route) ?? "";
        }

        public ValueTask<object> InvokeAsync(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
        {
            return new ValueTask<object>(Invoke(context, callerContext, arguments, blockStatement));
        }

        #endregion

    }

}
