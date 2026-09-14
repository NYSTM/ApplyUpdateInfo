using System.Globalization;
using System.Text.Json;

namespace ApplyUpdateInfo;

public sealed class ApplyPreviewDialog : Form
{
    public static readonly DateTime PreviewJapanTime = TimeTokenService.GetJapanStandardTime();

    public static bool HasChanges(ApplyPreviewRow row)
    {
        if (row.Operation.Type is "insert" or "delete" || row.CurrentValues is null)
        {
            return true;
        }

        foreach ((string columnName, JsonElement updated) in row.Operation.Values)
        {
            if (!row.CurrentValues.TryGetValue(columnName, out JsonElement current)
                || !AreEquivalent(current, updated))
            {
                return true;
            }
        }

        return false;
    }

    public ApplyPreviewDialog(
        string connectionName,
        string environment,
        string tableName,
        IEnumerable<ApplyPreviewRow> previewRows)
    {
        Text = "適用内容の確認";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.Sizable;
        MinimizeBox = false;
        MaximizeBox = true;
        ShowInTaskbar = false;
        ClientSize = new Size(980, 520);

        List<ApplyPreviewRow> rows = previewRows.ToList();
        OperationSummary summary = OperationSummary.FromOperations(rows.Select(row => row.Operation));
        Label header = new()
        {
            Dock = DockStyle.Top,
            Height = 72,
            Padding = new Padding(12),
            Text = $"接続先: {connectionName} ({environment}){Environment.NewLine}テーブル: {tableName}{Environment.NewLine}{summary}",
            BackColor = string.Equals(environment, "Production", StringComparison.OrdinalIgnoreCase)
                ? Color.MistyRose
                : Color.AliceBlue
        };

        DataGridView grid = new()
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AutoGenerateColumns = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        };
        grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "操作", DataPropertyName = "Operation", FillWeight = 12 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Keys", DataPropertyName = "Keys", FillWeight = 18 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "項目", DataPropertyName = "ColumnName", FillWeight = 18 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "現在値", DataPropertyName = "CurrentValue", FillWeight = 24 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "更新値", DataPropertyName = "UpdatedValue", FillWeight = 24 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "差分", DataPropertyName = "Difference", FillWeight = 14 });
        grid.DataSource = rows.Where(static row => row.Operation.IsSelected)
            .SelectMany(CreateDiffRows)
            .ToList();

        Button cancelButton = new() { Text = "キャンセル", DialogResult = DialogResult.Cancel, Dock = DockStyle.Right, Width = 100 };
        Button okButton = new() { Text = "適用へ進む", DialogResult = DialogResult.OK, Dock = DockStyle.Right, Width = 110 };
        Panel buttons = new() { Dock = DockStyle.Bottom, Height = 42, Padding = new Padding(6) };
        buttons.Controls.Add(cancelButton);
        buttons.Controls.Add(okButton);
        AcceptButton = okButton;
        CancelButton = cancelButton;
        Controls.Add(grid);
        Controls.Add(buttons);
        Controls.Add(header);
    }

    private static IEnumerable<object> CreateDiffRows(ApplyPreviewRow row)
    {
        IEnumerable<string> columnNames = row.Operation.Type switch
        {
            "insert" => row.Operation.Values.Keys,
            "delete" => row.CurrentValues?.Keys ?? row.Operation.Keys.Keys,
            _ => row.Operation.Values.Keys
        };

        foreach (string columnName in columnNames.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            JsonElement current = default;
            bool hasCurrent = row.CurrentValues?.TryGetValue(columnName, out current) == true;
            JsonElement updated = default;
            bool hasUpdated = row.Operation.Values.TryGetValue(columnName, out updated);
            string currentText = row.Operation.Type == "insert"
                ? "（新規）"
                : hasCurrent ? FormatValue(current, PreviewJapanTime) : "（未取得）";
            string updatedText = row.Operation.Type == "delete"
                ? "（削除）"
                : hasUpdated ? FormatValue(updated, PreviewJapanTime) : currentText;
            bool changed = row.Operation.Type is "insert" or "delete"
                || !hasCurrent
                || !hasUpdated
                || !AreEquivalent(current, updated);

            if (changed)
            {
                yield return new
                {
                    Operation = row.Operation.OperationLabel,
                    Keys = Serialize(row.Operation.Keys),
                    ColumnName = columnName,
                    CurrentValue = currentText,
                    UpdatedValue = updatedText,
                    Difference = row.Operation.Type switch
                    {
                        "insert" => "追加",
                        "delete" => "削除",
                        _ when !hasCurrent => "未取得",
                        _ => "変更"
                    }
                };
            }
        }
    }

    private static bool AreEquivalent(JsonElement current, JsonElement updated)
    {
        if (updated.ValueKind == JsonValueKind.String && string.Equals(updated.GetString(), "$now", StringComparison.Ordinal))
        {
            updated = JsonSerializer.SerializeToElement(PreviewJapanTime);
        }

        if (current.ValueKind == JsonValueKind.String && updated.ValueKind == JsonValueKind.String)
        {
            string currentText = current.GetString() ?? string.Empty;
            string updatedText = updated.GetString() ?? string.Empty;
            if (DateTime.TryParse(currentText, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out DateTime currentDate)
                && DateTime.TryParse(updatedText, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out DateTime updatedDate))
            {
                return currentDate == updatedDate;
            }

            return string.Equals(currentText, updatedText, StringComparison.Ordinal);
        }

        if (current.ValueKind == JsonValueKind.Number && updated.ValueKind == JsonValueKind.Number
            && current.TryGetDecimal(out decimal currentNumber)
            && updated.TryGetDecimal(out decimal updatedNumber))
        {
            return currentNumber == updatedNumber;
        }

        return current.GetRawText() == updated.GetRawText();
    }

    private static string Serialize(IReadOnlyDictionary<string, System.Text.Json.JsonElement> values) =>
        string.Join(", ", values.Select(pair => $"{pair.Key}={FormatValue(pair.Value, PreviewJapanTime)}"));

    private static string FormatValue(System.Text.Json.JsonElement value, DateTime utcNow)
    {
        if (value.ValueKind == JsonValueKind.String && string.Equals(value.GetString(), "$now", StringComparison.Ordinal))
        {
            return $"{utcNow:yyyy-MM-dd HH:mm:ss.fff} JST（$now）";
        }

        return value.ValueKind switch
        {
            System.Text.Json.JsonValueKind.Object => "{" + string.Join(", ", value.EnumerateObject()
                .Select(property => $"{property.Name}={FormatValue(property.Value, utcNow)}")) + "}",
            System.Text.Json.JsonValueKind.Array => "[" + string.Join(", ", value.EnumerateArray().Select(item => FormatValue(item, utcNow))) + "]",
            System.Text.Json.JsonValueKind.String => value.GetString() ?? string.Empty,
            System.Text.Json.JsonValueKind.Null => "null",
            _ => value.ToString()
        };
    }
}

public sealed record ApplyPreviewRow(
    UpdateOperation Operation,
    IReadOnlyDictionary<string, System.Text.Json.JsonElement>? CurrentValues);
