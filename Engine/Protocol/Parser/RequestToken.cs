namespace GenHTTP.Engine.Protocol.Parser
{

    internal enum RequestToken
    {
        None,
        Word,
        Path,
        PathWithQuery,
        NewLine
    }

}
