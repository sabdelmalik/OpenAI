namespace AdvancedAligner
{
    partial class ReferenceListSelection
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
            listBoxReferences = new ListBox();
            cbVerse = new ComboBox();
            cbChapter = new ComboBox();
            cbBook = new ComboBox();
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            label4 = new Label();
            buttonOK = new Button();
            buttonCancel = new Button();
            buttonAdd = new Button();
            labelError = new Label();
            buttonDeleteSelection = new Button();
            SuspendLayout();
            // 
            // listBoxReferences
            // 
            listBoxReferences.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            listBoxReferences.FormattingEnabled = true;
            listBoxReferences.Location = new Point(424, 52);
            listBoxReferences.Name = "listBoxReferences";
            listBoxReferences.SelectionMode = SelectionMode.MultiExtended;
            listBoxReferences.Size = new Size(180, 228);
            listBoxReferences.Sorted = true;
            listBoxReferences.TabIndex = 4;
            // 
            // cbVerse
            // 
            cbVerse.AutoCompleteMode = AutoCompleteMode.Suggest;
            cbVerse.AutoCompleteSource = AutoCompleteSource.ListItems;
            cbVerse.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            cbVerse.FormattingEnabled = true;
            cbVerse.Location = new Point(282, 127);
            cbVerse.Name = "cbVerse";
            cbVerse.Size = new Size(103, 40);
            cbVerse.TabIndex = 2;
            // 
            // cbChapter
            // 
            cbChapter.AutoCompleteMode = AutoCompleteMode.Suggest;
            cbChapter.AutoCompleteSource = AutoCompleteSource.ListItems;
            cbChapter.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            cbChapter.FormattingEnabled = true;
            cbChapter.Location = new Point(158, 127);
            cbChapter.Name = "cbChapter";
            cbChapter.Size = new Size(103, 40);
            cbChapter.TabIndex = 1;
            cbChapter.SelectedIndexChanged += cbChapter_SelectedIndexChanged;
            // 
            // cbBook
            // 
            cbBook.AutoCompleteMode = AutoCompleteMode.Suggest;
            cbBook.AutoCompleteSource = AutoCompleteSource.ListItems;
            cbBook.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            cbBook.FormattingEnabled = true;
            cbBook.Location = new Point(34, 127);
            cbBook.Name = "cbBook";
            cbBook.Size = new Size(103, 40);
            cbBook.TabIndex = 0;
            cbBook.SelectedIndexChanged += cbBook_SelectedIndexChanged;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label1.Location = new Point(34, 92);
            label1.Name = "label1";
            label1.Size = new Size(72, 32);
            label1.TabIndex = 2;
            label1.Text = "Book";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label2.Location = new Point(158, 92);
            label2.Name = "label2";
            label2.Size = new Size(103, 32);
            label2.TabIndex = 2;
            label2.Text = "Chapter";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label3.Location = new Point(282, 92);
            label3.Name = "label3";
            label3.Size = new Size(75, 32);
            label3.TabIndex = 2;
            label3.Text = "Verse";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label4.Location = new Point(424, 17);
            label4.Name = "label4";
            label4.Size = new Size(138, 32);
            label4.TabIndex = 2;
            label4.Text = "References";
            // 
            // buttonOK
            // 
            buttonOK.DialogResult = DialogResult.OK;
            buttonOK.Location = new Point(273, 309);
            buttonOK.Name = "buttonOK";
            buttonOK.Size = new Size(112, 34);
            buttonOK.TabIndex = 5;
            buttonOK.Text = "Done";
            buttonOK.UseVisualStyleBackColor = true;
            buttonOK.Click += buttonOK_Click;
            // 
            // buttonCancel
            // 
            buttonCancel.Location = new Point(34, 309);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new Size(112, 34);
            buttonCancel.TabIndex = 6;
            buttonCancel.Text = "Cancel";
            buttonCancel.UseVisualStyleBackColor = true;
            // 
            // buttonAdd
            // 
            buttonAdd.Location = new Point(34, 228);
            buttonAdd.Name = "buttonAdd";
            buttonAdd.Size = new Size(351, 34);
            buttonAdd.TabIndex = 3;
            buttonAdd.Text = "Add To List";
            buttonAdd.UseVisualStyleBackColor = true;
            buttonAdd.Click += buttonAdd_Click;
            // 
            // labelError
            // 
            labelError.AutoSize = true;
            labelError.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            labelError.ForeColor = Color.Red;
            labelError.Location = new Point(34, 179);
            labelError.Name = "labelError";
            labelError.Size = new Size(72, 32);
            labelError.TabIndex = 2;
            labelError.Text = "Book";
            // 
            // buttonDeleteSelection
            // 
            buttonDeleteSelection.Location = new Point(424, 309);
            buttonDeleteSelection.Name = "buttonDeleteSelection";
            buttonDeleteSelection.Size = new Size(180, 34);
            buttonDeleteSelection.TabIndex = 7;
            buttonDeleteSelection.Text = "Delete Selection";
            buttonDeleteSelection.UseVisualStyleBackColor = true;
            buttonDeleteSelection.Click += buttonDeleteSelection_Click;
            // 
            // ReferenceListSelection
            // 
            AcceptButton = buttonOK;
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = buttonCancel;
            ClientSize = new Size(654, 384);
            Controls.Add(buttonDeleteSelection);
            Controls.Add(buttonAdd);
            Controls.Add(buttonCancel);
            Controls.Add(buttonOK);
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(labelError);
            Controls.Add(label1);
            Controls.Add(cbBook);
            Controls.Add(cbChapter);
            Controls.Add(cbVerse);
            Controls.Add(listBoxReferences);
            Name = "ReferenceListSelection";
            Text = "ReferenceListSelection";
            Load += ReferenceListSelection_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private ListBox listBoxReferences;
        private ComboBox cbVerse;
        private ComboBox cbChapter;
        private ComboBox cbBook;
        private Label label1;
        private Label label2;
        private Label label3;
        private Label label4;
        private Button buttonOK;
        private Button buttonCancel;
        private Button buttonAdd;
        private Label labelError;
        private Button buttonDeleteSelection;
    }
}