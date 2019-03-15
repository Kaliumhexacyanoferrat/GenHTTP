using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using GenHTTP.Api.Abstraction;
using GenHTTP.Api.Compression;
using GenHTTP.Api.Http;
using GenHTTP.Api.SessionManagement;

namespace GenHTTP.Api.Caching
{

    /// <summary>
    /// Allows you to create a cache entry used by a project's
    /// cache.
    /// </summary>
    public class CachedResponse
    {
        private ushort _TTL;
        private ushort _HTL;
        private byte[] _Data;
        private byte[] _CompressedData;
        private CheckApplicability _Check;
        // extracted header information
        private Encoding _ContentEncoding;
        private LanguageInfo _ContentLanguage;
        private ContentType _ContentType;
        private DateTime _Expires, _Modified;
        private bool _DoesExpire, _IsModified;
        private bool _SendEncodingInfo;
        private ResponseType _Type;

        #region Constructors

        /// <summary>
        /// Create a new cached response.
        /// </summary>
        /// <param name="headerSource">The response header used to send the data</param>
        /// <param name="data">The uncompressed data to cache</param>
        /// <param name="check">The method used to check, whether the cache entry is applicable for a given <see cref="HttpRequest"/></param>
        /// <remarks>
        /// If you do not set the check parameter of the constructor, the
        /// entry will be sent without a applicability check.
        /// 
        /// By default, the cache entry will last one server tick or 10 hits.
        /// </remarks>
        public CachedResponse(IHttpResponse headerSource, byte[] data, CheckApplicability check)
        {
            _Data = data;
            _CompressedData = GzipCompression.Compress(data);
            _Check = check;
            _TTL = 1;
            _HTL = 10;
            // extract header info
            _ContentEncoding = headerSource.Header.ContentEncoding;
            _ContentLanguage = headerSource.Header.ContentLanguage;
            _ContentType = headerSource.Header.ContentType;
            _Expires = headerSource.Header.Expires;
            _DoesExpire = headerSource.Header.DoesExpire;
            _Modified = headerSource.Header.Modified;
            _IsModified = headerSource.Header.IsModified;
            _SendEncodingInfo = headerSource.Header.SendEncodingInfo;
            _Type = headerSource.Header.Type;
        }

        /// <summary>
        /// Create a new cached response.
        /// </summary>
        /// <param name="headerSource">The response header used to send the data</param>
        /// <param name="data">The uncompressed data to cache</param>
        /// <param name="check">The method used to check, whether the cache entry is applicable for a given <see cref="HttpRequest"/></param>
        /// <param name="htl">The number of hits the entry will remain in the project's cache</param>
        /// <param name="ttl">The number of project ticks the entry will remain in the project's cache</param>
        /// <remarks>
        /// If you do not set the check parameter of the constructor, the
        /// entry will be sent without a applicability check.
        /// </remarks>
        public CachedResponse(IHttpResponse headerSource, byte[] data, CheckApplicability check, ushort ttl, ushort htl) : this(headerSource, data, check)
        {
            _TTL = ttl;
            _HTL = htl;
        }

        #endregion

        #region Data providers

        /// <summary>
        /// Prepare a response so it will send an identical
        /// header to the client like the original did.
        /// </summary>
        /// <param name="response">The response to prepare</param>
        /// <remarks>
        /// This method won't set the HTTP protocol version or the
        /// compression algorithm, because these values depend on the client.
        /// </remarks>
        public void PrepareResponse(IHttpResponse response)
        {
            if (_HTL != 0) _HTL--;
            response.Header.ContentEncoding = _ContentEncoding;
            response.Header.ContentLanguage = _ContentLanguage;
            if (response.Header.ContentType != _ContentType) response.Header.ContentType = _ContentType;
            if (_DoesExpire) response.Header.Expires = _Expires;
            if (_IsModified) response.Header.Modified = _Modified;
            response.Header.SendEncodingInfo = _SendEncodingInfo;
            if (response.Header.Type != _Type) response.Header.Type = _Type;
        }

        /// <summary>
        /// The uncompressed data of the cache entry.
        /// </summary>
        public byte[] Data
        {
            get { return _Data; }
        }

        /// <summary>
        /// The compressed data of the cache entry.
        /// </summary>
        /// <remarks>
        /// The cache entry stores the data in both, uncompressed
        /// and compressed form, to increase performance and allow
        /// to serve clients which do not support compression.
        /// </remarks>
        public byte[] CompressedData
        {
            get { return _CompressedData; }
        }

        /// <summary>
        /// Checks, whether the cache entry is applicable for
        /// a given request.
        /// </summary>
        /// <param name="request">The request to check applicability for</param>
        /// <param name="info">Information about the client's session</param>
        /// <returns>true, if the entry can be used to send a response</returns>
        public bool IsApplicable(IHttpRequest request, AuthorizationInfo info)
        {
            if (_Check == null) return true;
            return _Check(request, info);
        }

        #endregion

        #region Life-cycle

        /// <summary>
        /// Decrease the ticks to live value.
        /// </summary>
        /// <returns>true, if the cache entry expired</returns>
        public bool Tick()
        {
            if (_HTL == 0 || _TTL == 0) return true;
            return (--_TTL == 0);
        }

        #endregion

    }

}
