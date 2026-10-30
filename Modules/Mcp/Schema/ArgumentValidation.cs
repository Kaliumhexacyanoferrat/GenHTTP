using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace GenHTTP.Modules.Mcp.Schema;

/// <summary>
/// Checks the arguments sent by a client against the validation attributes
/// that are also advertised in the schema of the tool.
/// </summary>
/// <remarks>
/// Validators are only created for types that declare constraints, so
/// arguments without any constraints are not inspected at all.
/// </remarks>
internal interface IArgumentValidator
{

    void Validate(object? value, string path, List<string> errors);

}

internal static class ArgumentValidation
{

    internal static IArgumentValidator? ForParameter(ParameterInfo parameter, JsonSerializerOptions options)
        => Combine(Constraints.Find(parameter), ForType(parameter.ParameterType, options, new Dictionary<Type, PendingValidator>()));

    internal static string Format(List<string> errors) => $"The arguments are invalid:\n- {string.Join("\n- ", errors)}";

    private static IArgumentValidator? ForType(Type type, JsonSerializerOptions options, Dictionary<Type, PendingValidator> pending)
    {
        if (pending.TryGetValue(type, out var known))
        {
            return known;
        }

        var typeInfo = options.GetTypeInfo(type);

        if (typeInfo.Kind == JsonTypeInfoKind.Enumerable && typeInfo.ElementType != null)
        {
            var element = ForType(typeInfo.ElementType, options, pending);

            return element != null ? new CollectionValidator(element) : null;
        }

        if (typeInfo.Kind != JsonTypeInfoKind.Object)
        {
            return null;
        }

        var placeholder = new PendingValidator();

        pending[type] = placeholder;

        var properties = new List<PropertyValidator>();

        foreach (var property in typeInfo.Properties)
        {
            if (property.Get == null)
            {
                continue;
            }

            var attributes = Constraints.Find(property.AttributeProvider, property.AssociatedParameter?.AttributeProvider);

            var validator = Combine(attributes, ForType(property.PropertyType, options, pending));

            if (validator != null)
            {
                properties.Add(new PropertyValidator(property.Name, property.Get, validator));
            }
        }

        placeholder.Target = properties.Count > 0 ? new ObjectValidator(properties) : null;

        return placeholder.Target;
    }

    private static IArgumentValidator? Combine(IEnumerable<ValidationAttribute> attributes, IArgumentValidator? inner)
    {
        var usable = attributes.Where(a => !a.RequiresValidationContext).ToArray();

        return usable.Length > 0 ? new AttributeValidator(usable, inner) : inner;
    }

    private static string Join(string path, string name) => path.Length > 0 ? $"{path}.{name}" : name;

    private static string Name(string path)
    {
        var index = path.LastIndexOf('.');

        return index >= 0 ? path[(index + 1)..] : path;
    }

    private sealed class AttributeValidator(ValidationAttribute[] attributes, IArgumentValidator? inner) : IArgumentValidator
    {

        public void Validate(object? value, string path, List<string> errors)
        {
            foreach (var attribute in attributes)
            {
                if (!attribute.IsValid(value))
                {
                    errors.Add($"{path}: {attribute.FormatErrorMessage(Name(path))}");
                }
            }

            inner?.Validate(value, path, errors);
        }

    }

    private sealed class ObjectValidator(List<PropertyValidator> properties) : IArgumentValidator
    {

        public void Validate(object? value, string path, List<string> errors)
        {
            if (value == null)
            {
                return;
            }

            foreach (var property in properties)
            {
                property.Validator.Validate(property.Getter(value), Join(path, property.Name), errors);
            }
        }

    }

    private sealed class CollectionValidator(IArgumentValidator element) : IArgumentValidator
    {

        public void Validate(object? value, string path, List<string> errors)
        {
            if (value is not IEnumerable items)
            {
                return;
            }

            var index = 0;

            foreach (var item in items)
            {
                element.Validate(item, $"{path}[{index++}]", errors);
            }
        }

    }

    private sealed class PendingValidator : IArgumentValidator
    {

        internal IArgumentValidator? Target { get; set; }

        public void Validate(object? value, string path, List<string> errors) => Target?.Validate(value, path, errors);

    }

    private sealed record PropertyValidator(string Name, Func<object, object?> Getter, IArgumentValidator Validator);

}
