using GenHTTP.Api.Content;
using GenHTTP.Api.Content.IO;

namespace GenHTTP.Modules.Compression.PreCompression;

public sealed class PreCompressedResourceHandlerBuilder : IHandlerBuilder<PreCompressedResourceHandlerBuilder>
{
    private readonly List<IConcernBuilder> _concerns = [];

    private readonly List<ICompressionAlgorithm> _algorithms = [];

    private readonly IResourceTree _source;

    public PreCompressedResourceHandlerBuilder(IResourceTree source)
    {
        _source = source;
    }

    public PreCompressedResourceHandlerBuilder Add(ICompressionAlgorithm algorithm)
    {
        _algorithms.Add(algorithm);
        return this;
    }

    public PreCompressedResourceHandlerBuilder Add(IConcernBuilder concern)
    {
        _concerns.Add(concern);
        return this;
    }

    public IHandler Build()
    {
        return Concerns.Chain(_concerns, new PreCompressedResourceHandler(_source, _algorithms));
    }

}
