namespace ApplyUpdateInfo;

partial class ApplyPreviewDialog
{
    private System.ComponentModel.IContainer components = null;
    private Label headerLabel;
    private LaControl.LaTableControl previewGrid;
    private Panel buttonsPanel;
    private Button cancelButton;
    private Button okButton;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components is not null)
        {
            components.Dispose();
        }

        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        headerLabel = new Label();
        previewGrid = new LaControl.LaTableControl();
        buttonsPanel = new Panel();
        cancelButton = new Button();
        okButton = new Button();
        buttonsPanel.SuspendLayout();
        SuspendLayout();
        // 
        // headerLabel
        // 
        headerLabel.Dock = DockStyle.Top;
        headerLabel.Location = new Point(0, 0);
        headerLabel.Name = "headerLabel";
        headerLabel.Padding = new Padding(12);
        headerLabel.Size = new Size(980, 100);
        headerLabel.TabIndex = 0;
        headerLabel.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // previewGrid
        // 
        previewGrid.AllowUserToAddRows = false;
        previewGrid.AllowUserToDeleteRows = false;
        previewGrid.AutoGenerateColumns = false;
        previewGrid.Dock = DockStyle.Fill;
        previewGrid.Location = new Point(0, 100);
        previewGrid.Name = "previewGrid";
        previewGrid.ReadOnly = true;
        previewGrid.Size = new Size(980, 378);
        previewGrid.TabIndex = 1;
        // 
        // buttonsPanel
        // 
        buttonsPanel.Controls.Add(okButton);
        buttonsPanel.Controls.Add(cancelButton);
        buttonsPanel.Dock = DockStyle.Bottom;
        buttonsPanel.Location = new Point(0, 478);
        buttonsPanel.Name = "buttonsPanel";
        buttonsPanel.Padding = new Padding(6);
        buttonsPanel.Size = new Size(980, 42);
        buttonsPanel.TabIndex = 2;
        // 
        // cancelButton
        // 
        cancelButton.DialogResult = DialogResult.Cancel;
        cancelButton.Dock = DockStyle.Right;
        cancelButton.Location = new Point(768, 6);
        cancelButton.Name = "cancelButton";
        cancelButton.Size = new Size(100, 30);
        cancelButton.TabIndex = 0;
        cancelButton.Text = "キャンセル";
        cancelButton.UseVisualStyleBackColor = true;
        // 
        // okButton
        // 
        okButton.DialogResult = DialogResult.OK;
        okButton.Dock = DockStyle.Right;
        okButton.Location = new Point(874, 6);
        okButton.Name = "okButton";
        okButton.Size = new Size(100, 30);
        okButton.TabIndex = 1;
        okButton.Text = "適用へ進む";
        okButton.UseVisualStyleBackColor = true;
        // 
        // ApplyPreviewDialog
        // 
        AcceptButton = okButton;
        AutoScaleDimensions = new SizeF(10F, 20F);
        AutoScaleMode = AutoScaleMode.Font;
        CancelButton = cancelButton;
        ClientSize = new Size(980, 520);
        Controls.Add(previewGrid);
        Controls.Add(buttonsPanel);
        Controls.Add(headerLabel);
        Font = new Font("Meiryo UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 128);
        FormBorderStyle = FormBorderStyle.Sizable;
        MaximizeBox = true;
        MinimizeBox = false;
        Name = "ApplyPreviewDialog";
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent;
        Text = "適用内容の確認";
        buttonsPanel.ResumeLayout(false);
        ResumeLayout(false);
    }
}
