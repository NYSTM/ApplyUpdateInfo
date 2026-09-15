using System.Text.Json;

namespace ApplyUpdateInfo;

public sealed class AuditLogService
{
    private static readonly JsonSerializerOptions SerializerOptions = new() { WriteIndented = false };

    public async Task AppendAsync(string directory, AuditLogEntry entry, CancellationToken cancellationToken = default)
    {
        string targetDirectory = Path.IsPathRooted(directory)
            ? directory
            : Path.Combine(AppContext.BaseDirectory, directory);
        Directory.CreateDirectory(targetDirectory);
        string path = Path.Combine(targetDirectory, "audit.log.jsonl");
        string json = JsonSerializer.Serialize(entry, SerializerOptions) + Environment.NewLine;
        await File.AppendAllTextAsync(path, json, new System.Text.UTF8Encoding(false), cancellationToken);
    }
}

public static class BackupRestoreService
{
    public static async Task<string> CreateRestoreJsonAsync(string backupPath, string outputPath, CancellationToken cancellationToken = default)
    {
        await using FileStream stream = File.OpenRead(backupPath);
        UpdateBackupDocument? backup = await JsonSerializer.DeserializeAsync<UpdateBackupDocument>(stream, cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("バックアップJSONを読み込めません。");

        UpdateInfoDocument restore = new()
        {
            TableName = backup.TableName,
            Columns = backup.Columns,
            Operations = backup.Records.Select(record => new UpdateOperation
            {
                Type = record.OperationType,
                Keys = new(record.Keys, StringComparer.OrdinalIgnoreCase),
                Values = new(record.Values, StringComparer.OrdinalIgnoreCase),
                IsSelected = true
            }).ToList()
        };
        string json = JsonSerializer.Serialize(restore, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(outputPath, json, new System.Text.UTF8Encoding(false), cancellationToken);
        return outputPath;
    }
}
