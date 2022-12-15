using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Websites;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Basics;
using GenHTTP.Modules.IO;

namespace GenHTTP.Modules.Websites.Resources
{

    public sealed class ScriptRouter : IHandler
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

            var bundle = Websites.Bundle.Create().ContentType(ContentType.ApplicationJavaScript, "UTF-8");

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

        public ValueTask<IResponse?> HandleAsync(IRequest request)
        {
            var file = request.Target.Current?.Value;

            if (file is not null)
            {
                if (!request.Server.Development)
                {
                    if (file == "bundle.js")
                    {
                        return Bundle.HandleAsync(request);
                    }
                }
                else if (Scripts.TryGetValue(file, out Script? script))
                {
                    return Content.From(script.Provider)
                                   .Build(this)
                                   .HandleAsync(request);
                }
            }

            return new ValueTask<IResponse?>();
        }

        public IAsyncEnumerable<ContentElement> GetContentAsync(IRequest request)
        {
            var path = this.GetRoot(request, false);

            return Scripts.Values.Select(s =>
            {
                var childPath = path.Edit(false)
                                    .Append(s.Name)
                                    .Build();

                var info = ContentInfo.Create()
                                      .Title(s.Name)
                                      .Build();

                return new ContentElement(childPath, info, ContentType.ApplicationJavaScript, null);
            }).ToAsyncEnumerable();
        }

        public ValueTask PrepareAsync() => Bundle.PrepareAsync();

        #endregion

    }

}
