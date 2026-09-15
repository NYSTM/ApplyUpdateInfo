using System.Text.Json.Serialization;

namespace ApplyUpdateInfo;

public sealed class UpdateColumnDefinition
{
    [JsonPropertyName("dataType")]
    public string DataType { get; set; } = "String";

    [JsonPropertyName("format")]
    public string? Format { get; set; }

    [JsonPropertyName("nullable")]
    public bool Nullable { get; set; } = true;

    [JsonPropertyName("maxLength")]
    public int? MaxLength { get; set; }

    [JsonPropertyName("precision")]
    public byte? Precision { get; set; }

    [JsonPropertyName("scale")]
    public byte? Scale { get; set; }

    public static UpdateColumnDefinition FromType(Type? dataType, bool nullable = true, int? maxLength = null)
    {
        string logicalType = dataType switch
        {
            not null when dataType == typeof(bool) => "Boolean",
            not null when dataType == typeof(byte) || dataType == typeof(sbyte)
                || dataType == typeof(short) || dataType == typeof(ushort)
                || dataType == typeof(int) || dataType == typeof(uint) => "Int32",
            not null when dataType == typeof(long) || dataType == typeof(ulong) => "Int64",
            not null when dataType == typeof(float) => "Single",
            not null when dataType == typeof(double) => "Double",
            not null when dataType == typeof(decimal) => "Decimal",
            not null when dataType == typeof(DateTime) || dataType == typeof(DateTimeOffset) => "DateTime",
            not null when dataType == typeof(Guid) => "Guid",
            not null when dataType == typeof(byte[]) => "Binary",
            _ => "String"
        };

        return new UpdateColumnDefinition
        {
            DataType = logicalType,
            Nullable = nullable,
            MaxLength = maxLength
        };
    }
}
