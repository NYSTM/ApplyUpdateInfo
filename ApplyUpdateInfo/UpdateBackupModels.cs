using System.Text.Json;
using System.Text.Json.Serialization;

namespace ApplyUpdateInfo;

public sealed class UpdateBackupDocument
{
    [JsonPropertyName("createdAtJst")]
    public DateTimeOffset CreatedAtJst { get; init; } = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(9));

    [JsonPropertyName("tableName")]
    public string TableName { get; init; } = string.Empty;

    [JsonPropertyName("columns")]
    public Dictionary<string, UpdateColumnDefinition> Columns { get; init; } = new(StringComparer.OrdinalIgnoreCase);

    [JsonPropertyName("records")]
    public List<UpdateBackupRecord> Records { get; init; } = [];
}

public sealed class UpdateBackupRecord
{
    [JsonPropertyName("operationType")]
    public string OperationType { get; init; } = string.Empty;

    [JsonPropertyName("keys")]
    public Dictionary<string, JsonElement> Keys { get; init; } = new(StringComparer.OrdinalIgnoreCase);

    [JsonPropertyName("values")]
    public Dictionary<string, JsonElement> Values { get; init; } = new(StringComparer.OrdinalIgnoreCase);
}
