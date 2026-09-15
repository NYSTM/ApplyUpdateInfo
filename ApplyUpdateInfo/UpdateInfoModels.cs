using System.Text.Json;
using System.Text.Json.Serialization;

namespace ApplyUpdateInfo;

public sealed class UpdateInfoDocument
{
    [JsonPropertyName("tableName")]
    public string TableName { get; set; } = string.Empty;

    [JsonPropertyName("columns")]
    public Dictionary<string, UpdateColumnDefinition> Columns { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    [JsonPropertyName("operations")]
    public List<UpdateOperation> Operations { get; set; } = [];
}

public sealed class UpdateOperation
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("values")]
    public Dictionary<string, JsonElement> Values { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    [JsonPropertyName("keys")]
    public Dictionary<string, JsonElement> Keys { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    [JsonIgnore]
    public bool IsSelected { get; set; } = true;

    [JsonIgnore]
    public string OperationLabel => Type.ToLowerInvariant() switch
    {
        "insert" => "新規",
        "update" => "更新",
        "delete" => "削除",
        _ => Type
    };
}

public sealed class UpdateInfoJsonService
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip
    };

    public async Task<UpdateInfoDocument> ReadAsync(string filePath, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        await using FileStream stream = File.OpenRead(filePath);
        UpdateInfoDocument? document = await JsonSerializer.DeserializeAsync<UpdateInfoDocument>(stream, SerializerOptions, cancellationToken);
        return Validate(document);
    }

    private static UpdateInfoDocument Validate(UpdateInfoDocument? document)
    {
        if (document is null)
        {
            throw new JsonException("JSONの内容が空です。");
        }

        if (string.IsNullOrWhiteSpace(document.TableName))
        {
            throw new JsonException("tableNameは必須です。");
        }

        if (document.Columns is null)
        {
            throw new JsonException("columnsがnullです。");
        }

        if (document.Operations is null)
        {
            throw new JsonException("operationsは必須です。");
        }

        if (document.Operations.Count == 0)
        {
            throw new JsonException("operationsに適用対象がありません。");
        }

        foreach (UpdateOperation? operation in document.Operations)
        {
            if (operation is null)
            {
                throw new JsonException("operationsにnullの操作が含まれています。");
            }

            if (operation.Values is null)
            {
                throw new JsonException("操作のvaluesは必須です。");
            }

            if (operation.Keys is null)
            {
                throw new JsonException("操作のkeysは必須です。");
            }

            if (operation.Type is not ("insert" or "update" or "delete"))
            {
                throw new JsonException($"未対応の操作種別です: {operation.Type}");
            }

            if (operation.Type != "insert" && operation.Keys.Count == 0)
            {
                throw new JsonException($"{operation.Type}操作にはkeysが必要です。");
            }
        }

        return document;
    }
}
