namespace GenHTTP.Api.Content
{

    public interface IConcern : IHandler
    {

        IHandler Content { get; }

    }

}
