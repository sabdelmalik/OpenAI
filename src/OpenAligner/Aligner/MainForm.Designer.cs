namespace AdvancedAligner
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

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
            menuStrip1 = new MenuStrip();
            fileToolStripMenuItem = new ToolStripMenuItem();
            parseResultToolStripMenuItem = new ToolStripMenuItem();
            settingsToolStripMenuItem = new ToolStripMenuItem();
            alignToolStripMenuItem = new ToolStripMenuItem();
            listToolStripMenuItem = new ToolStripMenuItem();
            rangeToolStripMenuItem = new ToolStripMenuItem();
            top5ToolStripMenuItem = new ToolStripMenuItem();
            datbaseToolStripMenuItem = new ToolStripMenuItem();
            clearToolStripMenuItem = new ToolStripMenuItem();
            reloadToolStripMenuItem = new ToolStripMenuItem();
            specialSearchToolStripMenuItem = new ToolStripMenuItem();
            openFileDialog1 = new OpenFileDialog();
            traceBox = new RichTextBox();
            menuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // menuStrip1
            // 
            menuStrip1.ImageScalingSize = new Size(24, 24);
            menuStrip1.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem, alignToolStripMenuItem, datbaseToolStripMenuItem, specialSearchToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(800, 33);
            menuStrip1.TabIndex = 0;
            menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { parseResultToolStripMenuItem, settingsToolStripMenuItem });
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new Size(54, 29);
            fileToolStripMenuItem.Text = "File";
            // 
            // parseResultToolStripMenuItem
            // 
            parseResultToolStripMenuItem.Name = "parseResultToolStripMenuItem";
            parseResultToolStripMenuItem.Size = new Size(207, 34);
            parseResultToolStripMenuItem.Text = "Parse Result";
            parseResultToolStripMenuItem.Click += parseResultToolStripMenuItem_Click;
            // 
            // settingsToolStripMenuItem
            // 
            settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            settingsToolStripMenuItem.Size = new Size(207, 34);
            settingsToolStripMenuItem.Text = "Settings";
            settingsToolStripMenuItem.Click += settingsToolStripMenuItem_Click;
            // 
            // alignToolStripMenuItem
            // 
            alignToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { listToolStripMenuItem, rangeToolStripMenuItem, top5ToolStripMenuItem });
            alignToolStripMenuItem.Name = "alignToolStripMenuItem";
            alignToolStripMenuItem.Size = new Size(69, 29);
            alignToolStripMenuItem.Text = "Align";
            // 
            // listToolStripMenuItem
            // 
            listToolStripMenuItem.Name = "listToolStripMenuItem";
            listToolStripMenuItem.Size = new Size(164, 34);
            listToolStripMenuItem.Text = "List";
            listToolStripMenuItem.Click += listToolStripMenuItem_Click;
            // 
            // rangeToolStripMenuItem
            // 
            rangeToolStripMenuItem.Name = "rangeToolStripMenuItem";
            rangeToolStripMenuItem.Size = new Size(164, 34);
            rangeToolStripMenuItem.Text = "Range";
            rangeToolStripMenuItem.Click += rangeToolStripMenuItem_Click;
            // 
            // top5ToolStripMenuItem
            // 
            top5ToolStripMenuItem.Name = "top5ToolStripMenuItem";
            top5ToolStripMenuItem.Size = new Size(164, 34);
            top5ToolStripMenuItem.Text = "Top5";
            top5ToolStripMenuItem.Click += top5ToolStripMenuItem_Click;
            // 
            // datbaseToolStripMenuItem
            // 
            datbaseToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { clearToolStripMenuItem, reloadToolStripMenuItem });
            datbaseToolStripMenuItem.Name = "datbaseToolStripMenuItem";
            datbaseToolStripMenuItem.Size = new Size(93, 29);
            datbaseToolStripMenuItem.Text = "Datbase";
            // 
            // clearToolStripMenuItem
            // 
            clearToolStripMenuItem.Name = "clearToolStripMenuItem";
            clearToolStripMenuItem.Size = new Size(270, 34);
            clearToolStripMenuItem.Text = "Clear";
            clearToolStripMenuItem.Click += clearToolStripMenuItem_Click;
            // 
            // reloadToolStripMenuItem
            // 
            reloadToolStripMenuItem.Name = "reloadToolStripMenuItem";
            reloadToolStripMenuItem.Size = new Size(270, 34);
            reloadToolStripMenuItem.Text = "Reload";
            reloadToolStripMenuItem.Click += reloadToolStripMenuItem_Click;
            // 
            // specialSearchToolStripMenuItem
            // 
            specialSearchToolStripMenuItem.Name = "specialSearchToolStripMenuItem";
            specialSearchToolStripMenuItem.Size = new Size(140, 29);
            specialSearchToolStripMenuItem.Text = "Special Search";
            specialSearchToolStripMenuItem.Click += specialSearchToolStripMenuItem_Click;
            // 
            // openFileDialog1
            // 
            openFileDialog1.FileName = "openFileDialog1";
            // 
            // traceBox
            // 
            traceBox.Dock = DockStyle.Fill;
            traceBox.Location = new Point(0, 33);
            traceBox.Margin = new Padding(4);
            traceBox.Name = "traceBox";
            traceBox.Size = new Size(800, 417);
            traceBox.TabIndex = 2;
            traceBox.Text = "";
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(traceBox);
            Controls.Add(menuStrip1);
            MainMenuStrip = menuStrip1;
            Name = "MainForm";
            Text = "Form1";
            Load += Form1_Load;
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private MenuStrip menuStrip1;
        private ToolStripMenuItem fileToolStripMenuItem;
        //private ToolStripMenuItem openTheVersionToAlignToolStripMenuItem;
        private OpenFileDialog openFileDialog1;
        private ToolStripMenuItem alignToolStripMenuItem;
        private ToolStripMenuItem listToolStripMenuItem;
        private ToolStripMenuItem rangeToolStripMenuItem;
        private ToolStripMenuItem parseResultToolStripMenuItem;
        private ToolStripMenuItem specialSearchToolStripMenuItem;
        private RichTextBox traceBox;
        private ToolStripMenuItem settingsToolStripMenuItem;
        private ToolStripMenuItem top5ToolStripMenuItem;
        private ToolStripMenuItem datbaseToolStripMenuItem;
        private ToolStripMenuItem clearToolStripMenuItem;
        private ToolStripMenuItem reloadToolStripMenuItem;
    }
}
