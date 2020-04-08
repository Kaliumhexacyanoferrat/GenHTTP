using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Websites;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace GenHTTP.Modules.Core.Websites
{

    public class ScriptRouterBuilder : IHandlerBuilder
    {
        private readonly List<Script> _Scripts = new List<Script>();

        private ITheme? _Theme;

        #region Functionality

        public ScriptRouterBuilder Add(string name, IResourceProvider provider, bool asynchronous = false)
        {
            _Scripts.Add(new Script(name, asynchronous, provider));
            return this;
        }

        public ScriptRouterBuilder Theme(ITheme theme)
        {
            _Theme = theme;
            return this;
        }

        public IHandler Build(IHandler parent)
        {
            var scripts = (_Theme != null) ? _Theme.Scripts.Union(_Scripts) : _Scripts;

            return new ScriptRouter(parent, scripts.ToList());
        }

        #endregion

    }

}
