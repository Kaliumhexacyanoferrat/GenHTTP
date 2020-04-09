using GenHTTP.Modules.Core;
using GenHTTP.Modules.Scriban;

using GenHTTP.Examples.Examples.Infrastructure;
using GenHTTP.Examples.Examples.Webservices;
using GenHTTP.Examples.Examples.Listing;
using GenHTTP.Api.Content;

namespace GenHTTP.Examples
{

    public static class Project
    {

        public static IHandlerBuilder Build()
        {
            var index = ModScriban.Page(Data.FromResource("Index.html"))
                                  .Title("Examples");

            return Layout.Create()
                         .Add("plaintext", Content.From("Hello, World!"))
                         .Add("infrastructure", InfrastructureExamples.Create())
                         .Add("webservice", WebserviceExamples.Create())
                         .Add("listing", ListingExamples.Create())
                         .Index(index);
        }

    }

}
