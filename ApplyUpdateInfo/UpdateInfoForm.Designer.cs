namespace ApplyUpdateInfo
{
    partial class UpdateInfoForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        private TableLayoutPanel mainLayout;
        private FlowLayoutPanel commandPanel;
        private FlowLayoutPanel actionPanel;
        private Button openButton;
        private Button selectAllButton;
        private Button clearSelectionButton;
        private Button applyButton;
        private Button checkButton;
        private Button exportJsonButton;
        private Button createLayoutButton;
        private Label tableNameLabel;
        private Label statusLabel;
        private LaControl.LaTableControl updateGrid;
        private SplitContainer tableSplitContainer;
        private Panel tablePanel;
        private TreeView tableTree;
        private Button refreshTablesButton;
        private Button openSelectedTableButton;
        private TextBox errorLogTextBox;
        private Label connectionLabel;
        private ComboBox connectionComboBox;
        private Button switchConnectionButton;
        private TextBox filterTextBox;
        private ComboBox operationFilterComboBox;
        private ComboBox operationTypeComboBox;
        private Button setOperationTypeButton;
        private Button setNowButton;
        private Button autoSizeColumnsButton;
        private Panel tableInfoPanel;
        private FlowLayoutPanel loadingPanel;
        private Label loadingLabel;
        private ProgressBar loadingProgressBar;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            mainLayout = new TableLayoutPanel();
            commandPanel = new FlowLayoutPanel();
            openButton = new Button();
            selectAllButton = new Button();
            clearSelectionButton = new Button();
            exportJsonButton = new Button();
            createLayoutButton = new Button();
            connectionLabel = new Label();
            connectionComboBox = new ComboBox();
            switchConnectionButton = new Button();
            actionPanel = new FlowLayoutPanel();
            checkButton = new Button();
            applyButton = new Button();
            filterTextBox = new TextBox();
            operationFilterComboBox = new ComboBox();
            operationTypeComboBox = new ComboBox();
            setOperationTypeButton = new Button();
            setNowButton = new Button();
            autoSizeColumnsButton = new Button();
            tableInfoPanel = new Panel();
            tableNameLabel = new Label();
            loadingPanel = new FlowLayoutPanel();
            loadingLabel = new Label();
            loadingProgressBar = new ProgressBar();
            tableSplitContainer = new SplitContainer();
            tablePanel = new Panel();
            tableTree = new TreeView();
            openSelectedTableButton = new Button();
            refreshTablesButton = new Button();
            updateGrid = new LaControl.LaTableControl();
            statusLabel = new Label();
            errorLogTextBox = new TextBox();
            mainLayout.SuspendLayout();
            commandPanel.SuspendLayout();
            actionPanel.SuspendLayout();
            tableInfoPanel.SuspendLayout();
            loadingPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)tableSplitContainer).BeginInit();
            tableSplitContainer.Panel1.SuspendLayout();
            tableSplitContainer.Panel2.SuspendLayout();
            tableSplitContainer.SuspendLayout();
            tablePanel.SuspendLayout();
            SuspendLayout();
            // 
            // mainLayout
            // 
            mainLayout.ColumnCount = 1;
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 1549F));
            mainLayout.Controls.Add(commandPanel, 0, 0);
            mainLayout.Controls.Add(actionPanel, 0, 1);
            mainLayout.Controls.Add(tableInfoPanel, 0, 2);
            mainLayout.Controls.Add(tableSplitContainer, 0, 3);
            mainLayout.Controls.Add(statusLabel, 0, 4);
            mainLayout.Controls.Add(errorLogTextBox, 0, 5);
            mainLayout.Dock = DockStyle.Fill;
            mainLayout.Location = new Point(0, 0);
            mainLayout.Margin = new Padding(4);
            mainLayout.Name = "mainLayout";
            mainLayout.Padding = new Padding(11);
            mainLayout.RowCount = 6;
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 56F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 56F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 37F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 37F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 147F));
            mainLayout.Size = new Size(1571, 867);
            mainLayout.TabIndex = 0;
            // 
            // commandPanel
            // 
            commandPanel.Controls.Add(openButton);
            commandPanel.Controls.Add(selectAllButton);
            commandPanel.Controls.Add(clearSelectionButton);
            commandPanel.Controls.Add(exportJsonButton);
            commandPanel.Controls.Add(connectionLabel);
            commandPanel.Controls.Add(connectionComboBox);
            commandPanel.Controls.Add(switchConnectionButton);
            commandPanel.Dock = DockStyle.Fill;
            commandPanel.Location = new Point(15, 15);
            commandPanel.Margin = new Padding(4);
            commandPanel.Name = "commandPanel";
            commandPanel.Size = new Size(1541, 48);
            commandPanel.TabIndex = 0;
            // 
            // openButton
            // 
            openButton.AutoSize = true;
            openButton.BackColor = Color.FromArgb(232, 240, 254);
            openButton.FlatAppearance.BorderColor = Color.FromArgb(160, 174, 192);
            openButton.FlatStyle = FlatStyle.Flat;
            openButton.ForeColor = Color.FromArgb(32, 45, 64);
            openButton.Location = new Point(4, 4);
            openButton.Margin = new Padding(4);
            openButton.Name = "openButton";
            openButton.Size = new Size(107, 33);
            openButton.TabIndex = 0;
            openButton.Text = "JSON読込";
            openButton.UseVisualStyleBackColor = false;
            openButton.Click += OpenButton_Click;
            // 
            // selectAllButton
            // 
            selectAllButton.AutoSize = true;
            selectAllButton.BackColor = Color.FromArgb(232, 245, 233);
            selectAllButton.FlatAppearance.BorderColor = Color.FromArgb(129, 170, 132);
            selectAllButton.FlatStyle = FlatStyle.Flat;
            selectAllButton.ForeColor = Color.FromArgb(35, 85, 40);
            selectAllButton.Location = new Point(119, 4);
            selectAllButton.Margin = new Padding(4);
            selectAllButton.Name = "selectAllButton";
            selectAllButton.Size = new Size(107, 33);
            selectAllButton.TabIndex = 1;
            selectAllButton.Text = "全選択";
            selectAllButton.UseVisualStyleBackColor = false;
            selectAllButton.Click += SelectAllButton_Click;
            // 
            // clearSelectionButton
            // 
            clearSelectionButton.AutoSize = true;
            clearSelectionButton.BackColor = Color.FromArgb(255, 243, 224);
            clearSelectionButton.FlatAppearance.BorderColor = Color.FromArgb(210, 160, 85);
            clearSelectionButton.FlatStyle = FlatStyle.Flat;
            clearSelectionButton.ForeColor = Color.FromArgb(120, 75, 20);
            clearSelectionButton.Location = new Point(234, 4);
            clearSelectionButton.Margin = new Padding(4);
            clearSelectionButton.Name = "clearSelectionButton";
            clearSelectionButton.Size = new Size(107, 33);
            clearSelectionButton.TabIndex = 2;
            clearSelectionButton.Text = "全解除";
            clearSelectionButton.UseVisualStyleBackColor = false;
            clearSelectionButton.Click += ClearSelectionButton_Click;
            // 
            // exportJsonButton
            // 
            exportJsonButton.AutoSize = true;
            exportJsonButton.BackColor = Color.FromArgb(224, 242, 241);
            exportJsonButton.FlatAppearance.BorderColor = Color.FromArgb(102, 165, 160);
            exportJsonButton.FlatStyle = FlatStyle.Flat;
            exportJsonButton.ForeColor = Color.FromArgb(25, 85, 80);
            exportJsonButton.Location = new Point(349, 4);
            exportJsonButton.Margin = new Padding(4);
            exportJsonButton.Name = "exportJsonButton";
            exportJsonButton.Size = new Size(167, 33);
            exportJsonButton.TabIndex = 6;
            exportJsonButton.Text = "修正情報JSON作成";
            exportJsonButton.UseVisualStyleBackColor = false;
            exportJsonButton.Click += ExportJsonButton_Click;
            // 
            // createLayoutButton
            // 
            createLayoutButton.AutoSize = true;
            createLayoutButton.BackColor = Color.FromArgb(255, 243, 224);
            createLayoutButton.FlatAppearance.BorderColor = Color.FromArgb(210, 160, 85);
            createLayoutButton.FlatStyle = FlatStyle.Flat;
            createLayoutButton.ForeColor = Color.FromArgb(120, 75, 20);
            createLayoutButton.Dock = DockStyle.Top;
            createLayoutButton.Margin = new Padding(4);
            createLayoutButton.Name = "createLayoutButton";
            createLayoutButton.Size = new Size(167, 33);
            createLayoutButton.TabIndex = 7;
            createLayoutButton.Text = "レイアウト作成";
            createLayoutButton.UseVisualStyleBackColor = false;
            createLayoutButton.Click += CreateLayoutButton_Click;
            // 
            // connectionLabel
            // 
            connectionLabel.AutoSize = true;
            connectionLabel.Location = new Point(532, 13);
            connectionLabel.Margin = new Padding(12, 13, 4, 4);
            connectionLabel.Name = "connectionLabel";
            connectionLabel.Size = new Size(64, 20);
            connectionLabel.TabIndex = 7;
            connectionLabel.Text = "接続先:";
            // 
            // connectionComboBox
            // 
            connectionComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            connectionComboBox.Location = new Point(604, 8);
            connectionComboBox.Margin = new Padding(4, 8, 4, 4);
            connectionComboBox.Name = "connectionComboBox";
            connectionComboBox.Size = new Size(220, 28);
            connectionComboBox.TabIndex = 8;
            // 
            // switchConnectionButton
            // 
            switchConnectionButton.AutoSize = true;
            switchConnectionButton.BackColor = Color.FromArgb(232, 240, 254);
            switchConnectionButton.FlatAppearance.BorderColor = Color.FromArgb(115, 145, 185);
            switchConnectionButton.FlatStyle = FlatStyle.Flat;
            switchConnectionButton.ForeColor = Color.FromArgb(32, 60, 100);
            switchConnectionButton.Location = new Point(832, 4);
            switchConnectionButton.Margin = new Padding(4);
            switchConnectionButton.Name = "switchConnectionButton";
            switchConnectionButton.Size = new Size(100, 33);
            switchConnectionButton.TabIndex = 9;
            switchConnectionButton.Text = "接続切替";
            switchConnectionButton.UseVisualStyleBackColor = false;
            switchConnectionButton.Click += SwitchConnectionButton_Click;
            // 
            // actionPanel
            // 
            actionPanel.Controls.Add(checkButton);
            actionPanel.Controls.Add(applyButton);
            actionPanel.Controls.Add(filterTextBox);
            actionPanel.Controls.Add(operationFilterComboBox);
            actionPanel.Controls.Add(operationTypeComboBox);
            actionPanel.Controls.Add(setOperationTypeButton);
            actionPanel.Controls.Add(setNowButton);
            actionPanel.Controls.Add(autoSizeColumnsButton);
            actionPanel.Dock = DockStyle.Fill;
            actionPanel.Location = new Point(15, 71);
            actionPanel.Margin = new Padding(4);
            actionPanel.Name = "actionPanel";
            actionPanel.Size = new Size(1541, 48);
            actionPanel.TabIndex = 10;
            actionPanel.WrapContents = false;
            // 
            // checkButton
            // 
            checkButton.AutoSize = true;
            checkButton.BackColor = Color.FromArgb(33, 113, 181);
            checkButton.FlatAppearance.BorderColor = Color.FromArgb(25, 82, 132);
            checkButton.FlatStyle = FlatStyle.Flat;
            checkButton.ForeColor = Color.White;
            checkButton.Location = new Point(4, 4);
            checkButton.Margin = new Padding(4);
            checkButton.Name = "checkButton";
            checkButton.Size = new Size(176, 33);
            checkButton.TabIndex = 3;
            checkButton.Text = "適用内容をチェック";
            checkButton.UseVisualStyleBackColor = false;
            checkButton.Click += CheckButton_Click;
            // 
            // applyButton
            // 
            applyButton.AutoSize = true;
            applyButton.BackColor = Color.FromArgb(46, 125, 50);
            applyButton.FlatAppearance.BorderColor = Color.FromArgb(32, 87, 35);
            applyButton.FlatStyle = FlatStyle.Flat;
            applyButton.ForeColor = Color.White;
            applyButton.Location = new Point(188, 4);
            applyButton.Margin = new Padding(4);
            applyButton.Name = "applyButton";
            applyButton.Size = new Size(176, 33);
            applyButton.TabIndex = 4;
            applyButton.Text = "選択内容をDBへ適用";
            applyButton.UseVisualStyleBackColor = false;
            applyButton.Click += ApplyButton_Click;
            // 
            // filterTextBox
            // 
            filterTextBox.Location = new Point(371, 3);
            filterTextBox.Name = "filterTextBox";
            filterTextBox.PlaceholderText = "検索";
            filterTextBox.Size = new Size(180, 28);
            filterTextBox.TabIndex = 5;
            filterTextBox.TextChanged += FilterTextBox_TextChanged;
            // 
            // operationFilterComboBox
            // 
            operationFilterComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            operationFilterComboBox.Items.AddRange(new object[] { "すべて", "新規", "更新", "削除" });
            operationFilterComboBox.Location = new Point(557, 3);
            operationFilterComboBox.Name = "operationFilterComboBox";
            operationFilterComboBox.Size = new Size(110, 28);
            operationFilterComboBox.TabIndex = 6;
            operationFilterComboBox.SelectedIndexChanged += OperationFilterComboBox_SelectedIndexChanged;
            // 
            // operationTypeComboBox
            // 
            operationTypeComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            operationTypeComboBox.Items.AddRange(new object[] { "新規", "更新", "削除" });
            operationTypeComboBox.Location = new Point(673, 3);
            operationTypeComboBox.Name = "operationTypeComboBox";
            operationTypeComboBox.Size = new Size(110, 28);
            operationTypeComboBox.TabIndex = 7;
            // 
            // setOperationTypeButton
            // 
            setOperationTypeButton.AutoSize = true;
            setOperationTypeButton.Location = new Point(789, 3);
            setOperationTypeButton.Name = "setOperationTypeButton";
            setOperationTypeButton.Size = new Size(144, 30);
            setOperationTypeButton.TabIndex = 7;
            setOperationTypeButton.Text = "選択行の種別変更";
            setOperationTypeButton.Click += SetOperationTypeButton_Click;
            // 
            // setNowButton
            // 
            setNowButton.AutoSize = true;
            setNowButton.BackColor = Color.FromArgb(255, 243, 205);
            setNowButton.FlatAppearance.BorderColor = Color.FromArgb(190, 155, 70);
            setNowButton.FlatStyle = FlatStyle.Flat;
            setNowButton.ForeColor = Color.FromArgb(100, 75, 20);
            setNowButton.Location = new Point(940, 4);
            setNowButton.Margin = new Padding(4);
            setNowButton.Name = "setNowButton";
            setNowButton.Size = new Size(150, 33);
            setNowButton.TabIndex = 11;
            setNowButton.Text = "現在時刻を設定";
            setNowButton.UseVisualStyleBackColor = false;
            setNowButton.Click += SetNowButton_Click;
            // 
            // autoSizeColumnsButton
            // 
            autoSizeColumnsButton.AutoSize = true;
            autoSizeColumnsButton.BackColor = Color.FromArgb(232, 240, 254);
            autoSizeColumnsButton.FlatAppearance.BorderColor = Color.FromArgb(115, 145, 185);
            autoSizeColumnsButton.FlatStyle = FlatStyle.Flat;
            autoSizeColumnsButton.ForeColor = Color.FromArgb(32, 60, 100);
            autoSizeColumnsButton.Location = new Point(1098, 4);
            autoSizeColumnsButton.Margin = new Padding(4);
            autoSizeColumnsButton.Name = "autoSizeColumnsButton";
            autoSizeColumnsButton.Size = new Size(150, 33);
            autoSizeColumnsButton.TabIndex = 12;
            autoSizeColumnsButton.Text = "列幅自動調整";
            autoSizeColumnsButton.UseVisualStyleBackColor = false;
            autoSizeColumnsButton.Click += AutoSizeColumnsButton_Click;
            // 
            // tableInfoPanel
            // 
            tableInfoPanel.Controls.Add(tableNameLabel);
            tableInfoPanel.Controls.Add(loadingPanel);
            tableInfoPanel.Dock = DockStyle.Fill;
            tableInfoPanel.Location = new Point(15, 123);
            tableInfoPanel.Margin = new Padding(4, 0, 4, 0);
            tableInfoPanel.Name = "tableInfoPanel";
            tableInfoPanel.Size = new Size(1541, 37);
            tableInfoPanel.TabIndex = 1;
            // 
            // tableNameLabel
            // 
            tableNameLabel.Dock = DockStyle.Fill;
            tableNameLabel.Location = new Point(0, 0);
            tableNameLabel.Margin = new Padding(0);
            tableNameLabel.Name = "tableNameLabel";
            tableNameLabel.Size = new Size(1091, 37);
            tableNameLabel.TabIndex = 0;
            tableNameLabel.Text = "対象テーブル: 未読込";
            tableNameLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // loadingPanel
            // 
            loadingPanel.Controls.Add(loadingLabel);
            loadingPanel.Controls.Add(loadingProgressBar);
            loadingPanel.Dock = DockStyle.Right;
            loadingPanel.Location = new Point(1091, 0);
            loadingPanel.Margin = new Padding(0);
            loadingPanel.Name = "loadingPanel";
            loadingPanel.Size = new Size(450, 37);
            loadingPanel.TabIndex = 0;
            loadingPanel.Visible = false;
            loadingPanel.WrapContents = false;
            // 
            // loadingLabel
            // 
            loadingLabel.AutoSize = true;
            loadingLabel.Location = new Point(4, 9);
            loadingLabel.Margin = new Padding(4, 9, 4, 4);
            loadingLabel.Name = "loadingLabel";
            loadingLabel.Size = new Size(169, 20);
            loadingLabel.TabIndex = 0;
            loadingLabel.Text = "テーブル一覧を読込中...";
            loadingLabel.TextAlign = ContentAlignment.MiddleLeft;
            loadingLabel.Visible = false;
            // 
            // loadingProgressBar
            // 
            loadingProgressBar.Location = new Point(181, 5);
            loadingProgressBar.Margin = new Padding(4, 5, 4, 4);
            loadingProgressBar.MarqueeAnimationSpeed = 30;
            loadingProgressBar.Name = "loadingProgressBar";
            loadingProgressBar.Size = new Size(180, 28);
            loadingProgressBar.Style = ProgressBarStyle.Marquee;
            loadingProgressBar.TabIndex = 13;
            loadingProgressBar.Visible = false;
            // 
            // tableSplitContainer
            // 
            tableSplitContainer.Dock = DockStyle.Fill;
            tableSplitContainer.FixedPanel = FixedPanel.Panel1;
            tableSplitContainer.Location = new Point(15, 164);
            tableSplitContainer.Margin = new Padding(4);
            tableSplitContainer.Name = "tableSplitContainer";
            // 
            // tableSplitContainer.Panel1
            // 
            tableSplitContainer.Panel1.Controls.Add(tablePanel);
            tableSplitContainer.Panel1MinSize = 300;
            // 
            // tableSplitContainer.Panel2
            // 
            tableSplitContainer.Panel2.Controls.Add(updateGrid);
            tableSplitContainer.Size = new Size(1541, 504);
            tableSplitContainer.SplitterDistance = 360;
            tableSplitContainer.SplitterWidth = 6;
            tableSplitContainer.TabIndex = 2;
            // 
            // tablePanel
            // 
            tablePanel.Controls.Add(tableTree);
            tablePanel.Controls.Add(openSelectedTableButton);
            tablePanel.Controls.Add(refreshTablesButton);
            tablePanel.Controls.Add(createLayoutButton);
            tablePanel.Dock = DockStyle.Fill;
            tablePanel.Location = new Point(0, 0);
            tablePanel.Margin = new Padding(4);
            tablePanel.Name = "tablePanel";
            tablePanel.Padding = new Padding(0, 0, 11, 0);
            tablePanel.Size = new Size(257, 504);
            tablePanel.TabIndex = 0;
            // 
            // tableTree
            // 
            tableTree.Dock = DockStyle.Fill;
            tableTree.HideSelection = false;
            tableTree.Location = new Point(0, 43);
            tableTree.Margin = new Padding(4);
            tableTree.Name = "tableTree";
            tableTree.Size = new Size(246, 413);
            tableTree.TabIndex = 0;
            tableTree.AfterSelect += TableTree_AfterSelect;
            tableTree.DoubleClick += TableTree_DoubleClick;
            // 
            // openSelectedTableButton
            // 
            openSelectedTableButton.BackColor = Color.FromArgb(232, 240, 254);
            openSelectedTableButton.Dock = DockStyle.Bottom;
            openSelectedTableButton.FlatAppearance.BorderColor = Color.FromArgb(115, 145, 185);
            openSelectedTableButton.FlatStyle = FlatStyle.Flat;
            openSelectedTableButton.ForeColor = Color.FromArgb(32, 60, 100);
            openSelectedTableButton.Location = new Point(0, 456);
            openSelectedTableButton.Margin = new Padding(4);
            openSelectedTableButton.Name = "openSelectedTableButton";
            openSelectedTableButton.Size = new Size(246, 48);
            openSelectedTableButton.TabIndex = 1;
            openSelectedTableButton.Text = "選択テーブルを開く";
            openSelectedTableButton.UseVisualStyleBackColor = false;
            openSelectedTableButton.Click += OpenSelectedTableButton_Click;
            // 
            // refreshTablesButton
            // 
            refreshTablesButton.BackColor = Color.FromArgb(245, 245, 245);
            refreshTablesButton.Dock = DockStyle.Top;
            refreshTablesButton.FlatAppearance.BorderColor = Color.FromArgb(160, 160, 160);
            refreshTablesButton.FlatStyle = FlatStyle.Flat;
            refreshTablesButton.ForeColor = Color.FromArgb(64, 64, 64);
            refreshTablesButton.Location = new Point(0, 0);
            refreshTablesButton.Margin = new Padding(4);
            refreshTablesButton.Name = "refreshTablesButton";
            refreshTablesButton.Size = new Size(246, 43);
            refreshTablesButton.TabIndex = 2;
            refreshTablesButton.Text = "一覧更新";
            refreshTablesButton.UseVisualStyleBackColor = false;
            refreshTablesButton.Click += RefreshTablesButton_Click;
            // 
            // updateGrid
            // 
            updateGrid.AccessibleDescription = "表形式のデータを編集するコントロール";
            updateGrid.AccessibleName = "表";
            updateGrid.AccessibleRole = AccessibleRole.Table;
            updateGrid.ActiveCellBackColor = Color.Blue;
            updateGrid.AllowUserToResizeRows = false;
            updateGrid.AlternatingRowsDefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 0, 192);
            updateGrid.AlternatingRowsDefaultCellStyle.SelectionForeColor = SystemColors.HighlightText;
            updateGrid.AutoGenerateColumns = true;
            updateGrid.CellBorderStyle = DataGridViewCellBorderStyle.Single;
            updateGrid.CellValueConverter = null;
            updateGrid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(255, 128, 0);
            updateGrid.ColumnHeadersDefaultCellStyle.Font = new Font("Meiryo UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 128);
            updateGrid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            updateGrid.ColumnHeadersDefaultCellStyle.SelectionBackColor = SystemColors.Highlight;
            updateGrid.ColumnHeadersDefaultCellStyle.SelectionForeColor = SystemColors.HighlightText;
            updateGrid.ColumnHeadersHeight = 25;
            updateGrid.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            updateGrid.DefaultCellStyle.BackColor = SystemColors.Window;
            updateGrid.DefaultCellStyle.Font = new Font("Yu Gothic UI", 9F);
            updateGrid.DefaultCellStyle.ForeColor = SystemColors.ControlText;
            updateGrid.DefaultCellStyle.SelectionBackColor = SystemColors.Highlight;
            updateGrid.DefaultCellStyle.SelectionForeColor = SystemColors.HighlightText;
            updateGrid.DefaultCellStyle.WrapMode = DataGridViewTriState.False;
            updateGrid.Dock = DockStyle.Fill;
            updateGrid.EmptyAreaBackColor = Color.Empty;
            updateGrid.EnableHeadersVisualStyles = false;
            updateGrid.EvenRowBackColor = Color.Empty;
            updateGrid.GridColor = SystemColors.WindowFrame;
            updateGrid.HeaderBackColor = Color.FromArgb(255, 128, 0);
            updateGrid.HeaderForeColor = Color.White;
            updateGrid.IsDpiScalingEnabled = true;
            updateGrid.Location = new Point(0, 0);
            updateGrid.Margin = new Padding(0, 0, 0, 0);
            updateGrid.MultiSelect = true;
            updateGrid.Name = "updateGrid";
            updateGrid.OddRowBackColor = Color.Empty;
            updateGrid.ReadOnly = false;
            updateGrid.RowHeadersDefaultCellStyle.BackColor = Color.FromArgb(255, 128, 0);
            updateGrid.RowHeadersDefaultCellStyle.Font = new Font("Meiryo UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 128);
            updateGrid.RowHeadersDefaultCellStyle.ForeColor = Color.White;
            updateGrid.RowHeadersDefaultCellStyle.SelectionBackColor = SystemColors.Highlight;
            updateGrid.RowHeadersDefaultCellStyle.SelectionForeColor = SystemColors.HighlightText;
            updateGrid.RowHeadersWidth = 41;
            updateGrid.RowsDefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 0, 192);
            updateGrid.RowsDefaultCellStyle.SelectionForeColor = SystemColors.HighlightText;
            updateGrid.SelectionBackColor = Color.FromArgb(0, 0, 192);
            updateGrid.Size = new Size(1278, 504);
            updateGrid.TabIndex = 0;
            updateGrid.VirtualCellValueProvider = null;
            updateGrid.VirtualCellValuePusher = null;
            updateGrid.VirtualRowCount = 0;
            // 
            // statusLabel
            // 
            statusLabel.Dock = DockStyle.Fill;
            statusLabel.Location = new Point(15, 672);
            statusLabel.Margin = new Padding(4, 0, 4, 0);
            statusLabel.Name = "statusLabel";
            statusLabel.Size = new Size(1541, 37);
            statusLabel.TabIndex = 3;
            statusLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // errorLogTextBox
            // 
            errorLogTextBox.BackColor = SystemColors.Window;
            errorLogTextBox.Dock = DockStyle.Fill;
            errorLogTextBox.Font = new Font("Yu Gothic UI", 9F);
            errorLogTextBox.Location = new Point(15, 713);
            errorLogTextBox.Margin = new Padding(4);
            errorLogTextBox.Multiline = true;
            errorLogTextBox.Name = "errorLogTextBox";
            errorLogTextBox.PlaceholderText = "エラー情報がここに表示されます。";
            errorLogTextBox.ReadOnly = true;
            errorLogTextBox.ScrollBars = ScrollBars.Vertical;
            errorLogTextBox.Size = new Size(1541, 139);
            errorLogTextBox.TabIndex = 4;
            // 
            // UpdateInfoForm
            // 
            AutoScaleDimensions = new SizeF(10F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1571, 867);
            Controls.Add(mainLayout);
            Font = new Font("Meiryo UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 128);
            Margin = new Padding(4);
            MinimumSize = new Size(1136, 587);
            Name = "UpdateInfoForm";
            Text = "ApplyUpdateInfo";
            mainLayout.ResumeLayout(false);
            mainLayout.PerformLayout();
            commandPanel.ResumeLayout(false);
            commandPanel.PerformLayout();
            actionPanel.ResumeLayout(false);
            actionPanel.PerformLayout();
            tableInfoPanel.ResumeLayout(false);
            loadingPanel.ResumeLayout(false);
            loadingPanel.PerformLayout();
            tableSplitContainer.Panel1.ResumeLayout(false);
            tableSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)tableSplitContainer).EndInit();
            tableSplitContainer.ResumeLayout(false);
            tablePanel.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion
    }
}
