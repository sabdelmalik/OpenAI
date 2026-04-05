namespace AdvancedAligner.ExampleEditor
{
    partial class ExampleEditorForm
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
            components = new System.ComponentModel.Container();
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            menuStrip1 = new MenuStrip();
            fileToolStripMenuItem = new ToolStripMenuItem();
            saveDBToolStripMenuItem = new ToolStripMenuItem();
            splitContainer1 = new SplitContainer();
            splitContainer2 = new SplitContainer();
            hebrewGridView1 = new BibleTaggingUtil.Editor.HebrewGridView();
            targetGridView1 = new BibleTaggingUtil.Editor.TargetGridView();
            buttonDelete = new Button();
            button1 = new Button();
            label3 = new Label();
            label2 = new Label();
            label1 = new Label();
            cbFirstBook = new ComboBox();
            cbFirstChapter = new ComboBox();
            cbFirstVerse = new ComboBox();
            comboBoxReferences = new ComboBox();
            menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer2).BeginInit();
            splitContainer2.Panel1.SuspendLayout();
            splitContainer2.Panel2.SuspendLayout();
            splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)hebrewGridView1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)targetGridView1).BeginInit();
            SuspendLayout();
            // 
            // menuStrip1
            // 
            menuStrip1.ImageScalingSize = new Size(24, 24);
            menuStrip1.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(1257, 33);
            menuStrip1.TabIndex = 0;
            menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { saveDBToolStripMenuItem });
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new Size(54, 29);
            fileToolStripMenuItem.Text = "File";
            // 
            // saveDBToolStripMenuItem
            // 
            saveDBToolStripMenuItem.Name = "saveDBToolStripMenuItem";
            saveDBToolStripMenuItem.Size = new Size(179, 34);
            saveDBToolStripMenuItem.Text = "Save DB";
            saveDBToolStripMenuItem.Click += saveDBToolStripMenuItem_Click;
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.Location = new Point(0, 33);
            splitContainer1.Name = "splitContainer1";
            splitContainer1.Orientation = Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(splitContainer2);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(buttonDelete);
            splitContainer1.Panel2.Controls.Add(button1);
            splitContainer1.Panel2.Controls.Add(label3);
            splitContainer1.Panel2.Controls.Add(label2);
            splitContainer1.Panel2.Controls.Add(label1);
            splitContainer1.Panel2.Controls.Add(cbFirstBook);
            splitContainer1.Panel2.Controls.Add(cbFirstChapter);
            splitContainer1.Panel2.Controls.Add(cbFirstVerse);
            splitContainer1.Panel2.Controls.Add(comboBoxReferences);
            splitContainer1.Size = new Size(1257, 490);
            splitContainer1.SplitterDistance = 398;
            splitContainer1.TabIndex = 1;
            // 
            // splitContainer2
            // 
            splitContainer2.Dock = DockStyle.Fill;
            splitContainer2.Location = new Point(0, 0);
            splitContainer2.Name = "splitContainer2";
            splitContainer2.Orientation = Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            splitContainer2.Panel1.Controls.Add(hebrewGridView1);
            // 
            // splitContainer2.Panel2
            // 
            splitContainer2.Panel2.Controls.Add(targetGridView1);
            splitContainer2.Size = new Size(1257, 398);
            splitContainer2.SplitterDistance = 236;
            splitContainer2.TabIndex = 0;
            // 
            // hebrewGridView1
            // 
            hebrewGridView1.AllowDrop = true;
            hebrewGridView1.AllowUserToAddRows = false;
            hebrewGridView1.AllowUserToDeleteRows = false;
            hebrewGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            hebrewGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            hebrewGridView1.BackgroundColor = SystemColors.ControlLight;
            hebrewGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            hebrewGridView1.ColumnHeadersVisible = false;
            hebrewGridView1.Cursor = Cursors.Hand;
            dataGridViewCellStyle1.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = SystemColors.Window;
            dataGridViewCellStyle1.Font = new Font("Calibri", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            dataGridViewCellStyle1.ForeColor = SystemColors.ControlText;
            dataGridViewCellStyle1.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = DataGridViewTriState.False;
            hebrewGridView1.DefaultCellStyle = dataGridViewCellStyle1;
            hebrewGridView1.Dock = DockStyle.Fill;
            hebrewGridView1.EditMode = DataGridViewEditMode.EditOnF2;
            hebrewGridView1.GridColor = SystemColors.ControlText;
            hebrewGridView1.Location = new Point(0, 0);
            hebrewGridView1.Name = "hebrewGridView1";
            hebrewGridView1.ReadOnly = true;
            hebrewGridView1.RowHeadersVisible = false;
            hebrewGridView1.RowHeadersWidth = 62;
            hebrewGridView1.SelectionMode = DataGridViewSelectionMode.CellSelect;
            hebrewGridView1.ShowCellToolTips = false;
            hebrewGridView1.Size = new Size(1257, 236);
            hebrewGridView1.TabIndex = 0;
            // 
            // targetGridView1
            // 
            targetGridView1.AllowDrop = true;
            targetGridView1.AllowUserToAddRows = false;
            targetGridView1.AllowUserToDeleteRows = false;
            targetGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            targetGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            targetGridView1.BackgroundColor = SystemColors.ControlLight;
            targetGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            targetGridView1.ColumnHeadersVisible = false;
            targetGridView1.Cursor = Cursors.Hand;
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = SystemColors.Window;
            dataGridViewCellStyle2.Font = new Font("Calibri", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            dataGridViewCellStyle2.ForeColor = SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = DataGridViewTriState.False;
            targetGridView1.DefaultCellStyle = dataGridViewCellStyle2;
            targetGridView1.Dock = DockStyle.Fill;
            targetGridView1.EditMode = DataGridViewEditMode.EditOnF2;
            targetGridView1.GridColor = SystemColors.ControlText;
            targetGridView1.Location = new Point(0, 0);
            targetGridView1.Name = "targetGridView1";
            targetGridView1.RowHeadersVisible = false;
            targetGridView1.RowHeadersWidth = 62;
            targetGridView1.SelectionMode = DataGridViewSelectionMode.CellSelect;
            targetGridView1.ShowCellToolTips = false;
            targetGridView1.Size = new Size(1257, 158);
            targetGridView1.TabIndex = 0;
            // 
            // buttonDelete
            // 
            buttonDelete.Location = new Point(354, 22);
            buttonDelete.Name = "buttonDelete";
            buttonDelete.Size = new Size(112, 34);
            buttonDelete.TabIndex = 11;
            buttonDelete.Text = "Delete";
            buttonDelete.UseVisualStyleBackColor = true;
            buttonDelete.Click += buttonDelete_Click;
            // 
            // button1
            // 
            button1.Location = new Point(685, 21);
            button1.Name = "button1";
            button1.Size = new Size(112, 34);
            button1.TabIndex = 10;
            button1.Text = "Add";
            button1.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label3.Location = new Point(1109, 19);
            label3.Name = "label3";
            label3.Size = new Size(40, 32);
            label3.TabIndex = 5;
            label3.Text = "Vs";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label2.Location = new Point(954, 19);
            label2.Name = "label2";
            label2.Size = new Size(43, 32);
            label2.TabIndex = 6;
            label2.Text = "Ch";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label1.Location = new Point(805, 22);
            label1.Name = "label1";
            label1.Size = new Size(42, 32);
            label1.TabIndex = 8;
            label1.Text = "Bk";
            // 
            // cbFirstBook
            // 
            cbFirstBook.AutoCompleteMode = AutoCompleteMode.Suggest;
            cbFirstBook.AutoCompleteSource = AutoCompleteSource.ListItems;
            cbFirstBook.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            cbFirstBook.FormattingEnabled = true;
            cbFirstBook.Location = new Point(846, 16);
            cbFirstBook.Name = "cbFirstBook";
            cbFirstBook.Size = new Size(103, 40);
            cbFirstBook.TabIndex = 3;
            cbFirstBook.SelectedIndexChanged += cbFirstBook_SelectedIndexChanged;
            // 
            // cbFirstChapter
            // 
            cbFirstChapter.AutoCompleteMode = AutoCompleteMode.Suggest;
            cbFirstChapter.AutoCompleteSource = AutoCompleteSource.ListItems;
            cbFirstChapter.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            cbFirstChapter.FormattingEnabled = true;
            cbFirstChapter.Location = new Point(1000, 17);
            cbFirstChapter.Name = "cbFirstChapter";
            cbFirstChapter.Size = new Size(103, 40);
            cbFirstChapter.TabIndex = 4;
            cbFirstChapter.SelectedIndexChanged += cbFirstChapter_SelectedIndexChanged;
            // 
            // cbFirstVerse
            // 
            cbFirstVerse.AutoCompleteMode = AutoCompleteMode.Suggest;
            cbFirstVerse.AutoCompleteSource = AutoCompleteSource.ListItems;
            cbFirstVerse.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            cbFirstVerse.FormattingEnabled = true;
            cbFirstVerse.Location = new Point(1142, 16);
            cbFirstVerse.Name = "cbFirstVerse";
            cbFirstVerse.Size = new Size(103, 40);
            cbFirstVerse.TabIndex = 9;
            // 
            // comboBoxReferences
            // 
            comboBoxReferences.FormattingEnabled = true;
            comboBoxReferences.Location = new Point(49, 25);
            comboBoxReferences.Name = "comboBoxReferences";
            comboBoxReferences.Size = new Size(191, 33);
            comboBoxReferences.TabIndex = 0;
            comboBoxReferences.SelectedIndexChanged += comboBoxReferences_SelectedIndexChanged;
            // 
            // ExampleEditorForm
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1257, 523);
            Controls.Add(splitContainer1);
            Controls.Add(menuStrip1);
            MainMenuStrip = menuStrip1;
            Name = "ExampleEditorForm";
            Text = "ExampleEditorForm";
            Load += ExampleEditorForm_Load;
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            splitContainer2.Panel1.ResumeLayout(false);
            splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer2).EndInit();
            splitContainer2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)hebrewGridView1).EndInit();
            ((System.ComponentModel.ISupportInitialize)targetGridView1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private MenuStrip menuStrip1;
        private SplitContainer splitContainer1;
        private SplitContainer splitContainer2;
        private BibleTaggingUtil.Editor.HebrewGridView hebrewGridView1;
        private BibleTaggingUtil.Editor.TargetGridView targetGridView1;
        private ComboBox comboBoxReferences;
        private Label label3;
        private Label label2;
        private Label label1;
        private ComboBox cbFirstBook;
        private ComboBox cbFirstChapter;
        private ComboBox cbFirstVerse;
        private Button button1;
        private Button buttonDelete;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem saveDBToolStripMenuItem;
    }
}