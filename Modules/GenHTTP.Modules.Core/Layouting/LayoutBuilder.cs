using System;
using System.Collections.Generic;
using System.Text;

using GenHTTP.Api.Routing;
using GenHTTP.Api.Modules;
using GenHTTP.Modules.Core.General;

namespace GenHTTP.Modules.Core.Layouting
{

    public class LayoutBuilder : RouterBuilderBase<LayoutBuilder>
    {
        private string? _Index;

        #region Get-/Setters
        
        private Dictionary<string, IRouter> Routes { get; }

        private Dictionary<string, IContentProvider> Content { get; }
        
        #endregion

        #region Initialization

        public LayoutBuilder()
        {
            Routes = new Dictionary<string, IRouter>();
            Content = new Dictionary<string, IContentProvider>();
        }

        #endregion

        #region Functionality

        public LayoutBuilder Index(string index)
        {
            _Index = index;
            return this;
        }
        
        public LayoutBuilder Add(string route, IRouterBuilder router, bool index = false)
        {
            return Add(route, router.Build());
        }

        public LayoutBuilder Add(string route, IRouter router, bool index = false)
        {
            Routes.Add(route, router);
            return (index) ? Index(route) : this;
        }

        public LayoutBuilder Add(string file, IContentBuilder content, bool index = false)
        {
            return Add(file, content.Build(), index);
        }

        public LayoutBuilder Add(string file, IContentProvider content, bool index = false)
        {
            Content.Add(file, content);
            return (index) ? Index(file) : this;
        }

        public override IRouter Build()
        {
            return new LayoutRouter(Routes, Content, _Index, _Template, _ErrorHandler);
        }

        #endregion

    }

}
