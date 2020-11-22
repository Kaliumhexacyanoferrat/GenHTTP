using System.Collections.Generic;
using System.Linq;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Content.Websites;

namespace GenHTTP.Modules.Websites.Resources
{

    public sealed class ScriptRouterBuilder : IHandlerBuilder<ScriptRouterBuilder>
    {
        private readonly List<Script> _Scripts = new();

        private ITheme? _Theme;

        private readonly List<IConcernBuilder> _Concerns = new();

        #region Functionality

        public ScriptRouterBuilder Add(string name, IResource provider, bool asynchronous = false)
        {
            _Scripts.Add(new Script(name, asynchronous, provider));
            return this;
        }

        public ScriptRouterBuilder Add(IConcernBuilder concern)
        {
            _Concerns.Add(concern);
            return this;
        }

        public ScriptRouterBuilder Theme(ITheme theme)
        {
            _Theme = theme;
            return this;
        }

        public IHandler Build(IHandler parent)
        {
            var scripts = _Theme is not null ? _Theme.Scripts.Union(_Scripts) : _Scripts;

            return Concerns.Chain(parent, _Concerns, (p) => new ScriptRouter(p, scripts.ToList()));
        }

        #endregion

    }

}
