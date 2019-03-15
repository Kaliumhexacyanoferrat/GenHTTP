using System;
using System.Collections.Generic;
using System.Text;

namespace GenHTTP.Api.SessionManagement
{

    /// <summary>
    /// Informs a web application about the status of a <see cref="SessionManager" />
    /// for a given <see cref="HttpRequest" />.
    /// </summary>
    public class AuthorizationInfo
    {
        private User _User;
        private bool _WrongPassword;
        private bool _WrongUsername;
        private Session _Session;

        internal AuthorizationInfo(User user, bool wrongPassword, bool wrongUsername, Session session)
        {
            _User = user;
            _WrongPassword = wrongPassword;
            _WrongUsername = wrongUsername;
            _Session = session;
        }

        #region get-/setters

        /// <summary>
        /// The current user for this request.
        /// </summary>
        public User User
        {
            get { return _User; }
        }

        /// <summary>
        /// Specifies, whether the given password has been wrong.
        /// </summary>
        public bool WrongPassword
        {
            get { return _WrongPassword; }
        }

        /// <summary>
        /// Specifies, whether the given username has been wrong.
        /// </summary>
        public bool WrongUsername
        {
            get { return _WrongUsername; }
        }

        /// <summary>
        /// The session assigned to the given <see cref="IHttpRequest" />. If the value of this
        /// property is null, there is no session for this user.
        /// </summary>
        public Session Session
        {
            get { return _Session; }
        }

        #endregion

    }

}
