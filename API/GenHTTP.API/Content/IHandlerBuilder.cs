namespace GenHTTP.Api.Content
{

    public interface IHandlerBuilder 
    {

        IHandler Build(IHandler parent);
    
    }

    public interface IHandlerBuilder<TBuilder> : IHandlerBuilder where TBuilder : IHandlerBuilder<TBuilder>
    {

        TBuilder Add(IConcernBuilder concern);

    }

}
