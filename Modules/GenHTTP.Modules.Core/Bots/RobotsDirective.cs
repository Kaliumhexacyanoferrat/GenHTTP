using System.Collections.Generic;

namespace GenHTTP.Modules.Core.Bots
{

    public class RobotsDirective
    {

        #region Get-/Setters

        public List<string> UserAgents { get; }

        public List<string> Allowed { get; }

        public List<string> Disallowed { get; }

        #endregion

        #region Initialization

        public RobotsDirective(List<string> userAgents, List<string> allowed, List<string> disallowed)
        {
            UserAgents = userAgents;

            Allowed = allowed;
            Disallowed = disallowed;
        }

        #endregion

    }

}
