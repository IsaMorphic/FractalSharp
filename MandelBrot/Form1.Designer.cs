namespace MandelBrot
{
    partial class Form1
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
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.renderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.x480ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.x720ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.x960ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.startToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox1.Location = new System.Drawing.Point(13, 31);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(597, 437);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.renderToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(622, 28);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // renderToolStripMenuItem
            // 
            this.renderToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.optionsToolStripMenuItem,
            this.startToolStripMenuItem});
            this.renderToolStripMenuItem.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.renderToolStripMenuItem.Name = "renderToolStripMenuItem";
            this.renderToolStripMenuItem.Size = new System.Drawing.Size(68, 24);
            this.renderToolStripMenuItem.Text = "Render";
            // 
            // optionsToolStripMenuItem
            // 
            this.optionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.x480ToolStripMenuItem,
            this.x720ToolStripMenuItem,
            this.x960ToolStripMenuItem});
            this.optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
            this.optionsToolStripMenuItem.Size = new System.Drawing.Size(136, 26);
            this.optionsToolStripMenuItem.Text = "Options";
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
            // startToolStripMenuItem
            // 
            this.startToolStripMenuItem.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.startToolStripMenuItem.Name = "startToolStripMenuItem";
            this.startToolStripMenuItem.Size = new System.Drawing.Size(136, 26);
            this.startToolStripMenuItem.Text = "Start";
            this.startToolStripMenuItem.Click += new System.EventHandler(this.startToolStripMenuItem_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(622, 480);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.menuStrip1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Form1";
            this.Text = "Mandelbrot Renderer";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem renderToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem optionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem x480ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem x720ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem x960ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem startToolStripMenuItem;
    }
}

