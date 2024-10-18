namespace GenHTTP.Engine.Internal.Protocol.Parser;

internal enum RequestToken
{
    None,
    Word,
    Path,
    PathWithQuery,
    NewLine
}
