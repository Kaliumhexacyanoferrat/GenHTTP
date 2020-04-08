namespace GenHTTP.Api.Content
{

    public interface IHandlerBuilder
    {

        IHandler Build(IHandler parent);

    }

}
