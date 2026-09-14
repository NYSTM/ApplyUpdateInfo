using System.Text.Json;
using ApplyUpdateInfo;
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
}
