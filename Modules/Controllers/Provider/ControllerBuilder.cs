using System.Diagnostics.CodeAnalysis;

using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Conversion;
using GenHTTP.Modules.Conversion.Formatters;
using GenHTTP.Modules.Conversion.Serializers;
using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Reflection.Injectors;

namespace GenHTTP.Modules.Controllers.Provider;

public sealed class ControllerBuilder : IHandlerBuilder<ControllerBuilder>, IRegistryBuilder<ControllerBuilder>
{
    private readonly List<IConcernBuilder> _concerns = [];

    private Type? _type;

    private Func<IRequest, ValueTask<object>>? _instanceProvider;

    private IBuilder<FormatterRegistry>? _formatters;

    private IBuilder<InjectionRegistry>? _injection;

    private IBuilder<SerializationRegistry>? _serializers;

    #region Functionality

    public ControllerBuilder Serializers(IBuilder<SerializationRegistry> registry)
    {
        _serializers = registry;
        return this;
    }

    public ControllerBuilder Injectors(IBuilder<InjectionRegistry> registry)
    {
        _injection = registry;
        return this;
    }

    public ControllerBuilder Formatters(IBuilder<FormatterRegistry> registry)
    {
        _formatters = registry;
        return this;
    }

    public ControllerBuilder Type<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>() where T : new()
        => Instance(new T());

    public ControllerBuilder Type([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type type)
    {
        _type = type;
        return this;
    }

    public ControllerBuilder Instance(object instance)
    {
        _type = instance.GetType();
        _instanceProvider = (_) => ValueTask.FromResult(instance);

        return this;
    }

    public ControllerBuilder InstanceProvider(Func<IRequest, ValueTask<object>> provider)
    {
        _instanceProvider = provider;
        return this;
    }

    public ControllerBuilder Add(IConcernBuilder concern)
    {
        _concerns.Add(concern);
        return this;
    }

    public IHandler Build()
    {
        var serializers = (_serializers ?? Serialization.Default()).Build();

        var injectors = (_injection ?? Injection.Default()).Build();

        var formatters = (_formatters ?? Formatting.Default()).Build();

        var instanceProvider = _instanceProvider ?? throw new BuilderMissingPropertyException("Instance provider has not been set");

        var type = _type ?? throw new BuilderMissingPropertyException("Type has not been set");

        var extensions = new MethodRegistry(serializers, injectors, formatters);

        return Concerns.Chain(_concerns, new ControllerHandler(type, instanceProvider, extensions));
    }

    #endregion

}
