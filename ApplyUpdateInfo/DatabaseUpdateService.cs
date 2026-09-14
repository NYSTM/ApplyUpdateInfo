using System.Data;
using System.Data.Common;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;

namespace ApplyUpdateInfo;

public sealed record DatabaseSettings(
    string ProviderInvariantName,
    string ConnectionString,
    string BackupDirectory = "backups",
    int MaxLoadRows = 10_000);

public sealed record DatabaseConnectionProfile(
    string Name,
    DatabaseSettings Settings,
    string Environment = "Development",
    string BackgroundColor = "#EAF3FF");
public sealed record OperationValidationResult(IReadOnlyList<string> Errors)
{
    public bool IsValid => Errors.Count == 0;
}

public sealed class DatabaseUpdateService
{
    private readonly DatabaseSettings _settings;
    private readonly DbProviderFactory _factory;

    public DatabaseUpdateService(DatabaseSettings settings)
    {
        _settings = settings;
        if (string.Equals(settings.ProviderInvariantName, "Microsoft.Data.Sqlite", StringComparison.OrdinalIgnoreCase))
        {
            DbProviderFactories.RegisterFactory("Microsoft.Data.Sqlite", SqliteFactory.Instance);
        }
        else if (string.Equals(settings.ProviderInvariantName, "Microsoft.Data.SqlClient", StringComparison.OrdinalIgnoreCase))
        {
            DbProviderFactories.RegisterFactory("Microsoft.Data.SqlClient", SqlClientFactory.Instance);
        }

        _factory = DbProviderFactories.GetFactory(settings.ProviderInvariantName);
    }

    public async Task<OperationValidationResult> ValidateOperationsAsync(
        string tableName,
        IEnumerable<UpdateOperation> operations,
        IReadOnlyList<string> primaryKeyNames,
        CancellationToken cancellationToken = default)
    {
        string safeTableName = ValidateIdentifier(tableName);
        List<UpdateOperation> selected = operations.Where(operation => operation.IsSelected).ToList();
        List<string> errors = [];
        if (selected.Count == 0)
        {
            errors.Add("適用対象が選択されていません。");
            return new(errors);
        }

        await using DbConnection connection = CreateConnection();
        await connection.OpenAsync(cancellationToken);
        for (int index = 0; index < selected.Count; index++)
        {
            UpdateOperation operation = selected[index];
            IReadOnlyDictionary<string, JsonElement> keys = operation.Type == "insert"
                ? GetInsertKeys(operation.Values, primaryKeyNames)
                : operation.Keys;
            if (keys.Count != primaryKeyNames.Count)
            {
                errors.Add($"{index + 1}件目（{operation.OperationLabel}）: 主キー値が不足しています。");
                continue;
            }

            int count = await CountMatchingRowsAsync(connection, safeTableName, keys, cancellationToken);
            if (operation.Type == "insert" && count != 0)
            {
                errors.Add($"{index + 1}件目（新規）: 同一キーのレコードが既に{count}件存在します。");
            }
            else if (operation.Type is "update" or "delete" && count != 1)
            {
                errors.Add($"{index + 1}件目（{operation.OperationLabel}）: 対象レコードが{count}件です（1件である必要があります）。");
            }
        }

        return new(errors);
    }

    public async Task TestConnectionAsync(CancellationToken cancellationToken = default)
    {
        await using DbConnection connection = CreateConnection();
        await connection.OpenAsync(cancellationToken);
    }

    public async Task<IReadOnlyDictionary<string, JsonElement>?> LoadCurrentValuesAsync(
        string tableName,
        UpdateOperation operation,
        CancellationToken cancellationToken = default)
    {
        if (operation.Type == "insert")
        {
            return null;
        }

        string safeTableName = ValidateIdentifier(tableName);
        await using DbConnection connection = CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using DbCommand command = connection.CreateCommand();
        command.CommandText = $"SELECT * FROM {QuoteIdentifier(safeTableName)} WHERE {BuildWhere(operation.Keys)}";
        command.CommandTimeout = 120;
        AddKeyParameters(command, operation.Keys);
        await using DbDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        Dictionary<string, JsonElement> values = new(StringComparer.OrdinalIgnoreCase);
        for (int index = 0; index < reader.FieldCount; index++)
        {
            values[reader.GetName(index)] = ToJsonElement(reader.GetValue(index));
        }

        return values;
    }

    private static IReadOnlyDictionary<string, JsonElement> GetInsertKeys(
        IReadOnlyDictionary<string, JsonElement> values,
        IReadOnlyList<string> primaryKeyNames)
    {
        Dictionary<string, JsonElement> keys = new(StringComparer.OrdinalIgnoreCase);
        foreach (string name in primaryKeyNames)
        {
            if (values.TryGetValue(name, out JsonElement value))
            {
                keys[name] = value;
            }
        }

        return keys;
    }

    private static async Task<int> CountMatchingRowsAsync(
        DbConnection connection,
        string tableName,
        IReadOnlyDictionary<string, JsonElement> keys,
        CancellationToken cancellationToken)
    {
        await using DbCommand command = connection.CreateCommand();
        command.CommandText = $"SELECT COUNT(*) FROM {QuoteIdentifier(tableName)} WHERE {BuildWhere(keys)}";
        command.CommandTimeout = 120;
        AddKeyParameters(command, keys);
        object? result = await command.ExecuteScalarAsync(cancellationToken);
        return Convert.ToInt32(result, System.Globalization.CultureInfo.InvariantCulture);
    }

    public async Task<DataTable> LoadTableAsync(string tableName, CancellationToken cancellationToken = default)
    {
        string safeTableName = ValidateIdentifier(tableName);
        long rowCount = await CountRowsAsync(safeTableName, cancellationToken);
        if (rowCount > _settings.MaxLoadRows)
        {
            throw new InvalidOperationException($"テーブル「{safeTableName}」には{rowCount:N0}件のレコードがあります。安全のため、最大{_settings.MaxLoadRows:N0}件までしか読み込めません。");
        }

        await using DbConnection connection = CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using DbCommand command = connection.CreateCommand();
        command.CommandText = $"SELECT * FROM {QuoteIdentifier(safeTableName)}";
        command.CommandTimeout = 120;
        await using DbDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        DataTable table = new();
        table.Load(reader);
        return table;
    }

    public async Task<long> CountRowsAsync(string tableName, CancellationToken cancellationToken = default)
    {
        string safeTableName = ValidateIdentifier(tableName);
        await using DbConnection connection = CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using DbCommand command = connection.CreateCommand();
        command.CommandText = $"SELECT COUNT(*) FROM {QuoteIdentifier(safeTableName)}";
        command.CommandTimeout = 120;
        object? result = await command.ExecuteScalarAsync(cancellationToken);
        return Convert.ToInt64(result, System.Globalization.CultureInfo.InvariantCulture);
    }

    public async Task<IReadOnlyList<string>> LoadTableNamesAsync(CancellationToken cancellationToken = default)
    {
        await using DbConnection connection = CreateConnection();
        await connection.OpenAsync(cancellationToken);
        if (string.Equals(_settings.ProviderInvariantName, "Microsoft.Data.Sqlite", StringComparison.OrdinalIgnoreCase))
        {
            await using DbCommand sqliteCommand = connection.CreateCommand();
            sqliteCommand.CommandText = "SELECT name FROM sqlite_master WHERE type = 'table' AND name NOT LIKE 'sqlite_%' ORDER BY name;";
            await using DbDataReader sqliteReader = await sqliteCommand.ExecuteReaderAsync(cancellationToken);
            List<string> names = [];
            while (await sqliteReader.ReadAsync(cancellationToken))
            {
                names.Add(sqliteReader.GetString(0));
            }

            return names;
        }

        if (string.Equals(_settings.ProviderInvariantName, "Microsoft.Data.SqlClient", StringComparison.OrdinalIgnoreCase))
        {
            await using DbCommand sqlServerCommand = connection.CreateCommand();
            sqlServerCommand.CommandText = """
                SELECT TABLE_SCHEMA, TABLE_NAME
                FROM INFORMATION_SCHEMA.TABLES
                WHERE TABLE_TYPE = 'BASE TABLE'
                ORDER BY TABLE_SCHEMA, TABLE_NAME;
                """;
            await using DbDataReader sqlServerReader = await sqlServerCommand.ExecuteReaderAsync(cancellationToken);
            List<string> names = [];
            while (await sqlServerReader.ReadAsync(cancellationToken))
            {
                names.Add($"{sqlServerReader.GetString(0)}.{sqlServerReader.GetString(1)}");
            }

            return names;
        }

        DataTable schema = await connection.GetSchemaAsync("Tables", cancellationToken: cancellationToken);
        return schema.Rows.Cast<DataRow>()
            .Where(row => string.Equals(row["TABLE_TYPE"]?.ToString(), "BASE TABLE", StringComparison.OrdinalIgnoreCase))
            .Select(row => row["TABLE_NAME"]?.ToString())
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Cast<string>()
            .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public async Task<IReadOnlyList<string>> LoadColumnNamesAsync(string tableName, CancellationToken cancellationToken = default)
    {
        string safeTableName = ValidateIdentifier(tableName);
        await using DbConnection connection = CreateConnection();
        await connection.OpenAsync(cancellationToken);
        (string? schemaName, string objectName) = SplitTableName(safeTableName);
        DataTable schema = await connection.GetSchemaAsync("Columns", [null, schemaName, objectName], cancellationToken);
        return schema.Rows.Cast<DataRow>()
            .Select(row => row["COLUMN_NAME"]?.ToString())
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Cast<string>()
            .ToArray();
    }

    public async Task<IReadOnlyList<string>> LoadPrimaryKeyNamesAsync(string tableName, CancellationToken cancellationToken = default)
    {
        string safeTableName = ValidateIdentifier(tableName);
        await using DbConnection connection = CreateConnection();
        await connection.OpenAsync(cancellationToken);

        if (string.Equals(_settings.ProviderInvariantName, "Microsoft.Data.Sqlite", StringComparison.OrdinalIgnoreCase))
        {
            await using DbCommand sqliteCommand = connection.CreateCommand();
            sqliteCommand.CommandText = $"PRAGMA table_info({QuoteIdentifier(safeTableName)});";
            await using DbDataReader sqliteReader = await sqliteCommand.ExecuteReaderAsync(cancellationToken);
            List<(int Order, string Name)> sqliteKeys = [];
            while (await sqliteReader.ReadAsync(cancellationToken))
            {
                int primaryKeyOrder = sqliteReader.GetInt32(sqliteReader.GetOrdinal("pk"));
                if (primaryKeyOrder > 0)
                {
                    sqliteKeys.Add((primaryKeyOrder, sqliteReader.GetString(sqliteReader.GetOrdinal("name"))));
                }
            }

            return sqliteKeys.OrderBy(item => item.Order).Select(item => item.Name).ToArray();
        }

        (string? schemaName, string objectName) = SplitTableName(safeTableName);
        if (string.Equals(_settings.ProviderInvariantName, "Microsoft.Data.SqlClient", StringComparison.OrdinalIgnoreCase))
        {
            await using DbCommand sqlServerCommand = connection.CreateCommand();
            sqlServerCommand.CommandText = """
                SELECT KU.COLUMN_NAME
                FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS AS TC
                INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE AS KU
                    ON TC.CONSTRAINT_NAME = KU.CONSTRAINT_NAME
                    AND TC.TABLE_SCHEMA = KU.TABLE_SCHEMA
                    AND TC.TABLE_NAME = KU.TABLE_NAME
                WHERE TC.CONSTRAINT_TYPE = 'PRIMARY KEY'
                    AND KU.TABLE_SCHEMA = @schemaName
                    AND KU.TABLE_NAME = @tableName
                ORDER BY KU.ORDINAL_POSITION;
                """;
            DbParameter schemaParameter = sqlServerCommand.CreateParameter();
            schemaParameter.ParameterName = "@schemaName";
            schemaParameter.Value = schemaName ?? "dbo";
            sqlServerCommand.Parameters.Add(schemaParameter);
            DbParameter tableParameter = sqlServerCommand.CreateParameter();
            tableParameter.ParameterName = "@tableName";
            tableParameter.Value = objectName;
            sqlServerCommand.Parameters.Add(tableParameter);

            List<string> sqlServerKeys = [];
            await using DbDataReader sqlServerReader = await sqlServerCommand.ExecuteReaderAsync(cancellationToken);
            while (await sqlServerReader.ReadAsync(cancellationToken))
            {
                sqlServerKeys.Add(sqlServerReader.GetString(0));
            }

            return sqlServerKeys;
        }

        DataTable schema = await connection.GetSchemaAsync("PrimaryKeys", [null, schemaName, objectName], cancellationToken);
        return schema.Rows.Cast<DataRow>()
            .OrderBy(row => row["ORDINAL"] is IConvertible value ? value.ToInt32(null) : 0)
            .Select(row => row["COLUMN_NAME"]?.ToString())
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Cast<string>()
            .ToArray();
    }

    public async Task<int> ApplyAsync(
        string tableName,
        IEnumerable<UpdateOperation> operations,
        DateTime? utcNow = null,
        CancellationToken cancellationToken = default)
    {
        string safeTableName = ValidateIdentifier(tableName);
        List<UpdateOperation> selected = operations.Where(operation => operation.IsSelected).ToList();
        if (selected.Count == 0)
        {
            return 0;
        }

        await using DbConnection connection = CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await CreateBackupAsync(connection, safeTableName, selected, cancellationToken);
        await using DbTransaction transaction = await connection.BeginTransactionAsync(cancellationToken);
        try
        {
            DateTime applyJapanTime = utcNow ?? TimeTokenService.GetJapanStandardTime();
            int affected = 0;
            foreach (UpdateOperation operation in selected)
            {
                await using DbCommand command = connection.CreateCommand();
                command.Transaction = transaction;
                command.CommandText = BuildCommandText(safeTableName, operation);
                AddParameters(command, operation, applyJapanTime);
                affected += await command.ExecuteNonQueryAsync(cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);
            return affected;
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
    }

    private async Task CreateBackupAsync(
        DbConnection connection,
        string tableName,
        IReadOnlyList<UpdateOperation> operations,
        CancellationToken cancellationToken)
    {
        List<UpdateOperation> backupTargets = operations
            .Where(operation => operation.Type is "update" or "delete")
            .ToList();
        if (backupTargets.Count == 0)
        {
            return;
        }

        UpdateBackupDocument backup = new() { TableName = tableName };
        foreach (UpdateOperation operation in backupTargets)
        {
            await using DbCommand command = connection.CreateCommand();
            command.CommandText = $"SELECT * FROM {QuoteIdentifier(tableName)} WHERE {BuildWhere(operation.Keys)}";
            AddKeyParameters(command, operation.Keys);
            await using DbDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
            if (!await reader.ReadAsync(cancellationToken))
            {
                throw new InvalidOperationException($"バックアップ対象のレコードが存在しません。操作: {operation.Type}");
            }

            Dictionary<string, JsonElement> values = new(StringComparer.OrdinalIgnoreCase);
            for (int index = 0; index < reader.FieldCount; index++)
            {
                values[reader.GetName(index)] = ToJsonElement(reader.GetValue(index));
            }

            backup.Records.Add(new UpdateBackupRecord
            {
                OperationType = operation.Type,
                Keys = new(operation.Keys, StringComparer.OrdinalIgnoreCase),
                Values = values
            });
        }

        string directory = Path.IsPathRooted(_settings.BackupDirectory)
            ? _settings.BackupDirectory
            : Path.Combine(AppContext.BaseDirectory, _settings.BackupDirectory);
        Directory.CreateDirectory(directory);
        DateTimeOffset japanTime = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(9));
        string fileName = $"{tableName.Replace('.', '_')}_{japanTime:yyyyMMdd_HHmmss_fff}_backup.json";
        string filePath = Path.Combine(directory, fileName);
        string temporaryPath = filePath + ".tmp";
        string json = JsonSerializer.Serialize(backup, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(temporaryPath, json, new System.Text.UTF8Encoding(false), cancellationToken);
        File.Move(temporaryPath, filePath);
    }

    private static void AddKeyParameters(DbCommand command, IReadOnlyDictionary<string, JsonElement> keys)
    {
        DateTime japanTime = TimeTokenService.GetJapanStandardTime();
        int index = 0;
        foreach (JsonElement value in keys.Values)
        {
            DbParameter parameter = command.CreateParameter();
            parameter.ParameterName = $"@p{index++}";
            parameter.Value = ToDbValue(value, japanTime);
            command.Parameters.Add(parameter);
        }
    }

    private static JsonElement ToJsonElement(object value)
    {
        return value == DBNull.Value
            ? JsonSerializer.SerializeToElement<object?>(null)
            : JsonSerializer.SerializeToElement(value, value.GetType());
    }

    private DbConnection CreateConnection()
    {
        DbConnection connection = _factory.CreateConnection()
            ?? throw new InvalidOperationException("DB接続を作成できません。");
        string connectionString = _settings.ConnectionString;
        if (string.Equals(_settings.ProviderInvariantName, "Microsoft.Data.Sqlite", StringComparison.OrdinalIgnoreCase))
        {
            SqliteConnectionStringBuilder builder = new(connectionString);
            if (!string.IsNullOrWhiteSpace(builder.DataSource) &&
                !string.Equals(builder.DataSource, ":memory:", StringComparison.OrdinalIgnoreCase))
            {
                builder.Mode = SqliteOpenMode.ReadWrite;
            }

            connectionString = builder.ConnectionString;
        }

        connection.ConnectionString = connectionString;
        return connection;
    }

    private string BuildCommandText(string tableName, UpdateOperation operation)
    {
        return operation.Type switch
        {
            "insert" => BuildInsert(tableName, operation.Values),
            "update" => BuildUpdate(tableName, operation.Values, operation.Keys),
            "delete" => $"DELETE FROM {QuoteIdentifier(tableName)} WHERE {BuildWhere(operation.Keys)}",
            _ => throw new InvalidOperationException($"未対応の操作種別です: {operation.Type}")
        };
    }

    private static string BuildInsert(string tableName, IReadOnlyDictionary<string, JsonElement> values)
    {
        EnsureValues(values);
        string columns = string.Join(", ", values.Keys.Select(QuoteIdentifier));
        string parameters = string.Join(", ", values.Keys.Select((_, index) => $"@p{index}"));
        return $"INSERT INTO {QuoteIdentifier(tableName)} ({columns}) VALUES ({parameters})";
    }

    private static string BuildUpdate(string tableName, IReadOnlyDictionary<string, JsonElement> values, IReadOnlyDictionary<string, JsonElement> keys)
    {
        EnsureValues(values);
        string assignments = string.Join(", ", values.Keys.Select((name, index) => $"{QuoteIdentifier(name)} = @p{index}"));
        return $"UPDATE {QuoteIdentifier(tableName)} SET {assignments} WHERE {BuildWhere(keys, values.Count)}";
    }

    private static string BuildWhere(IReadOnlyDictionary<string, JsonElement> keys, int parameterOffset = 0)
    {
        EnsureValues(keys);
        return string.Join(" AND ", keys.Keys.Select((name, index) => $"{QuoteIdentifier(name)} = @p{index + parameterOffset}"));
    }

    private static void AddParameters(DbCommand command, UpdateOperation operation, DateTime utcNow)
    {
        IEnumerable<JsonElement> values = operation.Type == "delete"
            ? operation.Keys.Values
            : operation.Values.Values.Concat(operation.Type == "update" ? operation.Keys.Values : []);
        int index = 0;
        foreach (JsonElement value in values)
        {
            DbParameter parameter = command.CreateParameter();
            parameter.ParameterName = $"@p{index++}";
            parameter.Value = ToDbValue(value, utcNow);
            command.Parameters.Add(parameter);
        }
    }

    private static object ToDbValue(JsonElement value, DateTime utcNow)
    {
        if (value.ValueKind == JsonValueKind.String && string.Equals(value.GetString(), "$now", StringComparison.Ordinal))
        {
            return utcNow;
        }

        return value.ValueKind switch
        {
            JsonValueKind.Null or JsonValueKind.Undefined => DBNull.Value,
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Number when value.TryGetInt64(out long number) => number,
            JsonValueKind.Number => value.GetDecimal(),
            JsonValueKind.String when value.TryGetDateTime(out DateTime date) => date,
            JsonValueKind.String => value.GetString() ?? string.Empty,
            _ => value.GetRawText()
        };
    }

    private static void EnsureValues(IReadOnlyDictionary<string, JsonElement> values)
    {
        if (values.Count == 0 || values.Keys.Any(name => !IsIdentifier(name)))
        {
            throw new InvalidOperationException("カラム名が不正、または値が指定されていません。");
        }
    }

    private static string ValidateIdentifier(string value)
    {
        if (!IsIdentifier(value))
        {
            throw new ArgumentException($"テーブル名が不正です: {value}");
        }

        return value;
    }

    private static bool IsIdentifier(string value) => !string.IsNullOrWhiteSpace(value) && value.All(character => char.IsLetterOrDigit(character) || character is '_' or '.');

    private static (string? SchemaName, string ObjectName) SplitTableName(string value)
    {
        string[] parts = value.Split('.', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length == 2 ? (parts[0], parts[1]) : (null, value);
    }

    private static string QuoteIdentifier(string value) => string.Join('.', value.Split('.').Select(part => $"[{part.Replace("]", "]]", StringComparison.Ordinal)}]"));
}
