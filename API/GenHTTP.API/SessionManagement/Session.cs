using GenHTTP.Api.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenHTTP.Api.SessionManagement
{

    /// <summary>
    /// Represents a session, managed by the <see cref="SessionCollection" />.
    /// </summary>
    public class Session
    {
        private Setting _Section;
        private SessionManager _Manager;

        /// <summary>
        /// Create a new session object.
        /// </summary>
        /// <param name="section">The XML section of the original data source the session is assigned to</param>
        /// <param name="manager">The <see cref="SessionManager" /> this session depends on</param>
        internal Session(Setting section, SessionManager manager)
        {
            _Section = section;
            _Manager = manager;
        }

        #region get-/setters

        /// <summary>
        /// The session key of this session.
        /// </summary>
        /// <remarks>
        /// The session key is a hexadecimal number consisting of 128 digits.
        /// </remarks>
        public string Key
        {
            get { return _Section.Attributes["key"]; }
        }

        /// <summary>
        /// For free use.
        /// </summary>
        public Setting Tag
        {
            get { return _Section["tag"]; }
        }

        /// <summary>
        /// Determine, when the session was updated last.
        /// </summary>
        /// <remarks>
        /// The <see cref="SessionManager" /> will update this session, whenever
        /// a <see cref="HttpRequest" /> with the session key of this session is
        /// handled. You don't need to update it by yourself.
        /// </remarks>
        public DateTime Updated
        {
            get
            {
                DateTime unix = new DateTime(1970, 1, 1, 0, 0, 0);
                unix = unix.AddSeconds(Convert.ToUInt64(_Section.Attributes["updated"]));
                return unix;
            }
        }

        /// <summary>
        /// The ID of this user.
        /// </summary>
        public uint UserID
        {
            get { return _Section["user"].ConvertTo<uint>(0); }
            set
            {
                _Section["user"].Value = value.ToString();
                if (_Manager.AutoDump) _Manager.Save();
            }
        }

        #endregion

        /// <summary>
        /// Update this session.
        /// </summary>
        public void Update()
        {
            DateTime now = DateTime.Now;
            DateTime unix = new DateTime(1970, 1, 1, 0, 0, 0);
            _Section.Attributes["updated"] = Math.Round((now - unix).TotalSeconds, 0).ToString();
            if (_Manager.AutoDump) _Manager.Save();
        }

        /// <summary>
        /// Check, whether this session timed out.
        /// </summary>
        /// <param name="timeout">The maximum timespan in seconds since the last activity of the user</param>
        /// <returns>true, if this session has timed out</returns>
        public bool TimedOut(ulong timeout)
        {
            DateTime now = DateTime.Now;
            return (now - Updated).TotalSeconds > timeout;
        }

    }

}
