using System.Threading.Tasks;

using Scriban;
using Scriban.Runtime;
using Scriban.Syntax;

namespace GenHTTP.Modules.Scriban
{

    public class RoutingMethod : IScriptCustomFunction
    {

        #region Get-/Setters

        #endregion

        #region Initialization

        public RoutingMethod()
        {

        }

        #endregion

        #region Functionality

        public object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
        {
            var route = (string)arguments[0];
            // ToDo: return Context?.Route(route) ?? "";
            return "";
        }

        public ValueTask<object> InvokeAsync(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
        {
            return new ValueTask<object>(Invoke(context, callerContext, arguments, blockStatement));
        }

        #endregion

    }

}
