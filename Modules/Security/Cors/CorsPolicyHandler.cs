using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Basics;

namespace GenHTTP.Modules.Security.Cors
{

    public sealed class CorsPolicyHandler : IConcern
    {
        public const string ALLOW_ANY = "*";

        #region Get-/Setters

        public IHandler Content { get; }

        public IHandler Parent { get; }

        public OriginPolicy? DefaultPolicy { get; }

        public IDictionary<string, OriginPolicy?> AdditionalPolicies { get; }

        #endregion

        #region Initialization

        public CorsPolicyHandler(IHandler parent, Func<IHandler, IHandler> contentFactory,
                                 OriginPolicy? defaultPolicy, IDictionary<string, OriginPolicy?> additionalPolicies)
        {
            Parent = parent;
            Content = contentFactory(this);

            DefaultPolicy = defaultPolicy;
            AdditionalPolicies = additionalPolicies;
        }

        #endregion

        #region Functionality

        public ValueTask PrepareAsync() => Content.PrepareAsync();

        public IAsyncEnumerable<ContentElement> GetContentAsync(IRequest request) => Content.GetContentAsync(request);

        public async ValueTask<IResponse?> HandleAsync(IRequest request)
        {
            var (origin, policy) = GetPolicy(request);

            IResponse? response;

            if (request.HasType(RequestMethod.OPTIONS))
            {
                response = request.Respond()
                                  .Status(ResponseStatus.NoContent)
                                  .Build();
            }
            else
            {
                response = await Content.HandleAsync(request).ConfigureAwait(false);
            }

            if ((response is not null) && (policy is not null))
            {
                ConfigureResponse(response, origin, policy);
            }

            return response;
        }

        private static void ConfigureResponse(IResponse response, string origin, OriginPolicy policy)
        {
            response.Headers["Access-Control-Allow-Origin"] = origin;

            if (HasValue(policy.AllowedMethods))
            {
                response.Headers["Access-Control-Allow-Methods"] = GetListOrWildcard(policy.AllowedMethods);
            }

            if (HasValue(policy.AllowedHeaders))
            {
                response.Headers["Access-Control-Allow-Headers"] = GetListOrWildcard(policy.AllowedHeaders);
            }

            if (HasValue(policy.ExposedHeaders))
            {
                response.Headers["Access-Control-Expose-Headers"] = GetListOrWildcard(policy.ExposedHeaders);
            }

            if (policy.AllowCredentials)
            {
                response.Headers["Access-Control-Allow-Credentials"] = "true";
            }

            response.Headers["Access-Control-Max-Age"] = policy.MaxAge.ToString();

            if (origin != ALLOW_ANY)
            {
                response.Headers["Vary"] = "Origin";
            }
        }

        private (string origin, OriginPolicy? policy) GetPolicy(IRequest request)
        {
            var origin = request["Origin"];

            if (origin is not null)
            {
                if (AdditionalPolicies.TryGetValue(origin, out var policy))
                {
                    return (origin, policy);
                }
            }

            return (origin ?? ALLOW_ANY, DefaultPolicy);
        }

        private static string GetListOrWildcard(List<string>? values)
        {
            if (values is not null)
            {
                return string.Join(", ", values);
            }

            return ALLOW_ANY;
        }

        private static string GetListOrWildcard(List<FlexibleRequestMethod>? values)
        {
            if (values is not null)
            {
                return string.Join(", ", values.Select(v => v.RawMethod));
            }

            return ALLOW_ANY;
        }

        private static bool HasValue<T>(List<T>? list) => (list is null) || (list.Count > 0);

        #endregion

    }

}
