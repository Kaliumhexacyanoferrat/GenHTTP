using System.Diagnostics.CodeAnalysis;

using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;

using GenHTTP.Modules.Conversion;
using GenHTTP.Modules.Conversion.Formatters;
using GenHTTP.Modules.Conversion.Serializers;
using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Reflection.Injectors;

namespace GenHTTP.Modules.Webservices.Provider;

public sealed class ServiceResourceBuilder : IHandlerBuilder<ServiceResourceBuilder>
{
    private object? _Instance;

    private IBuilder<SerializationRegistry>? _Serializers;

    private IBuilder<InjectionRegistry>? _Injectors;

    private IBuilder<FormatterRegistry>? _Formatters;

    private readonly List<IConcernBuilder> _Concerns = new();

    #region Functionality

    public ServiceResourceBuilder Type<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>() where T : new() => Instance(new T());

    public ServiceResourceBuilder Instance(object instance)
    {
        _Instance = instance;
        return this;
    }

    public ServiceResourceBuilder Serializers(IBuilder<SerializationRegistry> registry)
    {
        _Serializers = registry;
        return this;
    }

    public ServiceResourceBuilder Injectors(IBuilder<InjectionRegistry> registry)
    {
        _Injectors = registry;
        return this;
    }

    public ServiceResourceBuilder Formatters(IBuilder<FormatterRegistry> registry)
    {
        _Formatters = registry;
        return this;
    }

    public ServiceResourceBuilder Add(IConcernBuilder concern)
    {
        _Concerns.Add(concern);
        return this;
    }

    public IHandler Build(IHandler parent)
    {
        var serializers = (_Serializers ?? Serialization.Default()).Build();

        var injectors = (_Injectors ?? Injection.Default()).Build();

        var formatters = (_Formatters ?? Formatting.Default()).Build();

        var instance = _Instance ?? throw new BuilderMissingPropertyException("instance");

        return Concerns.Chain(parent, _Concerns, (p) => new ServiceResourceRouter(p, instance, serializers, injectors, formatters));
    }

    #endregion

}
