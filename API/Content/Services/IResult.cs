using GenHTTP.Api.Protocol;

namespace GenHTTP.Api.Content.Services
{

    public interface IResult
    {

        object? Payload { get; }

        void Apply(IResponseBuilder builder);

    }

}
