using System.Collections.Generic;
using System.Linq;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Websites;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Basics;
using GenHTTP.Modules.IO;

namespace GenHTTP.Modules.Websites.Resources
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

            var bundle = Websites.Bundle.Create().ContentType(ContentType.ApplicationJavaScript);

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
            var file = request.Target.Current;

            if (file != null)
            {
                if (!request.Server.Development)
                {
                    if (file == "bundle.js")
                    {
                        return Bundle.Handle(request);
                    }
                }
                else if (Scripts.TryGetValue(file, out Script script))
                {
                    return Download.From(script.Provider)
                                   .Type(ContentType.ApplicationJavaScript)
                                   .Build(this)
                                   .Handle(request);
                }
            }

            return null;
        }

        public IEnumerable<ContentElement> GetContent(IRequest request)
        {
            var path = this.GetRoot(request.Server.Handler, false);

            return Scripts.Values.Select(s =>
            {
                var childPath = path.Edit(false)
                                    .Append(s.Name)
                                    .Build();

                return new ContentElement(childPath, s.Name, ContentType.ApplicationJavaScript, null);
            });
        }

        #endregion

    }

}
