using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Reflection;
using System.Text.Json.Nodes;

namespace GenHTTP.Modules.Mcp.Schema;

/// <summary>
/// Translates validation attributes (such as <see cref="RangeAttribute" />) into
/// the corresponding keywords of a JSON schema.
/// </summary>
internal static class Constraints
{

    internal static IEnumerable<ValidationAttribute> Find(params ICustomAttributeProvider?[] providers)
    {
        var found = new List<ValidationAttribute>();

        foreach (var provider in providers)
        {
            if (provider == null)
            {
                continue;
            }

            foreach (var attribute in provider.GetCustomAttributes(true))
            {
                if (attribute is ValidationAttribute validation && !found.Contains(validation))
                {
                    found.Add(validation);
                }
            }
        }

        return found;
    }

    internal static void Apply(JsonObject schema, IEnumerable<ValidationAttribute> attributes)
    {
        foreach (var attribute in attributes)
        {
            switch (attribute)
            {
                case RangeAttribute range:
                    SetBound(schema, range.MinimumIsExclusive ? "exclusiveMinimum" : "minimum", range.Minimum);
                    SetBound(schema, range.MaximumIsExclusive ? "exclusiveMaximum" : "maximum", range.Maximum);
                    break;

                case LengthAttribute length:
                    SetLength(schema, length.MinimumLength, length.MaximumLength);
                    break;

                case MinLengthAttribute minLength:
                    SetLength(schema, minLength.Length, null);
                    break;

                case MaxLengthAttribute { Length: > 0 } maxLength:
                    SetLength(schema, null, maxLength.Length);
                    break;

                case StringLengthAttribute stringLength:
                    SetLength(schema, stringLength.MinimumLength > 0 ? stringLength.MinimumLength : null, stringLength.MaximumLength);
                    break;

                case RegularExpressionAttribute expression:
                    schema["pattern"] = expression.Pattern;
                    break;

                case EmailAddressAttribute:
                    schema["format"] = "email";
                    break;

                case UrlAttribute:
                    schema["format"] = "uri";
                    break;

                case AllowedValuesAttribute allowed:
                    schema["enum"] = new JsonArray(allowed.Values.Select(v => JsonValue.Create(v)).ToArray<JsonNode?>());
                    break;
            }
        }
    }

    private static void SetBound(JsonObject schema, string keyword, object? value)
    {
        var number = value switch
        {
            int i => i,
            double d => d,
            string s when double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed) => parsed,
            _ => (double?)null
        };

        if (number != null)
        {
            schema[keyword] = number % 1 == 0 ? JsonValue.Create((long)number.Value) : JsonValue.Create(number.Value);
        }
    }

    private static void SetLength(JsonObject schema, int? minimum, int? maximum)
    {
        var isArray = HasType(schema, "array");

        if (!isArray && !HasType(schema, "string"))
        {
            return;
        }

        if (minimum != null)
        {
            schema[isArray ? "minItems" : "minLength"] = minimum.Value;
        }

        if (maximum != null)
        {
            schema[isArray ? "maxItems" : "maxLength"] = maximum.Value;
        }
    }

    private static bool HasType(JsonObject schema, string type) => schema["type"] switch
    {
        JsonValue value => value.TryGetValue<string>(out var name) && name == type,
        JsonArray types => types.Any(t => t?.GetValue<string>() == type),
        _ => false
    };

}
