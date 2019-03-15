using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using GenHTTP.Api.Project;
using GenHTTP.Api.Http;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.SessionManagement;
using GenHTTP.Api.Content;
using GenHTTP.Api.Compilation;
using GenHTTP.Localization;
using GenHTTP.Api.Caching;

namespace GenHTTP
{

    /// <summary>
    /// Defines an abstract project which can be used as a template for new
    /// projects.
    /// </summary>
    /// <remarks>
    /// The abstract project provides features like caching, content providers or
    /// template and snippet support.
    /// </remarks>
    public abstract class ProjectBase : IProject
    {
        private IServer _Server;
        private string _LocalFolder;
        private ILog _Log;
        private TemplateCollection _Templates;
        private SnippetCollection _Snippets;
        /// <summary>
        /// Content providers of this project.
        /// </summary>
        protected List<IContentProvider> _Providers;
        /// <summary>
        /// The session manager of this project.
        /// </summary>
        protected SessionManager _Sessions;
        /// <summary>
        /// The localization of this project.
        /// </summary>
        protected LocalizationManager _Localization;
        private IContentProvider _NotFoundProvider;
        private IContentProvider _NotLoggedInProvider;
        private IContentProvider _NotEnoughRightsProvider;
        /// <summary>
        /// The cache of this project.
        /// </summary>
        protected ProjectCache _Cache;

        #region Constructors

        /// <summary>
        /// Initializes collections etc.
        /// </summary>
        public ProjectBase()
        {
            _Templates = new TemplateCollection();
            _Snippets = new SnippetCollection();
            _Providers = new List<IContentProvider>();
        }

        #endregion

        #region Project initialization and shutdown

        /// <summary>
        /// Initialize the project.
        /// </summary>
        /// <remarks>
        /// The server will call this method on initialization.
        /// </remarks>
        /// <param name="server">The server the project is running on</param>
        /// <param name="localFolder">The folder the project DLL lies in</param>
        /// <param name="log">The log file for this project</param>
        public virtual void Init(IServer server, string localFolder, ILog log)
        {
            _Server = server;
            _LocalFolder = localFolder;
            _Log = log;
            // init cache
            _Cache = new ProjectCache(this);
            // let the project init the collections etc.
            InitSessionManager();
            InitTemplateBases();
            InitSnippetBases();
            // set default content providers
            _NotFoundProvider = server.DefaultNotFoundProvider;
            _NotLoggedInProvider = server.DefaultNotLoggedInProvider;
            _NotEnoughRightsProvider = server.DefaultNotEnoughRightsProvider;
            // let the user add his own providers
            InitContentProviders();
            // init other stuff
            InitOther();
        }

        /// <summary>
        /// Allows the project to do some stuff before the server
        /// shuts down.
        /// </summary>
        public abstract void Dump();

        /// <summary>
        /// Allows the project to initialize some other stuff needed.
        /// </summary>
        /// <remarks>
        /// This method will be called after all the other init methods
        /// were called.
        /// </remarks>
        public abstract void InitOther();

        #endregion

        #region get-/setters

        /// <summary>
        /// The project's root folder.
        /// </summary>
        public string LocalFolder
        {
            get { return _LocalFolder; }
        }

        /// <summary>
        /// The name of the project.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// The version of the project.
        /// </summary>
        public abstract string Version { get; }

        /// <summary>
        /// Retrieve the template bases of this project.
        /// </summary>
        public TemplateCollection TemplateBases
        {
            get { return _Templates; }
        }

        /// <summary>
        /// Retrieve the snippet bases of this project.
        /// </summary>
        public SnippetCollection Snippets
        {
            get { return _Snippets; }
        }

        /// <summary>
        /// Retrieve the session manager of this project.
        /// </summary>
        public SessionManager SessionManager
        {
            get { return _Sessions; }
        }

        /// <summary>
        /// Retrieve the localization manager of this project.
        /// </summary>
        public LocalizationManager LocalizationManager
        {
            get { return _Localization; }
        }

        /// <summary>
        /// Error 404 generator.
        /// </summary>
        public IContentProvider NotFoundProvider
        {
            get { return _NotFoundProvider; }
            set { _NotFoundProvider = value; }
        }

        /// <summary>
        /// Provides a login page.
        /// </summary>
        public IContentProvider NotLoggedInProvider
        {
            get { return _NotLoggedInProvider; }
            set { _NotLoggedInProvider = value; }
        }

        /// <summary>
        /// Provides a "permission denied" page.
        /// </summary>
        public IContentProvider NotEnoughRightsProvider
        {
            get { return _NotEnoughRightsProvider; }
            set { _NotEnoughRightsProvider = value; }
        }

        /// <summary>
        /// The cache of the project.
        /// </summary>
        public ProjectCache Cache
        {
            get { return _Cache; }
        }

        /// <summary>
        /// The server the project is running on.
        /// </summary>
        public IServer Server
        {
            get { return _Server; }
        }

        /// <summary>
        /// The log writer of this project.
        /// </summary>
        public ILog Log
        {
            get { return _Log; }
            set { _Log = value; }
        }

        #endregion

        #region Templating

        /// <summary>
        /// Add template bases to this project.
        /// </summary>
        protected abstract void InitTemplateBases();

        /// <summary>
        /// Retrieve an instance of a template.
        /// </summary>
        /// <param name="name">The name of the template to retrieve</param>
        /// <returns>The requested template, if one could be instantiated</returns>
        public abstract ITemplate GetTemplateInstance(string name);

        #endregion

        #region Snippets

        /// <summary>
        /// Add snippet bases to this project.
        /// </summary>
        protected abstract void InitSnippetBases();

        /// <summary>
        /// Retrieve an instance of a snippet.
        /// </summary>
        /// <param name="name">The name of the snippet to retrieve</param>
        /// <returns>The requested snippet, if one chould be instantiated</returns>
        public abstract ISnippet GetSnippetInstance(string name);

        #endregion

        #region Content generation

        /// <summary>
        /// Allows the inheriting project to add content providers.
        /// </summary>
        protected abstract void InitContentProviders();

        /// <summary>
        /// Allows you to add a content provider to the internal list.
        /// </summary>
        /// <param name="provider">The provider to add</param>
        protected void AddContentProvider(IContentProvider provider)
        {
            if (!_Providers.Contains(provider)) _Providers.Add(provider);
        }

        #endregion

        #region Session management

        /// <summary>
        /// Allows the inheriting project to load a session manager file.
        /// </summary>
        protected abstract void InitSessionManager();

        #endregion

        #region Localization management

        /// <summary>
        /// Allows the inheriting project to load a localization file.
        /// </summary>
        protected abstract void InitLocalizationManager();

        #endregion

        #region Request handling

        /// <summary>
        /// Allows you to apply directives for a whole project.
        /// </summary>
        /// <param name="request">The request to handle</param>
        /// <param name="response">The response to adapt</param>
        public abstract void PreHandleRequest(IHttpRequest request, IHttpResponse response);

        /// <summary>
        /// Handles incoming requests.
        /// </summary>
        /// <param name="request">The request to handle</param>
        /// <param name="response">The response to write to</param>
        public virtual void HandleRequest(IHttpRequest request, IHttpResponse response)
        {
            // pre-handle request
            PreHandleRequest(request, response);
            // check session
            AuthorizationInfo info = null;
            if (_Sessions != null) info = _Sessions.CheckSession(request, response);
            // check cache for entries
            if (_Cache.HandleRequest(request, response, info)) return;
            // search for a matching provider
            foreach (IContentProvider provider in _Providers)
            {
                if (provider.IsResponsible(request, info))
                {
                    if (provider.RequiresLogin && info.Session == null)
                    {
                        // failed, because the user is not logged in
                        _NotLoggedInProvider.HandleRequest(request, response, info);
                        return;
                    }
                    else
                    {
                        // handler will handle the request for us (hopefully)
                        provider.HandleRequest(request, response, info);
                        if (response.Sent) return;
                    }
                }
            }
            // could not handle
            _NotFoundProvider.HandleRequest(request, response, info);
        }

        #endregion

    }

}
