namespace ASA.Domain.ValueObjects;

public record KeyValueProperty(string Key, string Value, string? Description, bool? Include);