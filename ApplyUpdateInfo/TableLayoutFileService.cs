using System.Text.Json;

namespace ApplyUpdateInfo;

public sealed class TableLayoutFileService
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public IReadOnlyDictionary<string, IReadOnlyDictionary<string, UpdateColumnDefinition>> Load(string? filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return CreateEmptyLayouts();
        }

        string resolvedPath = ResolvePath(filePath);
        if (!File.Exists(resolvedPath))
        {
            return CreateEmptyLayouts();
        }

        string json = File.ReadAllText(resolvedPath);
        Dictionary<string, Dictionary<string, UpdateColumnDefinition>> layouts =
            JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, UpdateColumnDefinition>>>(json, SerializerOptions)
            ?? throw new JsonException("テーブルレイアウトファイルの内容が空です。");

        return ToReadOnlyLayouts(layouts);
    }

    public async Task SaveAsync(
        string filePath,
        IReadOnlyDictionary<string, IReadOnlyDictionary<string, UpdateColumnDefinition>> layouts,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        string resolvedPath = ResolvePath(filePath);
        string? directory = Path.GetDirectoryName(resolvedPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        Dictionary<string, Dictionary<string, UpdateColumnDefinition>> writableLayouts = layouts
            .ToDictionary(
                static table => table.Key,
                static table => new Dictionary<string, UpdateColumnDefinition>(table.Value, StringComparer.OrdinalIgnoreCase),
                StringComparer.OrdinalIgnoreCase);
        string json = JsonSerializer.Serialize(writableLayouts, SerializerOptions);
        await File.WriteAllTextAsync(resolvedPath, json, new System.Text.UTF8Encoding(false), cancellationToken);
    }

    public string ResolvePath(string filePath)
    {
        return Path.IsPathRooted(filePath)
            ? filePath
            : Path.Combine(AppContext.BaseDirectory, filePath);
    }

    private static IReadOnlyDictionary<string, IReadOnlyDictionary<string, UpdateColumnDefinition>> ToReadOnlyLayouts(
        IReadOnlyDictionary<string, Dictionary<string, UpdateColumnDefinition>> layouts)
    {
        return layouts.ToDictionary(
            static table => table.Key,
            static table => (IReadOnlyDictionary<string, UpdateColumnDefinition>)new Dictionary<string, UpdateColumnDefinition>(table.Value, StringComparer.OrdinalIgnoreCase),
            StringComparer.OrdinalIgnoreCase);
    }

    private static IReadOnlyDictionary<string, IReadOnlyDictionary<string, UpdateColumnDefinition>> CreateEmptyLayouts()
    {
        return new Dictionary<string, IReadOnlyDictionary<string, UpdateColumnDefinition>>(StringComparer.OrdinalIgnoreCase);
    }
}
