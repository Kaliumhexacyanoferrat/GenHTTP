namespace GenHTTP.Api.Protocol
{

    public interface IRequestProperties
    {

        object this[string key] { get; set; }

        bool TryGet<T>(string key, out T entry);

    }

}
