using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Web.API.Serializations;

public sealed class ValueObjectJsonConverterString<T> : JsonConverter<T> where T : class
{
    private readonly ConstructorInfo? _stringConstructor;

    public ValueObjectJsonConverterString()
    {
        _stringConstructor = typeof(T).GetConstructor(new[] { typeof(string) });
        if (_stringConstructor == null)
            throw new InvalidOperationException($"{typeof(T)} debe tener un constructor público que reciba un string.");
    }

    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();

        if (value is null)
            throw new JsonException($"El valor para {typeof(T)} no puede ser nulo.");

        return (T)_stringConstructor!.Invoke(new object[] { value });
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value?.ToString());
    }
}
