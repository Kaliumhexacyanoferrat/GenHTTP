using GenHTTP.Api.Content;
using GenHTTP.Api.Content.IO;

namespace GenHTTP.Modules.Compression.PreCompression;

public sealed class PreCompressedResourceHandlerBuilder : IHandlerBuilder<PreCompressedResourceHandlerBuilder>
{
    private readonly List<IConcernBuilder> _concerns = [];

    private readonly List<ICompressionAlgorithm> _algorithms = [];

    private readonly IResourceTree _source;

    private char _separator = '.';
    
    public PreCompressedResourceHandlerBuilder(IResourceTree source)
    {
        _source = source;
    }

    /// <summary>
    /// Sets the separator used to build the paths to the compressed files. Defaults to '.'.
    /// </summary>
    /// <param name="separator">The separator used to built file paths</param>
    /// <example>
    /// If your files are stored like "file.js+br", you can set this to '+'.
    /// </example>
    public PreCompressedResourceHandlerBuilder Separator(char separator)
    {
        _separator = separator;
        return this;
    }

    /// <summary>
    /// Registers a algorithm used to search for file names.
    /// </summary>
    /// <param name="algorithm">The algorithm to use for searching</param>
    /// <remarks>
    /// The handler will not use the compression or decompression features of the algorithm,
    /// but use the priority field to prefer algorithms with better compression ratios.
    /// </remarks>
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

    public IHandler Build() => Concerns.Chain(_concerns, new PreCompressedResourceHandler(_source, _algorithms, _separator));

}
