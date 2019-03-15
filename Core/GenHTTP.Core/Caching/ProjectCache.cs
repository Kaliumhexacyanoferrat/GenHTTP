using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using GenHTTP.SessionManagement;

namespace GenHTTP.Caching
{

    /// <summary>
    /// Caches responses for a project.
    /// </summary>
    /// <remarks>
    /// The cache can be used to store any types of responses (like
    /// downloads, documents, templates, plain text, ...).
    /// </remarks>
    public class ProjectCache
    {
        private long _MaxSize;
        private long _CurrentSize;
        private uint _MaxEntries;
        private AbstractProject _Project;
        private Dictionary<string, CachedResponse> _Cache;

        #region Constructors

        /// <summary>
        /// Create a new project cache.
        /// </summary>
        /// <param name="project">The related project</param>
        public ProjectCache(AbstractProject project)
        {
            _MaxSize = 1048576;
            _MaxEntries = 50;
            _CurrentSize = 0;
            _Project = project;
            _Cache = new Dictionary<string, CachedResponse>();
            _Project.Server.OnTimerTick += new TimerTick(Tick);
        }

        /// <summary>
        /// Create a new project cache.
        /// </summary>
        /// <param name="project">The related project</param>
        /// <param name="maxSize">The maximum size of the cache in bytes</param>
        /// <param name="maxEntries">The maximum number of cached entries</param>
        public ProjectCache(AbstractProject project, long maxSize, uint maxEntries) : this(project)
        {
            _MaxSize = maxSize;
            _MaxEntries = maxEntries;
        }

        #endregion

        #region get-/setters

        /// <summary>
        /// The maximum size of the cache in bytes.
        /// </summary>
        public long MaxSize
        {
            get { return _MaxSize; }
            set { _MaxSize = value; }
        }

        /// <summary>
        /// The maximum number of entries.
        /// </summary>
        public uint MaxEntries
        {
            get { return _MaxEntries; }
            set { _MaxEntries = value; }
        }

        /// <summary>
        /// The usage of the cache in percent.
        /// </summary>
        public double Usage
        {
            get { return ((double)_CurrentSize / _MaxSize) * 100.0; }
        }

        /// <summary>
        /// The number of cached entries.
        /// </summary>
        public int Count
        {
            get { return _Cache.Count; }
        }

        #endregion

        #region Cache management

        /// <summary>
        /// This method will be called if the server's tick event
        /// is fired. It will remove expired cache entries.
        /// </summary>
        private void Tick()
        {
            lock (_Cache)
            {
                List<string> toRemove = new List<string>();
                foreach (string file in _Cache.Keys)
                {
                    if (_Cache[file].Tick()) toRemove.Add(file);
                }
                foreach (string file in toRemove)
                {
                    _CurrentSize = _CurrentSize - _Cache[file].Data.Length - _Cache[file].CompressedData.Length;
                    _Cache.Remove(file);
                }
            }
            // log interesting information
            if (Usage > 0.9) _Project.Server.Log.WriteLine("Cache of project '" + _Project.Name + "' is nearly full (" + Usage + "%)");
        }

        /// <summary>
        /// Add a new cache entry.
        /// </summary>
        /// <param name="file">The name of the file to cache</param>
        /// <param name="entry">The entry to store</param>
        /// <returns>true, if the entry was added successfully</returns>
        /// <remarks>
        /// The cache will search for matching entries before the
        /// rewriters do their work. So you need to specify the unrewritten
        /// URL of the file to cache.
        /// 
        /// If the cache is nearly full or has the maximum number of entries
        /// reached, the entry won't be added to the cache.
        /// </remarks>
        public bool Add(string file, CachedResponse entry)
        {
            if (_Cache.ContainsKey(file)) return false;
            // entry limit
            if (_Cache.Count >= _MaxEntries) return false;
            // size limit
            if (_CurrentSize + entry.Data.Length + entry.CompressedData.Length >= _MaxSize) return false;
            // add the entry
            _Cache[file] = entry;
            _CurrentSize += entry.Data.Length + entry.CompressedData.Length;
            return true;
        }

        /// <summary>
        /// Remove a cache entry.
        /// </summary>
        /// <param name="file">The URL of the file to remove</param>
        /// <remarks>
        /// Use this method if you want to force the removal of an entry.
        /// Cache entries should rather expire than be removed manually.
        /// </remarks>
        public void Remove(string file)
        {
            if (_Cache.ContainsKey(file))
            {
                _CurrentSize = _CurrentSize - _Cache[file].Data.Length - _Cache[file].CompressedData.Length;
                _Cache.Remove(file);
            }
        }

        /// <summary>
        /// Try to find a matching cached entry.
        /// </summary>
        /// <param name="request">The request to handle</param>
        /// <param name="response">The response to write to</param>
        /// <param name="info">Information about the user's session</param>
        /// <returns>true, if the cache handled the request</returns>
        public bool HandleRequest(HttpRequest request, HttpResponse response, AuthorizationInfo info)
        {
            if (_Cache.ContainsKey(request.File))
            {
                CachedResponse resp = _Cache[request.File];
                if (resp.IsApplicable(request, info))
                {
                    response.Send(resp);
                    return true;
                }
            }
            return false;
        }

        #endregion

    }

}
