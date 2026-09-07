using System.Text;

using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Compression.Providers;

namespace GenHTTP.Modules.Files.Multi;

/// <summary>
/// Shared precompressed-variant negotiation used by both the regular and the Ioxide asset handlers.
/// The sibling extension follows the algorithm's <see cref="ICompressionAlgorithm.FileExtension"/>
/// (e.g. a <c>.gz</c> file), while the resulting Content-Encoding follows the algorithm name (<c>gzip</c>).
/// </summary>
internal sealed class PreCompression
{
    private readonly SupportedCompression[] _algorithms;

    internal PreCompression(List<ICompressionAlgorithm> algorithms, char separator)
    {
        _algorithms = algorithms.Select(a =>
                                {
                                    var suffix = a.FileExtension;

                                    var extension = new byte[suffix.Length + 1];
                                    extension[0] = (byte)separator;
                                    Encoding.ASCII.GetBytes(suffix, extension.AsSpan(1));

                                    return new SupportedCompression(a, extension);
                                })
                                .OrderByDescending(a => (int)a.Algorithm.Priority)
                                .ToArray();
    }

    /// <summary>Whether any precompressed variant has been configured at all.</summary>
    internal bool Enabled => _algorithms.Length > 0;

    /// <summary>
    /// The configured variants the client accepts, best (highest priority) first. Returns a struct
    /// that is foreach-able without allocating an enumerator on the hot path.
    /// </summary>
    internal AcceptedVariants Accepted(IRequest request) => new(_algorithms, request);

    internal readonly struct AcceptedVariants
    {
        private readonly SupportedCompression[] _algorithms;

        private readonly HashSet<AlgorithmName>? _requested;

        internal AcceptedVariants(SupportedCompression[] algorithms, IRequest request)
        {
            _algorithms = algorithms;

            var header = request.Header.Headers.GetEntry(KnownHeaders.AcceptEncoding);

            _requested = header == null ? null : AcceptEncodingHeader.ParseSupported(header.Value);
        }

        public Enumerator GetEnumerator() => new(_algorithms, _requested);

        internal struct Enumerator
        {
            private readonly SupportedCompression[] _algorithms;

            private readonly HashSet<AlgorithmName>? _requested;

            private int _index;

            internal Enumerator(SupportedCompression[] algorithms, HashSet<AlgorithmName>? requested)
            {
                _algorithms = algorithms;
                _requested = requested;
                _index = -1;
            }

            public readonly SupportedCompression Current => _algorithms[_index];

            public bool MoveNext()
            {
                if (_requested == null)
                {
                    return false;
                }

                while (++_index < _algorithms.Length)
                {
                    if (_requested.Contains(_algorithms[_index].Algorithm.Name))
                    {
                        return true;
                    }
                }

                return false;
            }
        }
    }
}
