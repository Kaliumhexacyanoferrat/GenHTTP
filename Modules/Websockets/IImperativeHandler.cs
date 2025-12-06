namespace GenHTTP.Modules.Websockets;

public interface IImperativeHandler
{

    ValueTask HandleAsync(IImperativeConnection connection);

}
