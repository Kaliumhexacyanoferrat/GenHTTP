using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using System.Collections.Generic;

namespace GenHTTP.Modules.Core.Security
{

    public class StrictTransportConcern : IConcern
    {
        private const string HEADER = "Strict-Transport-Security";

        #region Get-/Setters

        public IHandler Parent { get; }

        public IHandler Content { get; }

        public StrictTransportPolicy Policy { get; }

        private string HeaderValue { get; }

        #endregion

        #region Initialization

        public StrictTransportConcern(IHandler parent, IHandler content, StrictTransportPolicy policy)
        {
            Parent = Parent;
            Content = Content;

            Policy = policy;
            HeaderValue = GetPolicyHeader();
        }

        #endregion

        #region Functionality

        public IResponse? Handle(IRequest request)
        {
            var response = Content.Handle(request);

            if (response != null)
            {
                if (request.EndPoint.Secure)
                {
                    if (!response.Headers.ContainsKey(HEADER))
                    {
                        response[HEADER] = HeaderValue;
                    }
                }
            }

            return response;
        }

        private string GetPolicyHeader()
        {
            var seconds = (int)Policy.MaximumAge.TotalSeconds;

            var result = $"max-age={seconds}";

            if (Policy.IncludeSubdomains)
            {
                result += "; includeSubDomains";
            }

            if (Policy.Preload)
            {
                result += "; preload";
            }

            return result;
        }

        public IEnumerable<ContentElement> GetContent(IRequest request)
        {
            return Parent.GetContent(request);
        }

        #endregion

    }

}
