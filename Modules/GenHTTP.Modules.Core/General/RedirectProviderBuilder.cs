using System;
using System.Collections.Generic;
using System.Text;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Modules;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Core.General
{

    public class RedirectProviderBuilder : ContentBuilderBase
    {
        private bool _Temporary = false;

        private string? _Location;

        #region Functionality

        public RedirectProviderBuilder Location(string location)
        {
            _Location = location;
            return this;
        }

        public RedirectProviderBuilder Mode(bool temporary)
        {
            _Temporary = temporary;
            return this;
        }

        public override IContentProvider Build()
        {
            if (_Location == null)
            {
                throw new BuilderMissingPropertyException("Location");
            }

            return new RedirectProvider(_Location, _Temporary, _Modification);
        }
        
        #endregion

    }

}
