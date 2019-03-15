using GenHTTP.Api.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenHTTP.Api.SessionManagement
{

    internal class SessionCollection
    {
        private SessionManager _Manager;
        private Dictionary<string, Session> _Sessions;

        internal SessionCollection(SessionManager manager)
        {
            _Manager = manager;
            _Sessions = new Dictionary<string, Session>();
            foreach (Setting session in _Manager.DataSource["sessions"].Children) _Sessions.Add(session.Attributes["key"], new Session(session, _Manager));
        }

        internal bool Exists(string key)
        {
            return _Sessions.ContainsKey(key);
        }

        internal void Remove(Session session)
        {
            if (!Exists(session.Key)) return;
            _Sessions.Remove(session.Key);
            _Manager.DataSource["sessions"].Children.Remove("session", "key", session.Key);
            if (_Manager.AutoDump) _Manager.Save();
        }

        internal Session Add(uint userID)
        {
            // generate random session key
            Random rnd = new Random();
            string key = "";
            do
            {
                key = "";
                for (int i = 0; i < 128; i++)
                { // 16^128 possible keys
                    key += rnd.Next(1, 16).ToString("X");
                }
            } while (this[key] != null); // find an unused session key
                                         // create setting structure
            Setting setting = new Setting("session");
            setting.Attributes["key"] = key;
            setting["user"].Value = userID.ToString();
            _Manager.DataSource["sessions"].Children.Add(setting);
            // generate session object
            Session session = new Session(setting, _Manager);
            session.Update();
            _Sessions.Add(key, session);
            if (_Manager.AutoDump) _Manager.Save();
            return session;
        }

        internal Session this[string key]
        {
            get { return (Exists(key)) ? _Sessions[key] : null; }
        }

        internal Session this[uint userID]
        {
            get
            {
                foreach (Session session in _Sessions.Values)
                {
                    if (session.UserID == userID) return session;
                }
                return null;
            }
        }

        public IEnumerator<Session> GetEnumerator()
        {
            return _Sessions.Values.GetEnumerator();
        }

    }

}
