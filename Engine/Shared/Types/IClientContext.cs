using System.IO.Pipelines;

using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Engine.Shared.Types;

public interface IClientContext
{

    IServer Server { get; }

    Stream Stream { get; }

    PipeWriter Writer { get; }

}
