namespace GenHTTP.Api.Content
{

    public interface IPageAdditionBuilder<out T>
    {

        T AddScript(string path, bool asynchronous = false);

        T AddStyle(string path);

    }

}
