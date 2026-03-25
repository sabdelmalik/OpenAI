namespace AlignmentUI
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
            hebrewList = new ListBox();
            targetList = new ListBox();
            alignmentList = new ListBox();
            addButton = new Button();
            saveButton = new Button();
            SuspendLayout();
            // 
            // hebrewList
            // 
            hebrewList.FormattingEnabled = true;
            hebrewList.Location = new Point(10, 10);
            hebrewList.Name = "hebrewList";
            hebrewList.SelectionMode = SelectionMode.MultiExtended;
            hebrewList.Size = new Size(400, 229);
            hebrewList.TabIndex = 0;
            // 
            // targetList
            // 
            targetList.FormattingEnabled = true;
            targetList.Location = new Point(10, 300);
            targetList.Name = "targetList";
            targetList.SelectionMode = SelectionMode.MultiExtended;
            targetList.Size = new Size(840, 129);
            targetList.TabIndex = 2;
            // 
            // alignmentList
            // 
            alignmentList.FormattingEnabled = true;
            alignmentList.Location = new Point(450, 10);
            alignmentList.Name = "alignmentList";
            alignmentList.Size = new Size(400, 229);
            alignmentList.TabIndex = 1;
            // 
            // addButton
            // 
            addButton.Location = new Point(10, 470);
            addButton.Name = "addButton";
            addButton.Size = new Size(112, 34);
            addButton.TabIndex = 3;
            addButton.Text = "Add Alignment";
            addButton.UseVisualStyleBackColor = true;
            addButton.Click += AddAlignment_Click;
            // 
            // saveButton
            // 
            saveButton.Location = new Point(150, 470);
            saveButton.Name = "saveButton";
            saveButton.Size = new Size(112, 34);
            saveButton.TabIndex = 4;
            saveButton.Text = "Save";
            saveButton.UseVisualStyleBackColor = true;
            saveButton.Click += Save_Click;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(900, 600);
            Controls.Add(saveButton);
            Controls.Add(targetList);
            Controls.Add(alignmentList);
            Controls.Add(addButton);
            Controls.Add(hebrewList);
            Name = "MainForm";
            Text = "Alignment Editor (WinForms)";
            Load += MainForm_Load;
            ResumeLayout(false);
        }

        #endregion

        private ListBox hebrewList;
        private ListBox alignmentList;
        private ListBox targetList;
        private Button addButton;
        private Button saveButton;
    }
}
