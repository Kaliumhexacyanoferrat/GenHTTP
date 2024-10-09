using GenHTTP.Api.Content.IO;

namespace GenHTTP.Modules.DirectoryBrowsing.Provider
{

    public record class ListingModel(IResourceContainer Container, bool HasParent);

}