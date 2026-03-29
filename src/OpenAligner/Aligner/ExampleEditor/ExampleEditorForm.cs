using AdvancedAligner.Examples;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace AdvancedAligner.ExampleEditor
{
    public partial class ExampleEditorForm : Form
    {
        private ExamplesDatabase examplesDatabase;
        private HebrewBibleParser hebrewBibleParser;
        private TargetParser targetParser;

        public ExampleEditorForm(ExamplesDatabase examplesDatabase, HebrewBibleParser hebrewBibleParser, TargetParser targetParser)
        {
            this.examplesDatabase = examplesDatabase;
            this.hebrewBibleParser = hebrewBibleParser;
            this.targetParser = targetParser;

            InitializeComponent();
            hebrewGridView1.HebrewBibleParser = this.hebrewBibleParser;
        }

        private void ExampleEditorForm_Load(object sender, EventArgs e)
        {
            targetGridView1.HebrewBibleParser = this.hebrewBibleParser;
            targetGridView1.TargetParser = this.targetParser;
            targetGridView1.ExamplesDatabase = this.examplesDatabase;

            hebrewGridView1.Update("Job.23.2");
            targetGridView1.Update("Job.23.2");
        }
    }
}
