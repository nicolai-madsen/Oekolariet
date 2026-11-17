using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Oekolariet.Infrastructure.Converters;

// Ensure that JSON is returned with a Z in the time component, to conform with formatting standards
public sealed class UtcDateTimeConverter() : ValueConverter<DateTime, DateTime>(
    value => value.Kind == DateTimeKind.Utc
    ? value : value.ToUniversalTime(),

    value => DateTime.SpecifyKind(value, DateTimeKind.Utc));
