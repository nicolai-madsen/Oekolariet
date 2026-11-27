using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Oekolariet.Domain.Content;

namespace Oekolariet.Infrastructure.Converters;

public sealed class ExhibitionContentJsonConverter() : ValueConverter<ExhibitionContent, string>(
    content => JsonSerializer.Serialize(content, SerializerOptions),
    json => JsonSerializer.Deserialize<ExhibitionContent>(json, SerializerOptions)!)
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
        RespectNullableAnnotations = true, // "game": If null -> JsonException
        RespectRequiredConstructorParameters = true, // "game": If missing -> JsonException
    };
}
