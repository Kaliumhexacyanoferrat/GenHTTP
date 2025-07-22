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

public sealed class ControllerBuilder : IHandlerBuilder<ControllerBuilder>
{
    private readonly List<IConcernBuilder> _Concerns = [];

    private Type? _Type;

    private Func<IRequest, ValueTask<object>>? _InstanceProvider;

    private IBuilder<FormatterRegistry>? _Formatters;

    private IBuilder<InjectionRegistry>? _Injection;

    private IBuilder<SerializationRegistry>? _Serializers;

    #region Functionality

    public ControllerBuilder Serializers(IBuilder<SerializationRegistry> registry)
    {
        _Serializers = registry;
        return this;
    }

    public ControllerBuilder Injectors(IBuilder<InjectionRegistry> registry)
    {
        _Injection = registry;
        return this;
    }

    public ControllerBuilder Formatters(IBuilder<FormatterRegistry> registry)
    {
        _Formatters = registry;
        return this;
    }

    public ControllerBuilder Type<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>() where T : new()
        => Instance(new T());

    public ControllerBuilder Instance(object instance)
    {
        _Type = instance.GetType();
        _InstanceProvider = (_) => ValueTask.FromResult(instance);

        return this;
    }

    public ControllerBuilder Add(IConcernBuilder concern)
    {
        _Concerns.Add(concern);
        return this;
    }

    public IHandler Build()
    {
        var serializers = (_Serializers ?? Serialization.Default()).Build();

        var injectors = (_Injection ?? Injection.Default()).Build();

        var formatters = (_Formatters ?? Formatting.Default()).Build();

        var instanceProvider = _InstanceProvider ?? throw new BuilderMissingPropertyException("Instance provider has not been set");

        var type = _Type ?? throw new BuilderMissingPropertyException("Type has not been set");

        var extensions = new MethodRegistry(serializers, injectors, formatters);

        return Concerns.Chain(_Concerns, new ControllerHandler(type, instanceProvider, extensions));
    }

    #endregion

}
