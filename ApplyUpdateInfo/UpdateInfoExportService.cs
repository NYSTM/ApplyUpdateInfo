using System.Data;
using System.Text.Json;

namespace ApplyUpdateInfo;

public sealed class UpdateInfoExportService
{
    private static readonly JsonSerializerOptions SerializerOptions = new() { WriteIndented = true };

    public string CreateUpdateJson(string tableName, DataTable table, IReadOnlyCollection<string> primaryKeyNames)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tableName);
        ArgumentNullException.ThrowIfNull(table);
        if (primaryKeyNames.Count == 0)
        {
            throw new InvalidOperationException("主キーが取得できないため、更新用JSONを作成できません。");
        }

        List<UpdateOperation> operations = [];
        foreach (DataRow row in table.Rows.Cast<DataRow>().Where(row => row.Field<bool>("Selected")))
        {
            string operationType = GetOperationType(row["OperationType"]);
            Dictionary<string, JsonElement> values = new(StringComparer.OrdinalIgnoreCase);
            Dictionary<string, JsonElement> keys = new(StringComparer.OrdinalIgnoreCase);
            foreach (DataColumn column in table.Columns)
            {
                if (column.ColumnName is "Selected" or "OperationType")
                {
                    continue;
                }

                JsonElement value = ToJsonElement(row[column]);
                values[column.ColumnName] = value;
                if (primaryKeyNames.Contains(column.ColumnName, StringComparer.OrdinalIgnoreCase))
                {
                    keys[column.ColumnName] = value;
                }
            }

            if (keys.Count != primaryKeyNames.Count)
            {
                throw new InvalidOperationException("主キー値が空のレコードが含まれています。");
            }

            operations.Add(new UpdateOperation { Type = operationType, Values = values, Keys = keys });
        }

        UpdateInfoDocument document = new() { TableName = tableName, Operations = operations };
        return JsonSerializer.Serialize(document, SerializerOptions);
    }

    private static string GetOperationType(object value)
    {
        string operationType = value == DBNull.Value ? string.Empty : value?.ToString()?.Trim() ?? string.Empty;
        return operationType switch
        {
            "新規" or "insert" => "insert",
            "更新" or "update" => "update",
            "削除" or "delete" => "delete",
            _ => throw new InvalidOperationException($"OperationTypeが不正です: {operationType}")
        };
    }

    private static JsonElement ToJsonElement(object value)
    {
        return value == DBNull.Value
            ? JsonSerializer.SerializeToElement<object?>(null)
            : JsonSerializer.SerializeToElement(value, value.GetType());
    }
}