using GenHTTP.Api.Configuration;
using GenHTTP.Api.Http;
using GenHTTP.Api.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenHTTP.Api.SessionManagement
{

    /// <summary>
    /// Allows you to manage users, groups and sessions.
    /// </summary>
    public class SessionManager
    {
        private Setting _DataSource;
        private UserCollection _Users;
        private GroupCollection _Groups;
        private SessionCollection _Sessions;
        private bool _AutoDump = false;

        /// <summary>
        /// Create a new session manager.
        /// </summary>
        /// <remarks>
        /// Because this session manager is virtual, you will not be able to call the
        /// <see cref="Save" /> method. Use the <see cref="SaveAs" /> method instead.
        /// </remarks>
        public SessionManager()
        {
            _DataSource = new Setting("sessionmanager");
            Timeout = 86400;
            InitCollections();
        }

        /// <summary>
        /// Create a new session manager.
        /// </summary>
        /// <param name="xmlSource">The XML source to read from</param>
        public SessionManager(string xmlSource)
        {
            _DataSource = Setting.FromXml(xmlSource);
            InitCollections();
        }

        private void InitCollections()
        {
            _Users = new UserCollection(this);
            _Groups = new GroupCollection(this);
            _Sessions = new SessionCollection(this);
        }

        #region internal get-/setters

        /// <summary>
        /// The data source for this manager.
        /// </summary>
        internal Setting DataSource
        {
            get
            {
                return _DataSource;
            }
        }

        /// <summary>
        /// The sessions which are managed by this class.
        /// </summary>
        internal SessionCollection Sessions
        {
            get { return _Sessions; }
        }

        #endregion


        #region get-/setters

        /// <summary>
        /// If you enable this setting, the session manager will
        /// dump the data source on every little change.
        /// </summary>
        /// <remarks>
        /// Enabling this setting is only recommended for very small web applications (with less then 10 users).
        /// It's recommended to add the <see cref="Save" /> method of the manager to a chronjob which dumps the 
        /// data in RAM regularly.
        /// </remarks>
        public bool AutoDump
        {
            get
            {
                return _AutoDump;
            }
            set
            {
                _AutoDump = value;
                if (_AutoDump) Save();
            }
        }

        /// <summary>
        /// The groups managed by the session manager.
        /// </summary>
        public GroupCollection Groups
        {
            get { return _Groups; }
        }

        /// <summary>
        /// The users managed by the session manager.
        /// </summary>
        public UserCollection Users
        {
            get { return _Users; }
        }

        /// <summary>
        /// Inactive sessions will be deleted after this time (in seconds)
        /// by the session manager.
        /// </summary>
        /// <remarks>
        /// Standard value: 84600 seconds, 1 day.
        /// </remarks>
        public ulong Timeout
        {
            get { return Convert.ToUInt64(_DataSource.Attributes["timeout"]); }
            set { _DataSource.Attributes["timeout"] = value.ToString(); }
        }

        /// <summary>
        /// The name to use for the session cookies.
        /// </summary>
        public string CookieName
        {
            get { return _DataSource.Attributes["cookie"]; }
            set { _DataSource.Attributes["cookie"] = value; }
        }

        #endregion

        /// <summary>
        /// Dump the data of the session manager to the source file.
        /// </summary>
        public void Save()
        {
            _DataSource.ToXml();
        }

        /// <summary>
        /// Save the data of the session manager to the given XML file.
        /// </summary>
        /// <param name="xmlFile">The file location to write to</param>
        public void SaveAs(string xmlFile)
        {
            _DataSource.ToXml(xmlFile);
        }

        /// <summary>
        /// Determine the authorization status for a given <see cref="HttpRequest" />.
        /// This method will retrieve the matching session for this request if there is one
        /// and will login the user if there is a login request.
        /// </summary>
        /// <param name="request">The request to check</param>
        /// <param name="response">The response to write the authentification cookie to</param>
        /// <returns>An <see cref="AuthorizationInfo" /> object with information about the current session</returns>
        public AuthorizationInfo CheckSession(IHttpRequest request, IHttpResponse response)
        {
            // check whether the user should get logged in
            if (request.PostFields.ContainsKey("Username") && request.PostFields.ContainsKey("Password"))
            {
                string password = Hash.HashString(request.PostFields["Password"]);
                User user = Users[request.PostFields["Username"]];
                // wrong username?
                if (user == null) return new AuthorizationInfo(Users.DefaultUser, false, true, null);
                // wrong password?
                if (user.PasswordHash != password) return new AuthorizationInfo(Users.DefaultUser, true, false, null);
                // successful login
                Session session = _Sessions.Add(user.ID);
                response.Header.AddCookie(new HttpCookie(CookieName, session.Key));
                return new AuthorizationInfo(user, false, false, session);
            }
            // delete old sessions
            List<Session> toDelete = new List<Session>();
            foreach (Session session in _Sessions)
            {
                if (session.TimedOut(Timeout)) toDelete.Add(session);
            }
            foreach (Session session in toDelete) _Sessions.Remove(session);
            // check the session of the current user
            string key = null;
            if (request.PostFields.ContainsKey(CookieName)) key = request.PostFields[CookieName];
            if (request.GetFields.ContainsKey(CookieName)) key = request.GetFields[CookieName];
            if (request.Cookies.Exists(CookieName)) key = request.Cookies[CookieName].Value;
            if (key == null || !_Sessions.Exists(key))
            {
                // no session found
                return new AuthorizationInfo(Users.DefaultUser, false, false, null);
            }
            // found a valid session
            Session foundSession = _Sessions[key];
            if (foundSession != null)
            {
                foundSession.Update();
                // get user
                User usr = Users[foundSession.UserID];
                usr.LastActivity = DateTime.Now;
                return new AuthorizationInfo(usr, false, false, foundSession);
            }
            else
            {
                return new AuthorizationInfo(Users.DefaultUser, false, false, null);
            }
        }

        /// <summary>
        /// Check, whether the given user can login on this system.
        /// </summary>
        /// <remarks>
        /// This method supports user managament without HTTP requests, e.g. for remoting.
        /// </remarks>
        /// <param name="userName">The name of the user to login</param>
        /// <param name="password">The password of the user to login</param>
        /// <returns>True, if the user can login</returns>
        public bool CanLogin(string userName, string password)
        {
            User user = Users[userName];
            if (user != null)
            {
                if (user.PasswordHash == Hash.HashString(password)) return true;
            }
            return false;
        }
        
        /// <summary>
        /// Remove a session from this manager.
        /// </summary>
        /// <param name="session">The session to remove</param>
        public void RemoveSession(Session session)
        {
            _Sessions.Remove(session);
        }

    }
    
}
