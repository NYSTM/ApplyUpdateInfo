using System.ComponentModel;
using System.Data;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Data.Sqlite;

namespace ApplyUpdateInfo;

public partial class UpdateInfoForm : Form
{
    private readonly BindingList<UpdateOperationRow> _rows = [];
    private readonly UpdateInfoJsonService _jsonService = new();
    private readonly UpdateInfoExportService _exportService = new();
    private readonly AuditLogService _auditLogService = new();
    private DatabaseUpdateService? _databaseService;
    private UpdateInfoDocument? _document;
    private DataTable? _developmentTable;
    private DataTable? _jsonOperationTable;
    private string? _developmentTableName;
    private IReadOnlyList<string> _primaryKeyNames = [];
    private IReadOnlyList<DatabaseConnectionProfile> _connectionProfiles = [];
    private DatabaseConnectionProfile? _activeConnectionProfile;
    private bool _isContentChecked;

    public UpdateInfoForm()
    {
        InitializeComponent();
        Application.ThreadException += Application_ThreadException;
        FormClosed += UpdateInfoForm_FormClosed;
        updateGrid.DiagnosticLogger = AppendLaControlLog;
        updateGrid.CellContentClick += UpdateGrid_CellContentClick;
        updateGrid.CellEndEdit += UpdateGrid_CellEndEdit;
        AppendLaControlLog("診断ログを開始しました。");
        InvalidateContentCheck();
        updateGrid.DataSource = _rows;
        Load += UpdateInfoForm_Loaded;
    }

    private void AppendLaControlLog(string message)
    {
        if (IsDisposed)
        {
            return;
        }

        if (InvokeRequired)
        {
            BeginInvoke(AppendLaControlLog, message);
            return;
        }

        errorLogTextBox.AppendText($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [LaControl] {message}{Environment.NewLine}");
    }

    private void Application_ThreadException(object? sender, ThreadExceptionEventArgs e)
    {
        ShowError("画面処理中に例外が発生しました。", e.Exception);
    }

    private void UpdateInfoForm_FormClosed(object? sender, FormClosedEventArgs e)
    {
        Application.ThreadException -= Application_ThreadException;
    }

    private async void UpdateInfoForm_Loaded(object? sender, EventArgs e)
    {
        try
        {
            _connectionProfiles = LoadConnectionProfiles();
            connectionComboBox.DataSource = _connectionProfiles.ToList();
            connectionComboBox.DisplayMember = nameof(DatabaseConnectionProfile.Name);
            connectionComboBox.SelectedIndex = 0;
            _activeConnectionProfile = _connectionProfiles[0];
            ApplyConnectionAppearance(_activeConnectionProfile);
        }
        catch (Exception exception)
        {
            ShowError("接続先設定の読込に失敗しました。", exception);
            return;
        }

        await LoadTableTreeAsync();
        string? filePath = Environment.GetCommandLineArgs().Skip(1).FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(filePath) && File.Exists(filePath))
        {
            _ = LoadJsonAsync(filePath);
        }
    }

    private void FilterTextBox_TextChanged(object? sender, EventArgs e) => ApplyGridFilter();

    private void OperationFilterComboBox_SelectedIndexChanged(object? sender, EventArgs e) => ApplyGridFilter();

    private void ApplyGridFilter()
    {
        DataView? view = updateGrid.DataSource switch
        {
            DataView dataView => dataView,
            _ => null
        };
        if (view is null)
        {
            return;
        }

        List<string> filters = [];
        string operationFilter = operationFilterComboBox.SelectedItem?.ToString() ?? "すべて";
        if (operationFilter is not "すべて")
        {
            filters.Add($"OperationType = '{operationFilter.Replace("'", "''", StringComparison.Ordinal)}'");
        }

        string searchText = filterTextBox.Text.Trim();
        if (searchText.Length > 0)
        {
            string escaped = EscapeRowFilterText(searchText);
            filters.Add(string.Join(" OR ", view.Table.Columns.Cast<DataColumn>()
                .Select(column => $"Convert([{column.ColumnName.Replace("]", "]]", StringComparison.Ordinal)}], 'System.String') LIKE '%{escaped}%'")));
        }

        view.RowFilter = string.Join(" AND ", filters);
        RebindFilteredView(view);
    }

    private static string EscapeRowFilterText(string value)
    {
        return value
            .Replace("'", "''", StringComparison.Ordinal)
            .Replace("[", "[[]", StringComparison.Ordinal)
            .Replace("]", "[]]", StringComparison.Ordinal)
            .Replace("%", "[%]", StringComparison.Ordinal)
            .Replace("*", "[*]", StringComparison.Ordinal);
    }

    private void RebindFilteredView(DataView view)
    {
        updateGrid.DataSource = null;
        updateGrid.DataSource = view;
        ConfigureOperationTypeColumn();
        ApplyPrimaryKeyHeaderStyle(_primaryKeyNames);
    }

    private void SetOperationTypeButton_Click(object? sender, EventArgs e)
    {
        string operationType = operationTypeComboBox.SelectedItem?.ToString() ?? string.Empty;
        if (operationType.Length == 0)
        {
            MessageBox.Show(this, "一括変更する操作種別を選択してください。", "確認", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        foreach (DataGridViewRow gridRow in updateGrid.SelectedRows)
        {
            if (gridRow.DataBoundItem is DataRowView rowView && rowView.Row.Table.Columns.Contains("OperationType"))
            {
                rowView.Row["OperationType"] = operationType;
            }
        }

        InvalidateContentCheck();
        updateGrid.RefreshData();
    }

    private void AutoSizeColumnsButton_Click(object? sender, EventArgs e)
    {
        updateGrid.AutoSizeColumnsToContents();
        statusLabel.Text = "列幅を内容に合わせて自動調整しました。";
    }

    private void SetNowButton_Click(object? sender, EventArgs e)
    {
        if (_jsonOperationTable is null || updateGrid.CurrentCell is null)
        {
            MessageBox.Show(this, "先にJSONを読み込み、設定対象のセルを選択してください。", "確認", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        DataGridViewCell cell = updateGrid.CurrentCell;
        if (cell.RowIndex < 0 || cell.ColumnIndex < 0 || cell.OwningRow.DataBoundItem is not DataRowView rowView)
        {
            return;
        }

        string columnName = updateGrid.Columns[cell.ColumnIndex].DataPropertyName;
        if (columnName is "Selected" or "OperationType" || !rowView.Row.Table.Columns.Contains(columnName))
        {
            MessageBox.Show(this, "値を設定できるセルを選択してください。", "確認", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        if (_primaryKeyNames.Contains(columnName, StringComparer.OrdinalIgnoreCase))
        {
            MessageBox.Show(this, "主キー列には現在時刻を設定できません。", "確認", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        rowView.Row[columnName] = "$now";
        InvalidateContentCheck();
        updateGrid.RefreshData();
    }

    private async void SwitchConnectionButton_Click(object? sender, EventArgs e)
    {
        if (connectionComboBox.SelectedItem is not DatabaseConnectionProfile profile ||
            ReferenceEquals(profile, _activeConnectionProfile))
        {
            return;
        }

        if (MessageBox.Show(this, "接続先を切り替えると、現在の画面内容は破棄されます。続行しますか？", "接続先切替確認", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
        {
            connectionComboBox.SelectedItem = _activeConnectionProfile;
            return;
        }

        SetBusy(true);
        try
        {
            _activeConnectionProfile = profile;
            ApplyConnectionAppearance(profile);
            _databaseService = new DatabaseUpdateService(profile.Settings);
            _document = null;
            _developmentTable = null;
            _developmentTableName = null;
            _jsonOperationTable = null;
            _rows.Clear();
            InvalidateContentCheck();
            updateGrid.DataSource = _rows;
            await LoadTableTreeAsync();
            tableNameLabel.Text = $"接続先: {profile.Name}";
            statusLabel.Text = $"接続先を「{profile.Name}」へ切り替えました。";
        }
        catch (Exception exception)
        {
            ShowError("接続先の切替に失敗しました。", exception);
        }
        finally
        {
            SetBusy(false);
    }
    }

    private async Task LoadTableTreeAsync()
    {
        try
        {
            _databaseService ??= CreateDatabaseService();
            IReadOnlyList<string> tableNames = await _databaseService.LoadTableNamesAsync();
            tableTree.Nodes.Clear();
            TreeNode root = tableTree.Nodes.Add("テーブル");
            foreach (string tableName in tableNames)
            {
                root.Nodes.Add(new TreeNode(tableName) { Tag = tableName });
            }
            root.Expand();
            statusLabel.Text = $"テーブル一覧を読み込みました（{tableNames.Count:N0}件）。テーブルを選択してください。";
        }
        catch (Exception exception)
        {
            ShowError("テーブル一覧の読込に失敗しました。", exception);
        }
    }

    private async void RefreshTablesButton_Click(object? sender, EventArgs e)
    {
        SetBusy(true);
        try
        {
            await LoadTableTreeAsync();
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async void OpenSelectedTableButton_Click(object? sender, EventArgs e)
    {
        await OpenSelectedTableAsync();
    }

    private async void TableTree_DoubleClick(object? sender, EventArgs e)
    {
        await OpenSelectedTableAsync();
    }

    private async void TableTree_AfterSelect(object? sender, TreeViewEventArgs e)
    {
        if (e.Node.Tag is string)
        {
            await OpenSelectedTableAsync();
        }
    }

    private async Task OpenSelectedTableAsync()
    {
        string? tableName = tableTree.SelectedNode?.Tag as string;
        if (string.IsNullOrWhiteSpace(tableName))
        {
            MessageBox.Show(this, "テーブルを選択してください。", "確認", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        SetBusy(true);
        try
        {
            await LoadDevelopmentTableAsync(tableName);
        }
        catch (Exception exception)
        {
            ShowError($"開発テーブルの読込に失敗しました。\n対象テーブル: {tableName}", exception);
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async void OpenButton_Click(object? sender, EventArgs e)
    {
        using OpenFileDialog dialog = new() { Filter = "JSONファイル (*.json)|*.json|すべてのファイル (*.*)|*.*", Title = "修正情報JSONを選択" };
        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            await LoadJsonAsync(dialog.FileName);
        }
    }

    private async Task LoadJsonAsync(string filePath)
    {
        SetBusy(true);
        try
        {
            AppendLaControlLog($"JSON読み込み開始: {filePath}");
            _document = await _jsonService.ReadAsync(filePath);
            InvalidateContentCheck();
            updateGrid.DataSource = null;
            _rows.Clear();
            foreach (UpdateOperation operation in _document.Operations)
            {
                _rows.Add(new UpdateOperationRow(operation));
            }
            _jsonOperationTable = CreateJsonOperationTable(_document);
            _jsonOperationTable.ColumnChanged += JsonOperationTable_ColumnChanged;
            updateGrid.DataSource = _jsonOperationTable.DefaultView;
            ConfigureOperationTypeColumn();

            _databaseService = CreateDatabaseService();
            long rowCount = await _databaseService.CountRowsAsync(_document.TableName);
            IReadOnlyList<string> primaryKeys = await _databaseService.LoadPrimaryKeyNamesAsync(_document.TableName);
            _primaryKeyNames = primaryKeys;
            ApplyPrimaryKeyHeaderStyle(primaryKeys);
            tableNameLabel.Text = $"対象テーブル: {_document.TableName}（既存 {rowCount:N0} 件）";
            statusLabel.Text = $"{_rows.Count:N0} 件の修正情報を読み込みました。適用対象を選択してください。";
            AppendLaControlLog($"JSON読み込み完了: 行数={_rows.Count}, Grid表示用DataSourceを設定しました。");
        }
        catch (Exception exception)
        {
            ShowError("JSONまたは対象テーブルの読込に失敗しました。", exception);
        }
        finally
        {
            SetBusy(false);
        }
    }

    private void SelectAllButton_Click(object? sender, EventArgs e) => SetSelection(true);
    private void ClearSelectionButton_Click(object? sender, EventArgs e) => SetSelection(false);

    private void UpdateGrid_CellContentClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (_jsonOperationTable is not null && e.RowIndex >= 0 && e.ColumnIndex >= 0 &&
            string.Equals(updateGrid.Columns[e.ColumnIndex].Name, "Selected", StringComparison.OrdinalIgnoreCase))
        {
            InvalidateContentCheck();
        }
    }

    private void UpdateGrid_CellEndEdit(object? sender, DataGridViewCellEventArgs e)
    {
        if (_jsonOperationTable is not null && e.RowIndex >= 0 && e.ColumnIndex >= 0)
        {
            InvalidateContentCheck();
        }
    }

    private async void CheckButton_Click(object? sender, EventArgs e)
    {
        if (_document is null || _databaseService is null)
        {
            MessageBox.Show(this, "先にJSONを読み込んでください。", "確認", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        SetBusy(true);
        try
        {
            SyncJsonSelection();
            IReadOnlyList<string> primaryKeys = await _databaseService.LoadPrimaryKeyNamesAsync(_document.TableName);
            _primaryKeyNames = primaryKeys;
            SyncAllJsonOperationTypes();
            OperationValidationResult result = await _databaseService.ValidateOperationsAsync(
                _document.TableName,
                _rows.Select(row => row.Operation),
                primaryKeys);
            if (!result.IsValid)
            {
                InvalidateContentCheck();
                ShowError("適用内容のチェックに失敗しました。", new InvalidOperationException(string.Join(Environment.NewLine, result.Errors)));
                return;
            }

            _isContentChecked = true;
            applyButton.Enabled = true;
            statusLabel.Text = "適用内容のチェックに成功しました。内容を変更した場合は再チェックしてください。";
        }
        catch (Exception exception)
        {
            InvalidateContentCheck();
            ShowError("適用内容のチェックに失敗しました。", exception);
        }
        finally
        {
            SetBusy(false);
        }
    }

    private void SyncAllJsonOperationTypes()
    {
        if (_jsonOperationTable is null)
        {
            return;
        }

        foreach (DataRow row in _jsonOperationTable.Rows)
        {
            SyncJsonOperationType(row);
        }
    }

    private async Task LoadDevelopmentTableAsync(string tableName)
    {
        _databaseService ??= CreateDatabaseService();
        _developmentTableName = tableName;
        _developmentTable = await _databaseService.LoadTableAsync(tableName);
        DataColumn selectedColumn = _developmentTable.Columns.Contains("Selected")
            ? _developmentTable.Columns["Selected"]!
            : _developmentTable.Columns.Add("Selected", typeof(bool));
        if (selectedColumn.Ordinal != 0)
        {
            selectedColumn.SetOrdinal(0);
        }
        foreach (DataRow row in _developmentTable.Rows)
        {
            row[selectedColumn] = true;
        }

        DataColumn operationTypeColumn = _developmentTable.Columns.Contains("OperationType")
            ? _developmentTable.Columns["OperationType"]!
            : _developmentTable.Columns.Add("OperationType", typeof(string));
        if (operationTypeColumn.Ordinal != 1)
        {
            operationTypeColumn.SetOrdinal(1);
        }

        foreach (DataRow row in _developmentTable.Rows)
        {
            row[operationTypeColumn] = "更新";
        }

        _document = null;
        InvalidateContentCheck();
        if (_jsonOperationTable is not null)
        {
            _jsonOperationTable.ColumnChanged -= JsonOperationTable_ColumnChanged;
        }
        _jsonOperationTable = null;
        updateGrid.DataSource = _developmentTable.DefaultView;
        ConfigureOperationTypeColumn();
        IReadOnlyList<string> primaryKeys = await _databaseService.LoadPrimaryKeyNamesAsync(tableName);
        _primaryKeyNames = primaryKeys;
        ApplyPrimaryKeyHeaderStyle(primaryKeys);
        tableNameLabel.Text = $"開発テーブル: {_developmentTableName}（{_developmentTable.Rows.Count:N0} 件）";
            statusLabel.Text = "Selected列で出力対象を選択し、OperationType列で行ごとの操作種別を指定してください。";
    }

    private async void ExportJsonButton_Click(object? sender, EventArgs e)
    {
        if (_developmentTable is null || string.IsNullOrWhiteSpace(_developmentTableName) || _databaseService is null)
        {
            MessageBox.Show(this, "先に開発テーブルを読み込んでください。", "確認", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        SetBusy(true);
        try
        {
            IReadOnlyList<string> primaryKeys = await _databaseService.LoadPrimaryKeyNamesAsync(_developmentTableName);
            string json = _exportService.CreateUpdateJson(_developmentTableName, _developmentTable, primaryKeys);
            using SaveFileDialog dialog = new()
            {
                Filter = "JSONファイル (*.json)|*.json",
                FileName = $"{_developmentTableName}_update.json",
                Title = "修正情報JSONの保存先を選択"
            };
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                await File.WriteAllTextAsync(dialog.FileName, json, new System.Text.UTF8Encoding(false));
                statusLabel.Text = $"修正情報JSONを出力しました: {dialog.FileName}";
            }
        }
        catch (Exception exception)
        {
            ShowError("修正情報JSONの作成に失敗しました。", exception);
        }
        finally
        {
            SetBusy(false);
        }
    }

    private static DataTable CreateJsonOperationTable(UpdateInfoDocument document)
    {
        DataTable table = new();
        table.Columns.Add("Selected", typeof(bool));
        table.Columns.Add("OperationType", typeof(string));

        HashSet<string> columnNames = new(StringComparer.OrdinalIgnoreCase);
        foreach (UpdateOperation operation in document.Operations)
        {
            foreach (string name in operation.Keys.Keys.Concat(operation.Values.Keys))
            {
                if (name is not ("Selected" or "OperationType") && columnNames.Add(name))
                {
                    table.Columns.Add(name, typeof(string));
                }
            }
        }

        foreach (UpdateOperation operation in document.Operations)
        {
            DataRow row = table.NewRow();
            row["Selected"] = operation.IsSelected;
            row["OperationType"] = operation.OperationLabel;
            foreach ((string name, JsonElement value) in operation.Keys)
            {
                row[name] = FormatJsonValue(value);
            }

            foreach ((string name, JsonElement value) in operation.Values)
            {
                row[name] = FormatJsonValue(value);
            }

            table.Rows.Add(row);
        }

        return table;
    }

    private static string FormatJsonValue(JsonElement value)
    {
        return value.ValueKind == JsonValueKind.String
            ? value.GetString() ?? string.Empty
            : value.GetRawText();
    }

    private void ConfigureOperationTypeColumn()
    {
        DataGridViewColumn? currentColumn = updateGrid.Columns["OperationType"];
        if (currentColumn is null || currentColumn is DataGridViewComboBoxColumn)
        {
            return;
        }

        int displayIndex = currentColumn.DisplayIndex;
        DataGridViewComboBoxColumn comboColumn = new()
        {
            DataPropertyName = "OperationType",
            HeaderText = "操作種別",
            Name = "OperationType",
            DisplayIndex = displayIndex,
            FlatStyle = FlatStyle.Flat,
            DataSource = new[] { "新規", "更新", "削除" }
        };
        updateGrid.Columns.Remove(currentColumn);
        updateGrid.Columns.Insert(displayIndex, comboColumn);
    }

    private void ApplyPrimaryKeyHeaderStyle(IReadOnlyCollection<string> primaryKeyNames)
    {
        Color primaryKeyColor = Color.FromArgb(255, 224, 130);
        Color normalColor = Color.FromArgb(255, 192, 128);

        foreach (DataGridViewColumn column in updateGrid.Columns)
        {
            bool isPrimaryKey = primaryKeyNames.Contains(column.Name, StringComparer.OrdinalIgnoreCase);
            column.HeaderCell.Style.BackColor = isPrimaryKey ? primaryKeyColor : normalColor;
            column.HeaderCell.Style.ForeColor = Color.FromArgb(64, 45, 0);
            column.HeaderCell.Style.Font = new Font(updateGrid.ColumnHeadersDefaultCellStyle.Font ?? updateGrid.Font, isPrimaryKey ? FontStyle.Bold : FontStyle.Regular);
            column.HeaderCell.ToolTipText = isPrimaryKey ? "主キー" : string.Empty;
        }
    }

    private void SyncJsonSelection()
    {
        if (_jsonOperationTable is null)
        {
            return;
        }

        int count = Math.Min(_rows.Count, _jsonOperationTable.Rows.Count);
        for (int index = 0; index < count; index++)
        {
            DataRow row = _jsonOperationTable.Rows[index];
            UpdateOperation operation = _rows[index].Operation;
            operation.IsSelected = row.Field<bool>("Selected");
            operation.Type = GetOperationTypeCode(row["OperationType"]);
            SyncJsonValues(operation.Keys, row);
            SyncJsonValues(operation.Values, row);
        }
    }

    private static string GetOperationTypeCode(object value)
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

    private void SyncJsonOperationType(DataRow row)
    {
        if (_jsonOperationTable is null || _rows.Count <= row.Table.Rows.IndexOf(row))
        {
            return;
        }

        int index = row.Table.Rows.IndexOf(row);
        UpdateOperation operation = _rows[index].Operation;
        operation.Type = GetOperationTypeCode(row["OperationType"]);
        SyncJsonValues(operation.Keys, row);
        SyncJsonValues(operation.Values, row);

        foreach (string primaryKeyName in _primaryKeyNames)
        {
            if (!row.Table.Columns.Contains(primaryKeyName))
            {
                continue;
            }

            string text = row[primaryKeyName] == DBNull.Value ? string.Empty : row[primaryKeyName]?.ToString() ?? string.Empty;
            JsonElement value = ConvertJsonValue(text, GetOriginalValue(operation, primaryKeyName), primaryKeyName);
            if (operation.Type == "insert")
            {
                operation.Values[primaryKeyName] = value;
                operation.Keys.Remove(primaryKeyName);
            }
            else
            {
                operation.Keys[primaryKeyName] = value;
            }
        }
    }

    private static JsonElement GetOriginalValue(UpdateOperation operation, string name)
    {
        if (operation.Values.TryGetValue(name, out JsonElement value))
        {
            return value;
        }

        if (operation.Keys.TryGetValue(name, out value))
        {
            return value;
        }

        return JsonSerializer.SerializeToElement<string>(string.Empty);
    }

    private static void SyncJsonValues(Dictionary<string, JsonElement> values, DataRow row)
    {
        foreach ((string name, JsonElement originalValue) in values.ToList())
        {
            if (!row.Table.Columns.Contains(name))
            {
                continue;
            }

            string text = row[name] == DBNull.Value ? string.Empty : row[name]?.ToString() ?? string.Empty;
            values[name] = ConvertJsonValue(text, originalValue, name);
        }
    }

    private static JsonElement ConvertJsonValue(string text, JsonElement originalValue, string propertyName)
    {
        if (originalValue.ValueKind == JsonValueKind.String)
        {
            return JsonSerializer.SerializeToElement(text);
        }

        if (string.IsNullOrWhiteSpace(text))
        {
            return JsonSerializer.SerializeToElement<object?>(null);
        }

        try
        {
            return originalValue.ValueKind switch
            {
                JsonValueKind.Number or JsonValueKind.Object or JsonValueKind.Array =>
                    ParseJsonValue(text, propertyName),
                JsonValueKind.True or JsonValueKind.False =>
                    bool.TryParse(text, out bool boolean)
                        ? JsonSerializer.SerializeToElement(boolean)
                        : throw new JsonException($"列「{propertyName}」の値が真偽値として解釈できません: {text}"),
                JsonValueKind.Null => JsonSerializer.SerializeToElement(text),
                _ => JsonSerializer.SerializeToElement(text)
            };
        }
        catch (JsonException)
        {
            throw;
        }
        catch (Exception exception)
        {
            throw new JsonException($"列「{propertyName}」の値をJSONへ変換できません: {text}", exception);
        }
    }

    private static JsonElement ParseJsonValue(string text, string propertyName)
    {
        try
        {
            using JsonDocument document = JsonDocument.Parse(text);
            return document.RootElement.Clone();
        }
        catch (JsonException exception)
        {
            throw new JsonException($"列「{propertyName}」のJSON値が不正です: {text}", exception);
        }
    }

    private void SetSelection(bool selected)
    {
        InvalidateContentCheck();
        foreach (UpdateOperationRow row in _rows)
        {
            row.Selected = selected;
        }

        if (_jsonOperationTable is not null)
        {
            foreach (DataRow row in _jsonOperationTable.Rows)
            {
                row["Selected"] = selected;
            }
        }

        if (_developmentTable is not null)
        {
            foreach (DataRow row in _developmentTable.Rows)
            {
                row["Selected"] = selected;
            }
        }

        updateGrid.Refresh();
    }

    private async void ApplyButton_Click(object? sender, EventArgs e)
    {
        if (_document is null || _databaseService is null)
        {
            MessageBox.Show(this, "先にJSONを読み込んでください。", "確認", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        if (!_isContentChecked)
        {
            MessageBox.Show(this, "先に「適用内容をチェック」を実行してください。", "確認", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        SyncJsonSelection();
        int selectedCount = _rows.Count(row => row.Selected);
        if (selectedCount == 0)
        {
            MessageBox.Show(this, "適用対象が選択されていません。", "確認", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        OperationSummary summary = OperationSummary.FromOperations(_rows.Select(row => row.Operation));
        string environment = _activeConnectionProfile?.Environment ?? "Development";
        List<ApplyPreviewRow> previewRows = [];
        foreach (UpdateOperation operation in _rows.Where(row => row.Selected).Select(row => row.Operation))
        {
            IReadOnlyDictionary<string, JsonElement>? currentValues =
                await _databaseService.LoadCurrentValuesAsync(_document.TableName, operation);
            previewRows.Add(new ApplyPreviewRow(operation, currentValues));
        }

        List<ApplyPreviewRow> effectivePreviewRows = previewRows
            .Where(ApplyPreviewDialog.HasChanges)
            .ToList();
        int skippedNoChangeCount = previewRows.Count - effectivePreviewRows.Count;
        if (effectivePreviewRows.Count == 0)
        {
            MessageBox.Show(this, "選択された更新対象に差分がないため、DBは更新しません。", "適用不要", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        if (skippedNoChangeCount > 0)
        {
            MessageBox.Show(
                this,
                $"差分がない更新 {skippedNoChangeCount:N0}件は適用対象から除外します。",
                "適用対象の確認",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        summary = OperationSummary.FromOperations(effectivePreviewRows.Select(row => row.Operation));

        using ApplyPreviewDialog previewDialog = new(
            _activeConnectionProfile?.Name ?? "未選択",
            environment,
            _document.TableName,
            effectivePreviewRows);
        if (previewDialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        string confirmation = string.Equals(environment, "Production", StringComparison.OrdinalIgnoreCase)
            ? $"本番環境「{_activeConnectionProfile?.Name}」へ{summary}を適用します。よろしいですか？"
            : $"{summary}をDBへ適用します。よろしいですか？";
        if (MessageBox.Show(this, confirmation, "適用確認", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
        {
            return;
        }

        SetBusy(true);
        try
        {
            int affected = await _databaseService.ApplyAsync(
                _document.TableName,
                effectivePreviewRows.Select(row => row.Operation),
                ApplyPreviewDialog.PreviewJapanTime);
            statusLabel.Text = $"適用完了: {affected:N0} 行に反映しました。";
            if (_activeConnectionProfile is not null)
            {
                await _auditLogService.AppendAsync(
                    _activeConnectionProfile.Settings.BackupDirectory,
                    new AuditLogEntry(
                        DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(9)),
                        _activeConnectionProfile.Name,
                        _document.TableName,
                        summary,
                        true,
                        _activeConnectionProfile.Settings.BackupDirectory,
                        null));
            }
        }
        catch (Exception exception)
        {
            if (_activeConnectionProfile is not null)
            {
                await _auditLogService.AppendAsync(
                    _activeConnectionProfile.Settings.BackupDirectory,
                    new AuditLogEntry(
                        DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(9)),
                        _activeConnectionProfile.Name,
                        _document.TableName,
                        summary,
                        false,
                        null,
                        exception.Message));
            }
            ShowError("修正情報の適用に失敗しました。DBはロールバックされています。", exception);
        }
        finally
        {
            SetBusy(false);
        }
    }

    private DatabaseUpdateService CreateDatabaseService()
    {
        DatabaseConnectionProfile profile = _activeConnectionProfile ?? LoadConnectionProfiles().First();
        return new DatabaseUpdateService(profile.Settings);
    }

    private static IReadOnlyList<DatabaseConnectionProfile> LoadConnectionProfiles()
    {
        string settingsPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        using JsonDocument document = JsonDocument.Parse(File.ReadAllText(settingsPath));
        JsonElement database = document.RootElement.GetProperty("Database");
        List<DatabaseConnectionProfile> profiles = [];
        if (database.TryGetProperty("Connections", out JsonElement connections) && connections.ValueKind == JsonValueKind.Array)
        {
            foreach (JsonElement connection in connections.EnumerateArray())
            {
                string name = connection.GetProperty("Name").GetString() ?? throw new InvalidOperationException("接続先名が未設定です。");
                string environment = connection.TryGetProperty("Environment", out JsonElement environmentElement)
                    ? environmentElement.GetString() ?? "Development"
                    : InferEnvironment(name);
                string backgroundColor = connection.TryGetProperty("BackgroundColor", out JsonElement colorElement)
                    ? colorElement.GetString() ?? GetDefaultBackgroundColor(environment)
                    : GetDefaultBackgroundColor(environment);
                profiles.Add(new DatabaseConnectionProfile(name, CreateDatabaseSettings(connection), environment, backgroundColor));
            }
        }
        else
        {
            profiles.Add(new DatabaseConnectionProfile("既定", CreateDatabaseSettings(database)));
        }

        return profiles;
    }

    private void ApplyConnectionAppearance(DatabaseConnectionProfile profile)
    {
        Color backgroundColor = ColorTranslator.FromHtml(profile.BackgroundColor);
        BackColor = backgroundColor;
        mainLayout.BackColor = backgroundColor;
        commandPanel.BackColor = backgroundColor;
        tableNameLabel.BackColor = backgroundColor;
        statusLabel.BackColor = backgroundColor;
        connectionLabel.BackColor = backgroundColor;
        connectionLabel.Text = $"接続先 ({profile.Environment}):";
        connectionComboBox.BackColor = backgroundColor;
    }

    private static string InferEnvironment(string name)
    {
        return name.Contains("本番", StringComparison.OrdinalIgnoreCase) ||
               name.Contains("production", StringComparison.OrdinalIgnoreCase)
            ? "Production"
            : "Development";
    }

    private static string GetDefaultBackgroundColor(string environment)
    {
        return string.Equals(environment, "Production", StringComparison.OrdinalIgnoreCase)
            ? "#FFF0F0"
            : "#EAF3FF";
    }

    private static DatabaseSettings CreateDatabaseSettings(JsonElement database)
    {
        string providerName = database.GetProperty("ProviderInvariantName").GetString()
            ?? throw new InvalidOperationException("ProviderInvariantNameが未設定です。");
        string connectionString = database.GetProperty("ConnectionString").GetString()
            ?? throw new InvalidOperationException("ConnectionStringが未設定です。");
        if (string.Equals(providerName, "Microsoft.Data.Sqlite", StringComparison.OrdinalIgnoreCase))
        {
            SqliteConnectionStringBuilder connectionBuilder = new(connectionString);
            if (!string.IsNullOrWhiteSpace(connectionBuilder.DataSource) &&
                !string.Equals(connectionBuilder.DataSource, ":memory:", StringComparison.OrdinalIgnoreCase) &&
                !Path.IsPathRooted(connectionBuilder.DataSource))
            {
                connectionBuilder.DataSource = Path.Combine(AppContext.BaseDirectory, connectionBuilder.DataSource);
            }

            connectionString = connectionBuilder.ConnectionString;
        }

        DatabaseSettings settings = new(
            providerName,
            connectionString,
            database.TryGetProperty("BackupDirectory", out JsonElement backupDirectory)
                ? backupDirectory.GetString() ?? "backups"
                : "backups",
            database.TryGetProperty("MaxLoadRows", out JsonElement maxLoadRows) && maxLoadRows.TryGetInt32(out int configuredMaxLoadRows)
                ? configuredMaxLoadRows
                : 10_000);
        if (settings.MaxLoadRows <= 0)
        {
            throw new InvalidOperationException("Database.MaxLoadRowsには1以上を設定してください。");
        }

        return settings;
    }

    private void SetBusy(bool busy)
    {
        openButton.Enabled = !busy;
        selectAllButton.Enabled = !busy;
        clearSelectionButton.Enabled = !busy;
        applyButton.Enabled = !busy && _isContentChecked;
        checkButton.Enabled = !busy;
        exportJsonButton.Enabled = !busy;
        autoSizeColumnsButton.Enabled = !busy;
        refreshTablesButton.Enabled = !busy;
        openSelectedTableButton.Enabled = !busy;
        Cursor = busy ? Cursors.WaitCursor : Cursors.Default;
    }

    private void JsonOperationTable_ColumnChanged(object? sender, DataColumnChangeEventArgs e)
    {
        InvalidateContentCheck();
        if (string.Equals(e.Column.ColumnName, "OperationType", StringComparison.OrdinalIgnoreCase))
        {
            SyncJsonOperationType(e.Row);
        }
    }

    private void InvalidateContentCheck()
    {
        _isContentChecked = false;
        applyButton.Enabled = false;
    }

    private void ShowError(string message, Exception exception)
    {
        statusLabel.Text = message;
        string log = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}{Environment.NewLine}{exception}{Environment.NewLine}{Environment.NewLine}";
        errorLogTextBox.AppendText(log);
        MessageBox.Show(this, $"{message}\n\n{exception.Message}", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}

public sealed class UpdateOperationRow
{
    private readonly UpdateOperation _operation;

    public UpdateOperationRow(UpdateOperation operation) => _operation = operation;

    [JsonIgnore]
    public UpdateOperation Operation => _operation;
    public bool Selected { get => _operation.IsSelected; set => _operation.IsSelected = value; }
    public string OperationType => _operation.OperationLabel;
    public string Values => Serialize(_operation.Values);
    public string Keys => Serialize(_operation.Keys);

    private static string Serialize(IReadOnlyDictionary<string, JsonElement> values) => string.Join(", ", values.Select(pair => $"{pair.Key}={pair.Value.GetRawText()}"));
}
