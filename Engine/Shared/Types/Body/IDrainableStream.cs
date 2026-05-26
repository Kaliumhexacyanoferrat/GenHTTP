namespace GenHTTP.Engine.Shared.Types.Body;

public interface IDrainableStream
{

    ValueTask DrainAsync(CancellationToken cancellationToken = default);

}
