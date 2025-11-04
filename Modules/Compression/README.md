# GenHTTP.Modules.Compression

This package provides a concern that will compress
generated content based on the
algorithms supported by the client.

By default, `br`, `gzip` and `zstd` algorithms
are supported. The server will select the best
algorithm based on the `Accept-Encoding` header
sent by the client.

The concern will analyze the `Content-Type` of the response
to determine, whether the content should be compressed
(as there are file types that are already compressed).

For more details, please refer to [the documentation](https://genhttp.org/documentation/content/concerns/compression/).
