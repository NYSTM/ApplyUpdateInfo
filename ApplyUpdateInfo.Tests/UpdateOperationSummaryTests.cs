using System.Text.Json;
using System.Data;
using ApplyUpdateInfo;
using Microsoft.Data.Sqlite;
using Xunit;

namespace ApplyUpdateInfo.Tests;

public sealed class UpdateOperationSummaryTests
{
    [Fact]
    public void FromOperations_CountsOnlySelectedOperations()
    {
        List<UpdateOperation> operations =
        [
            new() { Type = "insert", IsSelected = true },
            new() { Type = "update", IsSelected = true },
            new() { Type = "delete", IsSelected = true },
            new() { Type = "delete", IsSelected = false }
        ];

        OperationSummary summary = OperationSummary.FromOperations(operations);

        Assert.Equal(1, summary.InsertCount);
        Assert.Equal(1, summary.UpdateCount);
        Assert.Equal(1, summary.DeleteCount);
        Assert.Equal(3, summary.TotalCount);
    }

    [Fact]
    public async Task BackupRestoreService_CreatesRestoreDocument()
    {
        string directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        string backupPath = Path.Combine(directory, "backup.json");
        string restorePath = Path.Combine(directory, "restore.json");
        string backupJson = "{\"tableName\":\"Users\",\"records\":[{\"operationType\":\"update\",\"keys\":{\"Id\":1},\"values\":{\"Id\":1,\"Name\":\"A\"}}]}";
        await File.WriteAllTextAsync(backupPath, backupJson);

        await BackupRestoreService.CreateRestoreJsonAsync(backupPath, restorePath);
        string restoreJson = await File.ReadAllTextAsync(restorePath);
        UpdateInfoDocument? document = JsonSerializer.Deserialize<UpdateInfoDocument>(restoreJson);

        Assert.NotNull(document);
        Assert.Equal("Users", document.TableName);
        Assert.Single(document.Operations);
        Assert.Equal("update", document.Operations[0].Type);
    }

    [Fact]
    public async Task UpdateInfoJsonService_RejectsNullOperations()
    {
        string filePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.json");
        await File.WriteAllTextAsync(filePath, "{\"tableName\":\"Users\",\"operations\":null}");

        try
        {
            UpdateInfoJsonService service = new();

            await Assert.ThrowsAsync<JsonException>(() => service.ReadAsync(filePath));
        }
        finally
        {
            File.Delete(filePath);
        }
    }

    [Fact]
    public void UpdateInfoExportService_RejectsNullPrimaryKey()
    {
        DataTable table = new();
        table.Columns.Add("Selected", typeof(bool));
        table.Columns.Add("OperationType", typeof(string));
        table.Columns.Add("Id", typeof(int));
        DataRow row = table.NewRow();
        row["Selected"] = true;
        row["OperationType"] = "insert";
        row["Id"] = DBNull.Value;
        table.Rows.Add(row);

        UpdateInfoExportService service = new();

        Assert.Throws<InvalidOperationException>(() => service.CreateUpdateJson("Users", table, ["Id"]));
    }

    [Fact]
    public void UpdateInfoExportService_WritesColumnTypesForNullValues()
    {
        DataTable table = new();
        table.Columns.Add("Selected", typeof(bool));
        table.Columns.Add("OperationType", typeof(string));
        table.Columns.Add("Id", typeof(int));
        table.Columns.Add("Amount", typeof(decimal));
        table.Columns.Add("Code", typeof(string));
        DataRow row = table.NewRow();
        row["Selected"] = true;
        row["OperationType"] = "insert";
        row["Id"] = 1;
        row["Amount"] = DBNull.Value;
        row["Code"] = "00123";
        table.Rows.Add(row);

        UpdateInfoExportService service = new();
        using JsonDocument document = JsonDocument.Parse(service.CreateUpdateJson("Orders", table, ["Id"]));

        Assert.Equal("Decimal", document.RootElement.GetProperty("columns").GetProperty("Amount").GetProperty("dataType").GetString());
        Assert.True(document.RootElement.GetProperty("columns").GetProperty("Amount").GetProperty("nullable").GetBoolean());
        Assert.Equal(JsonValueKind.Null, document.RootElement.GetProperty("operations")[0].GetProperty("values").GetProperty("Amount").ValueKind);
        Assert.Equal("00123", document.RootElement.GetProperty("operations")[0].GetProperty("values").GetProperty("Code").GetString());
    }

    [Fact]
    public async Task UpdateInfoJsonService_AllowsLegacyJsonWithoutColumns()
    {
        string filePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.json");
        await File.WriteAllTextAsync(filePath, "{\"tableName\":\"Users\",\"operations\":[{\"type\":\"update\",\"keys\":{\"Id\":1},\"values\":{\"Name\":\"A\"}}]}");

        try
        {
            UpdateInfoJsonService service = new();
            UpdateInfoDocument document = await service.ReadAsync(filePath);

            Assert.Empty(document.Columns);
            Assert.Equal("A", document.Operations[0].Values["Name"].GetString());
        }
        finally
        {
            File.Delete(filePath);
        }
    }

    [Fact]
    public async Task DatabaseUpdateService_PreservesNowLiteralForStringColumn()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            DatabaseSettings settings = new("Microsoft.Data.Sqlite", $"Data Source={databasePath}");
            await using (SqliteConnection connection = new(settings.ConnectionString))
            {
                await connection.OpenAsync();
                await using SqliteCommand command = connection.CreateCommand();
                command.CommandText = "CREATE TABLE ValuesTable (Id INTEGER PRIMARY KEY, Value TEXT NULL, FormattedValue TEXT NULL);";
                await command.ExecuteNonQueryAsync();
            }

            DatabaseUpdateService service = new(settings);
            UpdateOperation operation = new()
            {
                Type = "insert",
                Values = new Dictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase)
                {
                    ["Id"] = JsonSerializer.SerializeToElement(1),
                    ["Value"] = JsonSerializer.SerializeToElement("$now"),
                    ["FormattedValue"] = JsonSerializer.SerializeToElement("$now")
                }
            };
            Dictionary<string, UpdateColumnDefinition> columns = new(StringComparer.OrdinalIgnoreCase)
            {
                ["Id"] = new() { DataType = "Int32", Nullable = false },
                ["Value"] = new() { DataType = "String", Nullable = true },
                ["FormattedValue"] = new() { DataType = "String", Format = "yyyyMMddHHmmssfff", Nullable = true }
            };

            int affected = await service.ApplyAsync(
                "ValuesTable",
                [operation],
                utcNow: new DateTime(2026, 9, 15, 17, 55, 0),
                columnDefinitions: columns);

            Assert.Equal(1, affected);
            await using (SqliteConnection connection = new(settings.ConnectionString))
            {
                await connection.OpenAsync();
                await using SqliteCommand command = connection.CreateCommand();
                command.CommandText = "SELECT Value, FormattedValue FROM ValuesTable WHERE Id = 1";
                await using SqliteDataReader reader = await command.ExecuteReaderAsync();
                Assert.True(await reader.ReadAsync());
                Assert.Equal("$now", reader.GetString(0));
                Assert.Equal("20260915175500000", reader.GetString(1));
            }
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            File.Delete(databasePath);
        }
    }

    [Fact]
    public void UpdateInfoExportService_WritesConfiguredNowColumnAsToken()
    {
        DataTable table = new();
        table.Columns.Add("Selected", typeof(bool));
        table.Columns.Add("OperationType", typeof(string));
        table.Columns.Add("Id", typeof(int));
        table.Columns.Add("UpdatedAt", typeof(string));
        DataRow row = table.NewRow();
        row["Selected"] = true;
        row["OperationType"] = "update";
        row["Id"] = 1;
        row["UpdatedAt"] = "既存値";
        table.Rows.Add(row);

        UpdateInfoExportService service = new();
        using JsonDocument document = JsonDocument.Parse(service.CreateUpdateJson("Users", table, ["Id"], ["UpdatedAt"]));

        Assert.Equal("$now", document.RootElement.GetProperty("operations")[0].GetProperty("values").GetProperty("UpdatedAt").GetString());
    }

    [Fact]
    public void UpdateInfoExportService_PrefersConfiguredTableLayout()
    {
        DataTable table = new();
        table.Columns.Add("Selected", typeof(bool));
        table.Columns.Add("OperationType", typeof(string));
        table.Columns.Add("Id", typeof(int));
        table.Columns.Add("Amount", typeof(string));
        DataRow row = table.NewRow();
        row["Selected"] = true;
        row["OperationType"] = "insert";
        row["Id"] = 1;
        row["Amount"] = "12.34";
        table.Rows.Add(row);

        Dictionary<string, UpdateColumnDefinition> layout = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Id"] = new() { DataType = "Int32", Nullable = false },
            ["Amount"] = new() { DataType = "Decimal", Nullable = false, Precision = 10, Scale = 2 }
        };

        UpdateInfoExportService service = new();
        using JsonDocument document = JsonDocument.Parse(service.CreateUpdateJson("Orders", table, ["Id"], configuredColumnDefinitions: layout));

        JsonElement amount = document.RootElement.GetProperty("columns").GetProperty("Amount");
        Assert.Equal("Decimal", amount.GetProperty("dataType").GetString());
        Assert.Equal(10, amount.GetProperty("precision").GetInt32());
        Assert.Equal(2, amount.GetProperty("scale").GetInt32());
    }

    [Fact]
    public async Task TableLayoutFileService_SavesAndLoadsLayouts()
    {
        string filePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "tables.json");
        try
        {
            TableLayoutFileService service = new();
            Dictionary<string, IReadOnlyDictionary<string, UpdateColumnDefinition>> layouts = new(StringComparer.OrdinalIgnoreCase)
            {
                ["Users"] = new Dictionary<string, UpdateColumnDefinition>(StringComparer.OrdinalIgnoreCase)
                {
                    ["UpdatedAt"] = new() { DataType = "String", Format = "yyyyMMddHHmmssfff", Nullable = true }
                }
            };

            await service.SaveAsync(filePath, layouts);
            IReadOnlyDictionary<string, IReadOnlyDictionary<string, UpdateColumnDefinition>> loaded = service.Load(filePath);

            Assert.Equal("String", loaded["Users"]["UpdatedAt"].DataType);
            Assert.Equal("yyyyMMddHHmmssfff", loaded["Users"]["UpdatedAt"].Format);
        }
        finally
        {
            string? directory = Path.GetDirectoryName(filePath);
            if (directory is not null && Directory.Exists(directory))
            {
                Directory.Delete(directory, recursive: true);
            }
        }
    }
}
