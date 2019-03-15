using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

using GenHTTP.Api.SessionManagement;
using GenHTTP.Api.Caching;
using GenHTTP.Api.Http;

namespace GenHTTP.Api.Content
{

    /// <summary>
    /// Allows you to provide a whole folder with subfolders to
    /// the clients.
    /// </summary>
    public class FolderProvider : IContentProvider
    {
        private ProjectBase _Project;
        private string _VirtualPath;
        private string _LocalPath;
        private Regex _Regex;
        private long _CacheDownloadsSmallerThan = 0;
        private ushort _TTL = 5;
        private ushort _HTL = 50;

        #region Constructors

        /// <summary>
        /// Create a new folder provider.
        /// </summary>
        /// <param name="project">The project the provider relates to</param>
        /// <param name="virtualPath">The virtual path to provide (without the project name, MUST NOT begin or end with a slash)</param>
        /// <param name="localPath">The mapping to the local path (MUST end with a slash or backslash)</param>
        public FolderProvider(ProjectBase project, string virtualPath, string localPath)
        {
            _Project = project;
            _VirtualPath = virtualPath;
            _LocalPath = localPath;
            _Regex = new Regex(@"^/" + _Project.Name + "/" + _VirtualPath + @"/(.+)$");
        }

        #endregion

        #region get-/setters

        /// <summary>
        /// Specifies, whether small download should get cached, if
        /// their size in bytes is smaller than this value.
        /// </summary>
        /// <remarks>
        /// By default, the value of this property is set to 0.
        /// </remarks>
        public long CacheDownloadsSmallerThan
        {
            get { return _CacheDownloadsSmallerThan; }
            set { _CacheDownloadsSmallerThan = value; }
        }

        /// <summary>
        /// Specifies the number of intervalls, the file will remain
        /// in the project's cache.
        /// </summary>
        /// <remarks>
        /// By default, this value is set to 5.
        /// </remarks>
        public ushort CacheTTL
        {
            get { return _TTL; }
            set { _TTL = value; }
        }

        /// <summary>
        /// Specifies the number of this, the file will remain in the
        /// project's cache.
        /// </summary>
        /// <remarks>
        /// By default, this value is set to 50.
        /// </remarks>
        public ushort CacheHTL
        {
            get { return _HTL; }
            set { _HTL = value; }
        }

        #endregion

        #region Request handling

        /// <summary>
        /// Will always return false.
        /// </summary>
        public bool RequiresLogin
        {
            get { return false; }
        }

        /// <summary>
        /// Checks, whether the requested file is a local file.
        /// </summary>
        /// <param name="request">The request to check</param>
        /// <param name="info">Information about the user's session</param>
        /// <returns>true, if the provider can handle this request</returns>
        public bool IsResponsible(IHttpRequest request, AuthorizationInfo info)
        {
            Match m = _Regex.Match(request.File);
            if (!m.Success) return false;
            return File.Exists(_LocalPath + m.Groups[1].Value);
        }

        /// <summary>
        /// Send a file to the client.
        /// </summary>
        /// <param name="request">The request to handle</param>
        /// <param name="response">The response to write to</param>
        /// <param name="info">Information about the user's session</param>
        public void HandleRequest(IHttpRequest request, IHttpResponse response, AuthorizationInfo info)
        {
            Match m = _Regex.Match(request.File);
            Download d = new Download(_LocalPath + m.Groups[1].Value);
            response.Send(d);
            // should this response get cached?
            if (d.UncompressedLength < _CacheDownloadsSmallerThan)
            {
                CachedResponse entry = new CachedResponse(response, d.GetBytes(), null, _TTL, _HTL);
                _Project.Cache.Add(request.File, entry);
            }
        }

        #endregion

    }
}
