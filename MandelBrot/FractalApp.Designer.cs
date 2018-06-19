namespace MandelBrot
{
    partial class FractalApp
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FractalApp));
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newRenderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadRenderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.closeCurrentToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.loadPaletteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.renderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.startToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.stopToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.presicionStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.doublePrecisionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.extraPrescisionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ResolutionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.x480ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.x720ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.x960ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.colorDialog = new System.Windows.Forms.ColorDialog();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.ViewControl = new System.Windows.Forms.TabControl();
            this.advancedTab = new System.Windows.Forms.TabPage();
            this.timeDescLabel = new System.Windows.Forms.Label();
            this.timeLabel = new System.Windows.Forms.Label();
            this.livePreviewCheckBox = new System.Windows.Forms.CheckBox();
            this.livePreviewLabel = new System.Windows.Forms.Label();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.YOffLabel = new System.Windows.Forms.Label();
            this.yOffInput = new System.Windows.Forms.NumericUpDown();
            this.startFrameLabel = new System.Windows.Forms.Label();
            this.max_iteration_label = new System.Windows.Forms.Label();
            this.iterationCountInput = new System.Windows.Forms.NumericUpDown();
            this.startFrameInput = new System.Windows.Forms.NumericUpDown();
            this.XOffLabel = new System.Windows.Forms.Label();
            this.xOffInput = new System.Windows.Forms.NumericUpDown();
            this.coreCountLabel = new System.Windows.Forms.Label();
            this.threadCountInput = new System.Windows.Forms.NumericUpDown();
            this.viewTab = new System.Windows.Forms.TabPage();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.intervalTimer = new System.Windows.Forms.Timer(this.components);
            this.TrayIcon = new System.Windows.Forms.NotifyIcon(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.menuStrip1.SuspendLayout();
            this.ViewControl.SuspendLayout();
            this.advancedTab.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.yOffInput)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.iterationCountInput)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.startFrameInput)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.xOffInput)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.threadCountInput)).BeginInit();
            this.viewTab.SuspendLayout();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox1.Location = new System.Drawing.Point(3, 3);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(608, 417);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.renderToolStripMenuItem,
            this.presicionStripMenuItem,
            this.ResolutionToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(622, 28);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newRenderToolStripMenuItem,
            this.loadRenderToolStripMenuItem,
            this.closeCurrentToolStripMenuItem,
            this.toolStripSeparator1,
            this.loadPaletteToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(44, 24);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // newRenderToolStripMenuItem
            // 
            this.newRenderToolStripMenuItem.Name = "newRenderToolStripMenuItem";
            this.newRenderToolStripMenuItem.Size = new System.Drawing.Size(172, 26);
            this.newRenderToolStripMenuItem.Text = "New Render";
            this.newRenderToolStripMenuItem.Click += new System.EventHandler(this.newRenderToolStripMenuItem_Click);
            // 
            // loadRenderToolStripMenuItem
            // 
            this.loadRenderToolStripMenuItem.Name = "loadRenderToolStripMenuItem";
            this.loadRenderToolStripMenuItem.Size = new System.Drawing.Size(172, 26);
            this.loadRenderToolStripMenuItem.Text = "Load Render";
            this.loadRenderToolStripMenuItem.Click += new System.EventHandler(this.loadRenderToolStripMenuItem_Click);
            // 
            // closeCurrentToolStripMenuItem
            // 
            this.closeCurrentToolStripMenuItem.Enabled = false;
            this.closeCurrentToolStripMenuItem.Name = "closeCurrentToolStripMenuItem";
            this.closeCurrentToolStripMenuItem.Size = new System.Drawing.Size(172, 26);
            this.closeCurrentToolStripMenuItem.Text = "Close Current";
            this.closeCurrentToolStripMenuItem.Click += new System.EventHandler(this.closeCurrentToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(169, 6);
            // 
            // loadPaletteToolStripMenuItem
            // 
            this.loadPaletteToolStripMenuItem.Name = "loadPaletteToolStripMenuItem";
            this.loadPaletteToolStripMenuItem.Size = new System.Drawing.Size(172, 26);
            this.loadPaletteToolStripMenuItem.Text = "Load Palette";
            this.loadPaletteToolStripMenuItem.Click += new System.EventHandler(this.loadPaletteToolStripMenuItem_Click);
            // 
            // renderToolStripMenuItem
            // 
            this.renderToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.startToolStripMenuItem,
            this.stopToolStripMenuItem});
            this.renderToolStripMenuItem.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.renderToolStripMenuItem.Name = "renderToolStripMenuItem";
            this.renderToolStripMenuItem.Size = new System.Drawing.Size(68, 24);
            this.renderToolStripMenuItem.Text = "Render";
            // 
            // startToolStripMenuItem
            // 
            this.startToolStripMenuItem.Enabled = false;
            this.startToolStripMenuItem.Name = "startToolStripMenuItem";
            this.startToolStripMenuItem.Size = new System.Drawing.Size(115, 26);
            this.startToolStripMenuItem.Text = "Start";
            this.startToolStripMenuItem.Click += new System.EventHandler(this.startToolStripMenuItem_Click);
            // 
            // stopToolStripMenuItem
            // 
            this.stopToolStripMenuItem.Enabled = false;
            this.stopToolStripMenuItem.Name = "stopToolStripMenuItem";
            this.stopToolStripMenuItem.Size = new System.Drawing.Size(115, 26);
            this.stopToolStripMenuItem.Text = "Stop";
            this.stopToolStripMenuItem.Click += new System.EventHandler(this.stopToolStripMenuItem_Click);
            // 
            // presicionStripMenuItem
            // 
            this.presicionStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.doublePrecisionToolStripMenuItem,
            this.extraPrescisionToolStripMenuItem});
            this.presicionStripMenuItem.Name = "presicionStripMenuItem";
            this.presicionStripMenuItem.Size = new System.Drawing.Size(80, 24);
            this.presicionStripMenuItem.Text = "Precision";
            // 
            // doublePrecisionToolStripMenuItem
            // 
            this.doublePrecisionToolStripMenuItem.Checked = true;
            this.doublePrecisionToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.doublePrecisionToolStripMenuItem.Name = "doublePrecisionToolStripMenuItem";
            this.doublePrecisionToolStripMenuItem.Size = new System.Drawing.Size(207, 26);
            this.doublePrecisionToolStripMenuItem.Text = "Standard Precision";
            this.doublePrecisionToolStripMenuItem.Click += new System.EventHandler(this.doublePrecisionToolStripMenuItem_Click);
            // 
            // extraPrescisionToolStripMenuItem
            // 
            this.extraPrescisionToolStripMenuItem.Name = "extraPrescisionToolStripMenuItem";
            this.extraPrescisionToolStripMenuItem.Size = new System.Drawing.Size(207, 26);
            this.extraPrescisionToolStripMenuItem.Text = "Extra Prescision";
            this.extraPrescisionToolStripMenuItem.Click += new System.EventHandler(this.decimalPrescisionToolStripMenuItem_Click);
            // 
            // ResolutionToolStripMenuItem
            // 
            this.ResolutionToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.x480ToolStripMenuItem,
            this.x720ToolStripMenuItem,
            this.x960ToolStripMenuItem});
            this.ResolutionToolStripMenuItem.Name = "ResolutionToolStripMenuItem";
            this.ResolutionToolStripMenuItem.Size = new System.Drawing.Size(91, 24);
            this.ResolutionToolStripMenuItem.Text = "Resolution";
            // 
            // x480ToolStripMenuItem
            // 
            this.x480ToolStripMenuItem.Checked = true;
            this.x480ToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.x480ToolStripMenuItem.Name = "x480ToolStripMenuItem";
            this.x480ToolStripMenuItem.Size = new System.Drawing.Size(147, 26);
            this.x480ToolStripMenuItem.Text = "640x480";
            this.x480ToolStripMenuItem.Click += new System.EventHandler(this.x480ToolStripMenuItem_Click);
            // 
            // x720ToolStripMenuItem
            // 
            this.x720ToolStripMenuItem.Name = "x720ToolStripMenuItem";
            this.x720ToolStripMenuItem.Size = new System.Drawing.Size(147, 26);
            this.x720ToolStripMenuItem.Text = "900x720";
            this.x720ToolStripMenuItem.Click += new System.EventHandler(this.x720ToolStripMenuItem_Click);
            // 
            // x960ToolStripMenuItem
            // 
            this.x960ToolStripMenuItem.Name = "x960ToolStripMenuItem";
            this.x960ToolStripMenuItem.Size = new System.Drawing.Size(147, 26);
            this.x960ToolStripMenuItem.Text = "1280x960";
            this.x960ToolStripMenuItem.Click += new System.EventHandler(this.x960ToolStripMenuItem_Click);
            // 
            // colorDialog
            // 
            this.colorDialog.Color = System.Drawing.Color.Aqua;
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "blues.map";
            this.openFileDialog1.InitialDirectory = ".\\Palettes\\";
            // 
            // ViewControl
            // 
            this.ViewControl.Controls.Add(this.advancedTab);
            this.ViewControl.Controls.Add(this.viewTab);
            this.ViewControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ViewControl.Location = new System.Drawing.Point(0, 28);
            this.ViewControl.Name = "ViewControl";
            this.ViewControl.SelectedIndex = 0;
            this.ViewControl.Size = new System.Drawing.Size(622, 452);
            this.ViewControl.TabIndex = 2;
            // 
            // advancedTab
            // 
            this.advancedTab.BackColor = System.Drawing.Color.White;
            this.advancedTab.Controls.Add(this.timeDescLabel);
            this.advancedTab.Controls.Add(this.timeLabel);
            this.advancedTab.Controls.Add(this.livePreviewCheckBox);
            this.advancedTab.Controls.Add(this.livePreviewLabel);
            this.advancedTab.Controls.Add(this.tableLayoutPanel1);
            this.advancedTab.Controls.Add(this.coreCountLabel);
            this.advancedTab.Controls.Add(this.threadCountInput);
            this.advancedTab.Location = new System.Drawing.Point(4, 25);
            this.advancedTab.Name = "advancedTab";
            this.advancedTab.Padding = new System.Windows.Forms.Padding(3);
            this.advancedTab.Size = new System.Drawing.Size(614, 423);
            this.advancedTab.TabIndex = 1;
            this.advancedTab.Text = "Fractal Configuration";
            // 
            // timeDescLabel
            // 
            this.timeDescLabel.AutoSize = true;
            this.timeDescLabel.Location = new System.Drawing.Point(220, 138);
            this.timeDescLabel.Name = "timeDescLabel";
            this.timeDescLabel.Size = new System.Drawing.Size(223, 17);
            this.timeDescLabel.TabIndex = 14;
            this.timeDescLabel.Text = "Rendering Time for current frame:";
            // 
            // timeLabel
            // 
            this.timeLabel.Location = new System.Drawing.Point(261, 155);
            this.timeLabel.Name = "timeLabel";
            this.timeLabel.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.timeLabel.Size = new System.Drawing.Size(182, 23);
            this.timeLabel.TabIndex = 13;
            this.timeLabel.Text = "00:00:00.000";
            this.timeLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // livePreviewCheckBox
            // 
            this.livePreviewCheckBox.AutoSize = true;
            this.livePreviewCheckBox.Checked = true;
            this.livePreviewCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.livePreviewCheckBox.Location = new System.Drawing.Point(4, 158);
            this.livePreviewCheckBox.Margin = new System.Windows.Forms.Padding(5, 3, 3, 3);
            this.livePreviewCheckBox.Name = "livePreviewCheckBox";
            this.livePreviewCheckBox.Size = new System.Drawing.Size(82, 21);
            this.livePreviewCheckBox.TabIndex = 12;
            this.livePreviewCheckBox.Text = "Enabled";
            this.livePreviewCheckBox.UseVisualStyleBackColor = true;
            this.livePreviewCheckBox.CheckedChanged += new System.EventHandler(this.livePreviewCheckBox_CheckedChanged);
            // 
            // livePreviewLabel
            // 
            this.livePreviewLabel.AutoSize = true;
            this.livePreviewLabel.Location = new System.Drawing.Point(1, 138);
            this.livePreviewLabel.Name = "livePreviewLabel";
            this.livePreviewLabel.Size = new System.Drawing.Size(87, 17);
            this.livePreviewLabel.TabIndex = 11;
            this.livePreviewLabel.Text = "Live Preview";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Single;
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.YOffLabel, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.yOffInput, 1, 3);
            this.tableLayoutPanel1.Controls.Add(this.startFrameLabel, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.max_iteration_label, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.iterationCountInput, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.startFrameInput, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.XOffLabel, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.xOffInput, 0, 3);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 6);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 4;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 18.85755F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 30.989F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 18.85754F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 31.2959F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(443, 129);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // YOffLabel
            // 
            this.YOffLabel.AutoSize = true;
            this.YOffLabel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.YOffLabel.Location = new System.Drawing.Point(225, 70);
            this.YOffLabel.Name = "YOffLabel";
            this.YOffLabel.Size = new System.Drawing.Size(214, 17);
            this.YOffLabel.TabIndex = 8;
            this.YOffLabel.Text = "Y Offset";
            // 
            // yOffInput
            // 
            this.yOffInput.DecimalPlaces = 20;
            this.yOffInput.Increment = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.yOffInput.Location = new System.Drawing.Point(225, 91);
            this.yOffInput.Maximum = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.yOffInput.Minimum = new decimal(new int[] {
            2,
            0,
            0,
            -2147483648});
            this.yOffInput.Name = "yOffInput";
            this.yOffInput.Size = new System.Drawing.Size(196, 22);
            this.yOffInput.TabIndex = 9;
            // 
            // startFrameLabel
            // 
            this.startFrameLabel.AutoSize = true;
            this.startFrameLabel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.startFrameLabel.Location = new System.Drawing.Point(225, 7);
            this.startFrameLabel.Name = "startFrameLabel";
            this.startFrameLabel.Size = new System.Drawing.Size(214, 17);
            this.startFrameLabel.TabIndex = 4;
            this.startFrameLabel.Text = "Starting Frame";
            // 
            // max_iteration_label
            // 
            this.max_iteration_label.AutoSize = true;
            this.max_iteration_label.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.max_iteration_label.Location = new System.Drawing.Point(4, 7);
            this.max_iteration_label.Name = "max_iteration_label";
            this.max_iteration_label.Size = new System.Drawing.Size(214, 17);
            this.max_iteration_label.TabIndex = 0;
            this.max_iteration_label.Text = "Maximum Iterations";
            // 
            // iterationCountInput
            // 
            this.iterationCountInput.Location = new System.Drawing.Point(4, 28);
            this.iterationCountInput.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.iterationCountInput.Name = "iterationCountInput";
            this.iterationCountInput.Size = new System.Drawing.Size(198, 22);
            this.iterationCountInput.TabIndex = 1;
            // 
            // startFrameInput
            // 
            this.startFrameInput.Location = new System.Drawing.Point(225, 28);
            this.startFrameInput.Maximum = new decimal(new int[] {
            5000,
            0,
            0,
            0});
            this.startFrameInput.Name = "startFrameInput";
            this.startFrameInput.Size = new System.Drawing.Size(196, 22);
            this.startFrameInput.TabIndex = 5;
            // 
            // XOffLabel
            // 
            this.XOffLabel.AutoSize = true;
            this.XOffLabel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.XOffLabel.Location = new System.Drawing.Point(4, 70);
            this.XOffLabel.Name = "XOffLabel";
            this.XOffLabel.Size = new System.Drawing.Size(214, 17);
            this.XOffLabel.TabIndex = 6;
            this.XOffLabel.Text = "X Offset";
            // 
            // xOffInput
            // 
            this.xOffInput.DecimalPlaces = 20;
            this.xOffInput.Increment = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.xOffInput.Location = new System.Drawing.Point(4, 91);
            this.xOffInput.Maximum = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.xOffInput.Minimum = new decimal(new int[] {
            2,
            0,
            0,
            -2147483648});
            this.xOffInput.Name = "xOffInput";
            this.xOffInput.Size = new System.Drawing.Size(198, 22);
            this.xOffInput.TabIndex = 7;
            // 
            // coreCountLabel
            // 
            this.coreCountLabel.AutoSize = true;
            this.coreCountLabel.Location = new System.Drawing.Point(1, 182);
            this.coreCountLabel.Name = "coreCountLabel";
            this.coreCountLabel.Size = new System.Drawing.Size(157, 17);
            this.coreCountLabel.TabIndex = 12;
            this.coreCountLabel.Text = "Maximum Thread Count";
            // 
            // threadCountInput
            // 
            this.threadCountInput.Location = new System.Drawing.Point(4, 203);
            this.threadCountInput.Maximum = new decimal(new int[] {
            8,
            0,
            0,
            0});
            this.threadCountInput.Name = "threadCountInput";
            this.threadCountInput.Size = new System.Drawing.Size(198, 22);
            this.threadCountInput.TabIndex = 11;
            // 
            // viewTab
            // 
            this.viewTab.Controls.Add(this.pictureBox1);
            this.viewTab.Location = new System.Drawing.Point(4, 25);
            this.viewTab.Name = "viewTab";
            this.viewTab.Padding = new System.Windows.Forms.Padding(3);
            this.viewTab.Size = new System.Drawing.Size(614, 423);
            this.viewTab.TabIndex = 0;
            this.viewTab.Text = "View";
            this.viewTab.UseVisualStyleBackColor = true;
            // 
            // saveFileDialog1
            // 
            this.saveFileDialog1.DefaultExt = "avi";
            this.saveFileDialog1.Filter = "AVI Video Files|*.avi";
            this.saveFileDialog1.FileOk += new System.ComponentModel.CancelEventHandler(this.saveFileDialog1_FileOk);
            // 
            // intervalTimer
            // 
            this.intervalTimer.Tick += new System.EventHandler(this.intervalTimer_Tick);
            // 
            // TrayIcon
            // 
            this.TrayIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("TrayIcon.Icon")));
            this.TrayIcon.Text = "Mandelbrot Renderer";
            this.TrayIcon.Click += new System.EventHandler(this.TrayIcon_Click);
            // 
            // FractalApp
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(622, 480);
            this.Controls.Add(this.ViewControl);
            this.Controls.Add(this.menuStrip1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "FractalApp";
            this.Text = "Mandelbrot Renderer";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FractalApp_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ViewControl.ResumeLayout(false);
            this.advancedTab.ResumeLayout(false);
            this.advancedTab.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.yOffInput)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.iterationCountInput)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.startFrameInput)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.xOffInput)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.threadCountInput)).EndInit();
            this.viewTab.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem renderToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem startToolStripMenuItem;
        private System.Windows.Forms.ColorDialog colorDialog;
        private System.Windows.Forms.ToolStripMenuItem ResolutionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem x480ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem x720ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem x960ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem stopToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newRenderToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem closeCurrentToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem presicionStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem doublePrecisionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem extraPrescisionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadPaletteToolStripMenuItem;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.TabControl ViewControl;
        private System.Windows.Forms.TabPage viewTab;
        private System.Windows.Forms.TabPage advancedTab;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label max_iteration_label;
        private System.Windows.Forms.Label startFrameLabel;
        private System.Windows.Forms.NumericUpDown iterationCountInput;
        private System.Windows.Forms.Label YOffLabel;
        private System.Windows.Forms.NumericUpDown yOffInput;
        private System.Windows.Forms.NumericUpDown startFrameInput;
        private System.Windows.Forms.Label XOffLabel;
        private System.Windows.Forms.NumericUpDown xOffInput;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.CheckBox livePreviewCheckBox;
        private System.Windows.Forms.Label livePreviewLabel;
        private System.Windows.Forms.Label timeDescLabel;
        private System.Windows.Forms.Label timeLabel;
        private System.Windows.Forms.Timer intervalTimer;
        private System.Windows.Forms.NumericUpDown threadCountInput;
        private System.Windows.Forms.Label coreCountLabel;
        private System.Windows.Forms.NotifyIcon TrayIcon;
        private System.Windows.Forms.ToolStripMenuItem loadRenderToolStripMenuItem;
    }
}

