using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Templating;
using GenHTTP.Api.Content.Websites;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Core.General;
using System.Collections.Generic;
using System.Linq;

namespace GenHTTP.Modules.Core.Websites
{

    public class ScriptRouter : IHandler
    {

        #region Get-/Setters

        public IHandler Parent { get; }

        private Dictionary<string, Script> Scripts { get; }

        public bool Empty => Scripts.Count == 0;

        private IHandler Bundle { get; }

        #endregion

        #region Initialization

        public ScriptRouter(IHandler parent, List<Script> scripts)
        {
            Parent = parent;

            Scripts = scripts.ToDictionary(s => s.Name);

            var bundle = Core.Bundle.Create().ContentType(ContentType.ApplicationJavaScript);

            foreach (var script in Scripts.Values)
            {
                bundle.Add(script.Provider);
            }

            Bundle = bundle.Build(this);
        }

        #endregion

        #region Functionality

        public List<ScriptReference> GetReferences(bool bundle)
        {
            if (bundle)
            {
                return new List<ScriptReference>
                {
                    new ScriptReference("scripts/bundle.js", false)
                };
            }

            return Scripts.Values.Select(s => new ScriptReference($"scripts/{s.Name}", s.Async)).ToList();
        }

        public IResponse? Handle(IRequest request)
        {
            /*current.Scope(this);

            if (!current.Request.Server.Development)
            {
                if (current.ScopedPath.EndsWith("bundle.js"))
                {
                    current.RegisterContent(Bundle);
                }
            }
            else if (Scripts.TryGetValue(current.ScopedPath.Substring(1), out Script script))
            {
                current.RegisterContent(Download.From(script.Provider)
                                                .Type(ContentType.ApplicationJavaScript)
                                                .Build());
            }*/

            return null; // ToDo
        }

        public IEnumerable<ContentElement> GetContent(IRequest request)
        {
            // ToDo: Basepath
            return Scripts.Values.Select(s => new ContentElement($"{s.Name}", s.Name, ContentType.ApplicationJavaScript, null));
        }

        #endregion

    }

}
