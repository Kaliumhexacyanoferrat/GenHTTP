using GenHTTP.Api.Modules;
using GenHTTP.Api.Modules.Templating;
using GenHTTP.Api.Modules.Websites;
using GenHTTP.Api.Protocol;
using GenHTTP.Api.Routing;
using GenHTTP.Modules.Core.General;
using System.Collections.Generic;
using System.Linq;

namespace GenHTTP.Modules.Core.Websites
{

    public class ScriptRouter : RouterBase
    {

        #region Get-/Setters

        private Dictionary<string, Script> Scripts { get; }

        public bool Empty => Scripts.Count == 0;

        private IContentProvider Bundle { get; }

        #endregion

        #region Initialization

        public ScriptRouter(List<Script> scripts,
                            IRenderer<TemplateModel>? template,
                            IContentProvider? errorHandler) : base(template, errorHandler)
        {
            Scripts = scripts.ToDictionary(s => s.Name);

            var bundle = Core.Bundle.Create().ContentType(ContentType.ApplicationJavaScript);

            foreach (var script in Scripts.Values)
            {
                bundle.Add(script.Provider);
            }

            Bundle = bundle.Build();
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

        public override void HandleContext(IEditableRoutingContext current)
        {
            current.Scope(this);

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
            }
        }

        public override IEnumerable<ContentElement> GetContent(IRequest request, string basePath)
        {
            return Scripts.Values.Select(s => new ContentElement($"{basePath}{s.Name}", s.Name, ContentType.ApplicationJavaScript, null));
        }

        public override string? Route(string path, int currentDepth)
        {
            return Parent.Route(path, currentDepth);
        }

        #endregion

    }

}
