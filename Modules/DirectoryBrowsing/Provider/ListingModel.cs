using GenHTTP.Api.Content.IO;

namespace GenHTTP.Modules.DirectoryBrowsing.Provider;

public record ListingModel(IResourceContainer Container, bool HasParent);
