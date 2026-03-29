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
            splitContainer1 = new SplitContainer();
            splitContainer2 = new SplitContainer();
            hebrewGridView1 = new BibleTaggingUtil.Editor.HebrewGridView();
            targetGridView1 = new BibleTaggingUtil.Editor.TargetGridView();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
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
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(1257, 24);
            menuStrip1.TabIndex = 0;
            menuStrip1.Text = "menuStrip1";
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.Location = new Point(0, 24);
            splitContainer1.Name = "splitContainer1";
            splitContainer1.Orientation = Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(splitContainer2);
            splitContainer1.Size = new Size(1257, 499);
            splitContainer1.SplitterDistance = 406;
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
            splitContainer2.Size = new Size(1257, 406);
            splitContainer2.SplitterDistance = 241;
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
            hebrewGridView1.Size = new Size(1257, 241);
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
            targetGridView1.Size = new Size(1257, 161);
            targetGridView1.TabIndex = 0;
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
            splitContainer1.Panel1.ResumeLayout(false);
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
    }
}