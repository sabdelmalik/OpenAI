namespace AdvancedAligner
{
    partial class SettingsForm
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
            label1 = new Label();
            comboBoxGptModels = new ComboBox();
            buttonOK = new Button();
            buttonCancel = new Button();
            label2 = new Label();
            maxPromptVerses = new NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)maxPromptVerses).BeginInit();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(31, 37);
            label1.Name = "label1";
            label1.Size = new Size(134, 25);
            label1.TabIndex = 0;
            label1.Text = "Open AI model";
            // 
            // comboBoxGptModels
            // 
            comboBoxGptModels.FormattingEnabled = true;
            comboBoxGptModels.Location = new Point(273, 34);
            comboBoxGptModels.Name = "comboBoxGptModels";
            comboBoxGptModels.Size = new Size(263, 33);
            comboBoxGptModels.TabIndex = 1;
            comboBoxGptModels.SelectedIndexChanged += comboBoxGptModels_SelectedIndexChanged;
            // 
            // buttonOK
            // 
            buttonOK.DialogResult = DialogResult.OK;
            buttonOK.Location = new Point(333, 200);
            buttonOK.Name = "buttonOK";
            buttonOK.Size = new Size(112, 34);
            buttonOK.TabIndex = 2;
            buttonOK.Text = "OK";
            buttonOK.UseVisualStyleBackColor = true;
            buttonOK.Click += buttonOK_Click;
            // 
            // buttonCancel
            // 
            buttonCancel.DialogResult = DialogResult.Cancel;
            buttonCancel.Location = new Point(20, 200);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new Size(112, 34);
            buttonCancel.TabIndex = 3;
            buttonCancel.Text = "Cancel";
            buttonCancel.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(31, 105);
            label2.Name = "label2";
            label2.Size = new Size(194, 25);
            label2.TabIndex = 0;
            label2.Text = "Max Verses Per Prompt";
            // 
            // maxPromptVerses
            // 
            maxPromptVerses.Location = new Point(273, 103);
            maxPromptVerses.Maximum = new decimal(new int[] { 20, 0, 0, 0 });
            maxPromptVerses.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            maxPromptVerses.Name = "maxPromptVerses";
            maxPromptVerses.Size = new Size(180, 31);
            maxPromptVerses.TabIndex = 4;
            maxPromptVerses.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // SettingsForm
            // 
            AcceptButton = buttonOK;
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = buttonCancel;
            ClientSize = new Size(800, 450);
            Controls.Add(maxPromptVerses);
            Controls.Add(buttonCancel);
            Controls.Add(buttonOK);
            Controls.Add(comboBoxGptModels);
            Controls.Add(label2);
            Controls.Add(label1);
            Name = "SettingsForm";
            Text = "SettingsForm";
            Load += SettingsForm_Load;
            ((System.ComponentModel.ISupportInitialize)maxPromptVerses).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private ComboBox comboBoxGptModels;
        private Button buttonOK;
        private Button buttonCancel;
        private Label label2;
        private NumericUpDown maxPromptVerses;
    }
}