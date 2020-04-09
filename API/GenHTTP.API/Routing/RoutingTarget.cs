using System;

namespace GenHTTP.Api.Routing
{
    
    public class RoutingTarget
    {
        private int _Index = 0;

        #region Get-/Setters

        public WebPath Path { get; }

        public string? Current => (_Index < Path.Parts.Count) ? Path.Parts[_Index] : null;

        public bool Ended => (_Index >= Path.Parts.Count);

        #endregion

        #region Initialization

        public RoutingTarget(WebPath path)
        {
            Path = path;
        }

        #endregion

        #region Functionality

        public void Advance()
        {
            if (Ended)
            {
                throw new InvalidOperationException("Already at the end of the path");
            }
            
            _Index++;
        }

        #endregion

    }

}
