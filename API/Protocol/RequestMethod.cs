namespace GenHTTP.Api.Protocol;

public enum RequestMethod
{
    Get,
    Head,
    Post,
    Put,
    Delete,
    Connect,
    Options,
    Trace,
    Patch,

    // if it cannot be parsed into one of the ones above
    Other

}
