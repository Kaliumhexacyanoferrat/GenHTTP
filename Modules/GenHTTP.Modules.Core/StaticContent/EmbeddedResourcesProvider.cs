using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

using GenHTTP.Api.Protocol;
using GenHTTP.Api.Content;
using GenHTTP.Api.Routing;
using System.Xml.Schema;

namespace GenHTTP.Modules.Core.StaticContent
{

    public class EmbeddedResourcesProvider : IHandler
    {

        #region Get-/Setters

        public IHandler Parent { get; }

        private Dictionary<string, IHandler> QualifiedNames { get; }
        
        #endregion

        #region Initialization

        public EmbeddedResourcesProvider(IHandler parent, Assembly assembly, string root)
        {
            Parent = parent;

            QualifiedNames = assembly.GetManifestResourceNames()
                                     .Where(n => n.Contains(root))
                                     .Select(n => new
                                     {
                                         Key = n.Substring(n.IndexOf(root) + root.Length),
                                         Value = Download.FromResource(assembly, n).Build(this)
                                     })
                                     .ToDictionary(n => n.Key!, n => n.Value!);
        }

        #endregion

        #region Functionality

        public IResponse? Handle(IRequest request)
        {
            var identifier = request.Target.Remaining.ToString().Replace('/', '.');

            if (QualifiedNames.ContainsKey(identifier))
            {
                return QualifiedNames[identifier].Handle(request);
            }

            return null;
        }

        public IEnumerable<ContentElement> GetContent(IRequest request)
        {
            foreach (var qn in QualifiedNames)
            {
                var slashPath = qn.Key.Replace('.', '/');
                var fileName = Path.GetFileName(slashPath);

                var path = new List<string>(this.GetRoot(request.Server.Handler, false).Parts);
                path.Add(slashPath);

                yield return new ContentElement(new WebPath(path, false), fileName, fileName.GuessContentType() ?? ContentType.ApplicationForceDownload, null);
            }
        }

        #endregion

    }

}
