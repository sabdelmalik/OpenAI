using OpenAiAPI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Reflection.Emit;
using System.Text;
using System.Windows.Forms;

namespace AdvancedAligner
{
    public partial class SettingsForm : Form
    {
        public SettingsForm()
        {
            InitializeComponent();
        }

        private void SettingsForm_Load(object sender, EventArgs e)
        {
            List<string> gptModels = Enum.GetNames(typeof(AiModel)).ToList();
            comboBoxGptModels.Items.Clear();
            comboBoxGptModels.Items.AddRange(gptModels.ToArray());

            string currentModel = Properties.OpenAiSettings.Default.AiModel;
            AiModel gptModel;
            bool success = Enum.TryParse(currentModel, out gptModel);
            if (success)
            {
                comboBoxGptModels.Text = gptModel.ToString();
            }
            else
            {
                comboBoxGptModels.Text = gptModels[0];
            }

            maxPromptVerses.Value = Properties.OpenAiSettings.Default.MaxPromptVerses;
            cbPromptFiles.Checked = Properties.OpenAiSettings.Default.OutputPromptFiles;
            cbResultFiles.Checked = Properties.OpenAiSettings.Default.OutputResultFiles;
            checkBoxRequestNotes.Checked = Properties.OpenAiSettings.Default.RequestNotes;
        }

        bool modelChanged = false;
        private void comboBoxGptModels_SelectedIndexChanged(object sender, EventArgs e)
        {
            modelChanged = true;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            if (modelChanged)
            {
                Properties.OpenAiSettings.Default.AiModel = comboBoxGptModels.Text;
                Properties.OpenAiSettings.Default.MaxPromptVerses = (int)maxPromptVerses.Value;
                Properties.OpenAiSettings.Default.OutputPromptFiles = cbPromptFiles.Checked;
                Properties.OpenAiSettings.Default.OutputResultFiles = cbResultFiles.Checked;
                Properties.OpenAiSettings.Default.RequestNotes = checkBoxRequestNotes.Checked;
                Properties.OpenAiSettings.Default.Save();
            }
        }
    }
}
