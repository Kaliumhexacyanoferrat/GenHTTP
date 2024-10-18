namespace GenHTTP.Engine.Internal.Protocol.Parser;

internal enum ScannerMode
{
    Words,
    Path,
    HeaderKey,
    HeaderValue
}
