namespace AdvancedAligner
{
    partial class ReferenceRangeSelection
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
            cbFirstVerse = new ComboBox();
            cbFirstChapter = new ComboBox();
            cbFirstBook = new ComboBox();
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            buttonOK = new Button();
            buttonCancel = new Button();
            labelFirstError = new Label();
            label4 = new Label();
            cbLastVerse = new ComboBox();
            cbLastChapter = new ComboBox();
            cbLastBook = new ComboBox();
            label5 = new Label();
            labelLastError = new Label();
            SuspendLayout();
            // 
            // cbFirstVerse
            // 
            cbFirstVerse.AutoCompleteMode = AutoCompleteMode.Suggest;
            cbFirstVerse.AutoCompleteSource = AutoCompleteSource.ListItems;
            cbFirstVerse.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            cbFirstVerse.FormattingEnabled = true;
            cbFirstVerse.Location = new Point(478, 66);
            cbFirstVerse.Name = "cbFirstVerse";
            cbFirstVerse.Size = new Size(103, 40);
            cbFirstVerse.TabIndex = 2;
            // 
            // cbFirstChapter
            // 
            cbFirstChapter.AutoCompleteMode = AutoCompleteMode.Suggest;
            cbFirstChapter.AutoCompleteSource = AutoCompleteSource.ListItems;
            cbFirstChapter.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            cbFirstChapter.FormattingEnabled = true;
            cbFirstChapter.Location = new Point(354, 66);
            cbFirstChapter.Name = "cbFirstChapter";
            cbFirstChapter.Size = new Size(103, 40);
            cbFirstChapter.TabIndex = 1;
            cbFirstChapter.SelectedIndexChanged += cbFirstChapter_SelectedIndexChanged;
            // 
            // cbFirstBook
            // 
            cbFirstBook.AutoCompleteMode = AutoCompleteMode.Suggest;
            cbFirstBook.AutoCompleteSource = AutoCompleteSource.ListItems;
            cbFirstBook.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            cbFirstBook.FormattingEnabled = true;
            cbFirstBook.Location = new Point(230, 66);
            cbFirstBook.Name = "cbFirstBook";
            cbFirstBook.Size = new Size(103, 40);
            cbFirstBook.TabIndex = 0;
            cbFirstBook.SelectedIndexChanged += cbFirstBook_SelectedIndexChanged;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label1.Location = new Point(230, 31);
            label1.Name = "label1";
            label1.Size = new Size(72, 32);
            label1.TabIndex = 2;
            label1.Text = "Book";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label2.Location = new Point(354, 31);
            label2.Name = "label2";
            label2.Size = new Size(103, 32);
            label2.TabIndex = 2;
            label2.Text = "Chapter";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label3.Location = new Point(478, 31);
            label3.Name = "label3";
            label3.Size = new Size(75, 32);
            label3.TabIndex = 2;
            label3.Text = "Verse";
            // 
            // buttonOK
            // 
            buttonOK.Location = new Point(354, 292);
            buttonOK.Name = "buttonOK";
            buttonOK.Size = new Size(112, 34);
            buttonOK.TabIndex = 6;
            buttonOK.Text = "Done";
            buttonOK.UseVisualStyleBackColor = true;
            buttonOK.Click += buttonOK_Click;
            // 
            // buttonCancel
            // 
            buttonCancel.Location = new Point(105, 292);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new Size(112, 34);
            buttonCancel.TabIndex = 7;
            buttonCancel.Text = "Cancel";
            buttonCancel.UseVisualStyleBackColor = true;
            // 
            // labelFirstError
            // 
            labelFirstError.AutoSize = true;
            labelFirstError.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            labelFirstError.ForeColor = Color.Red;
            labelFirstError.Location = new Point(24, 120);
            labelFirstError.Name = "labelFirstError";
            labelFirstError.Size = new Size(72, 32);
            labelFirstError.TabIndex = 2;
            labelFirstError.Text = "Book";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label4.Location = new Point(24, 74);
            label4.Name = "label4";
            label4.Size = new Size(183, 32);
            label4.TabIndex = 2;
            label4.Text = "First Reference";
            // 
            // cbLastVerse
            // 
            cbLastVerse.AutoCompleteMode = AutoCompleteMode.Suggest;
            cbLastVerse.AutoCompleteSource = AutoCompleteSource.ListItems;
            cbLastVerse.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            cbLastVerse.FormattingEnabled = true;
            cbLastVerse.Location = new Point(478, 162);
            cbLastVerse.Name = "cbLastVerse";
            cbLastVerse.Size = new Size(103, 40);
            cbLastVerse.TabIndex = 5;
            // 
            // cbLastChapter
            // 
            cbLastChapter.AutoCompleteMode = AutoCompleteMode.Suggest;
            cbLastChapter.AutoCompleteSource = AutoCompleteSource.ListItems;
            cbLastChapter.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            cbLastChapter.FormattingEnabled = true;
            cbLastChapter.Location = new Point(354, 162);
            cbLastChapter.Name = "cbLastChapter";
            cbLastChapter.Size = new Size(103, 40);
            cbLastChapter.TabIndex = 4;
            cbLastChapter.SelectedIndexChanged += cbLastChapter_SelectedIndexChanged;
            // 
            // cbLastBook
            // 
            cbLastBook.AutoCompleteMode = AutoCompleteMode.Suggest;
            cbLastBook.AutoCompleteSource = AutoCompleteSource.ListItems;
            cbLastBook.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            cbLastBook.FormattingEnabled = true;
            cbLastBook.Location = new Point(230, 162);
            cbLastBook.Name = "cbLastBook";
            cbLastBook.Size = new Size(103, 40);
            cbLastBook.TabIndex = 3;
            cbLastBook.SelectedIndexChanged += cbLastBook_SelectedIndexChanged;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label5.Location = new Point(24, 170);
            label5.Name = "label5";
            label5.Size = new Size(179, 32);
            label5.TabIndex = 2;
            label5.Text = "Last Reference";
            // 
            // labelLastError
            // 
            labelLastError.AutoSize = true;
            labelLastError.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            labelLastError.ForeColor = Color.Red;
            labelLastError.Location = new Point(24, 215);
            labelLastError.Name = "labelLastError";
            labelLastError.Size = new Size(72, 32);
            labelLastError.TabIndex = 2;
            labelLastError.Text = "Book";
            // 
            // ReferenceRangeSelection
            // 
            AcceptButton = buttonOK;
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = buttonCancel;
            ClientSize = new Size(619, 357);
            Controls.Add(buttonCancel);
            Controls.Add(buttonOK);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(labelLastError);
            Controls.Add(labelFirstError);
            Controls.Add(label5);
            Controls.Add(label4);
            Controls.Add(label1);
            Controls.Add(cbLastBook);
            Controls.Add(cbLastChapter);
            Controls.Add(cbFirstBook);
            Controls.Add(cbLastVerse);
            Controls.Add(cbFirstChapter);
            Controls.Add(cbFirstVerse);
            Name = "ReferenceRangeSelection";
            Text = "ReferenceRangeSelection";
            Load += ReferenceRangeSelection_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private ComboBox cbFirstVerse;
        private ComboBox cbFirstChapter;
        private ComboBox cbFirstBook;
        private Label label1;
        private Label label2;
        private Label label3;
        private Button buttonOK;
        private Button buttonCancel;
        private Label labelFirstError;
        private Label label4;
        private ComboBox cbLastVerse;
        private ComboBox cbLastChapter;
        private ComboBox cbLastBook;
        private Label label5;
        private Label labelLastError;
    }
}