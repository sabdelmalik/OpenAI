using AdvancedAligner.Examples;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.DirectoryServices.ActiveDirectory;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;

namespace AdvancedAligner.ExampleEditor
{
    public partial class ExampleEditorForm : Form
    {
        private ExamplesDatabase examplesDatabase;
        private HebrewBibleParser hebrewBibleParser;
        private TargetParser targetParser;

        List<string> bookNames = new List<string>();
        int lastSelectedBookOffset = 0;
        bool loadInProgress = true;

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

            targetGridView1.RefernceHighlightRequest += DgvTarget_RefernceHighlightRequest;


            Initialise();

            examplesDatabase.Load();

            // When the user closes the form (click the x control)
            // we want to check with the user if they realy want to close the form
            this.FormClosing += ExampleEditorForm_FormClosing; 

        }

        private void ExampleEditorForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            if (examplesDatabase.Dirty)
            {
                if( MessageBox.Show("Save before closing?", "Editor Closing", MessageBoxButtons.YesNo, MessageBoxIcon.Question ) == DialogResult.Yes)
                {
                    examplesDatabase.Save();
                }
            }
        }

        private void Initialise()
        {
            comboBoxReferences.Items.Clear();
            List<string> references = targetGridView1.Examples.Keys.ToList();
            // let us sort the references in the Bible order
            SortedDictionary<int, string> sortedRefs = new();
            foreach (string reference in references)
            {
                int index = hebrewBibleParser.referenceIndices[reference];
                sortedRefs.Add(index, reference);
            }

            comboBoxReferences.Items.AddRange(sortedRefs.Values.ToArray());

            this.targetParser = targetParser;

            // populate the combo boxes with the bookCounts from the version parser
            bookNames = new List<string>();

            foreach (var bookCount in targetParser.bookCounts)
            {
                bookNames.Add(bookCount.Key);
            }
            cbFirstBook.Items.Clear();
            cbFirstBook.Items.AddRange(bookNames.ToArray());
            cbFirstBook.SelectedItem = bookNames[0];

            cbFirstBook.SelectedItem = bookNames[0];
        }

        private void DgvTarget_RefernceHighlightRequest(object sender, string tag, bool firstHalf)
        {
            new Thread(() =>
            {
                try
                {
                    SelectReferenceTags(tag, firstHalf);
                }
                catch (Exception ex)
                {
                    var cm = System.Reflection.MethodBase.GetCurrentMethod();
                    var name = cm.DeclaringType.FullName + "." + cm.Name;
                    //Tracing.TraceException(name, ex.Message);
                    //container.HandleException(ex);
                }
            }).Start();

            //new Thread(() =>
            //{
            //    try
            //    {
            //        SelectTargetTags(tag, firstHalf);
            //    }
            //    catch (Exception ex)
            //    {
            //        var cm = System.Reflection.MethodBase.GetCurrentMethod();
            //        var name = cm.DeclaringType.FullName + "." + cm.Name;
            //        //Tracing.TraceException(name, ex.Message);
            //        //container.HandleException(ex);
            //    }
            //}).Start();
        }

        private void SelectTargetTags(string tag, bool firstHalf)
        {
            if (InvokeRequired)
            {
                Action safeWrite = delegate { SelectTargetTags(tag, firstHalf); };
                Invoke(safeWrite);
            }
            else
            {
                try
                {
                    if (targetGridView1.SelectedCells.Count == 1)
                    {
                        if (string.IsNullOrEmpty(tag.ToString()))
                        {
                            SetHighlightedCell(targetGridView1, new List<int>(), firstHalf);
                            return;
                        }

                        List<int> columns2Highligt = new List<int>();
                        string[] parts = tag.Split(',');
                        foreach (string part in parts)
                        {
                            columns2Highligt.Add(int.Parse(part));
                        }

                        SetHighlightedCell(targetGridView1, columns2Highligt, firstHalf);
                    }
                }
                catch (Exception ex)
                {
                    var cm = System.Reflection.MethodBase.GetCurrentMethod();
                    var name = cm.DeclaringType.FullName + "." + cm.Name;
                    //Tracing.TraceException(name, ex.Message);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tag">comma seperated indices</param>
        /// <param name="firstHalf"></param>
        private void SelectReferenceTags(string tag, bool firstHalf)
        {
            if (InvokeRequired)
            {
                Action safeWrite = delegate { SelectReferenceTags(tag, firstHalf); };
                Invoke(safeWrite);
            }
            else
            {
                try
                {
                    hebrewGridView1.ClearSelection();

                    if (string.IsNullOrEmpty(tag))
                    {
                        SetHighlightedCell(hebrewGridView1, new List<int>(), firstHalf);
                        return;
                    }

                    List<int> columns2Highligt = new List<int>();
                    string[] parts = tag.Split(',');
                    foreach (string part in parts)
                    {
                        columns2Highligt.Add(int.Parse(part) + 1);
                    }

                    SetHighlightedCell(hebrewGridView1, columns2Highligt, firstHalf);

                }
                catch (Exception ex)
                {
                    var cm = System.Reflection.MethodBase.GetCurrentMethod();
                    var name = cm.DeclaringType.FullName + "." + cm.Name;
                    //Tracing.TraceException(name, ex.Message);
                }
            }
        }

        private void SetHighlightedCell(DataGridView dgv, List<int> columns2Highligt, bool firstHalf)
        {
            int count = dgv.ColumnCount;
            int tagsRow = dgv.RowCount - 1;

            // remove all previous highlightind
            for (int i = 0; i < dgv.Columns.Count; i++)
            {
                dgv.Rows[tagsRow].Cells[i].Style.BackColor = Color.Empty;
            }


            int selectedCellColumn = -1;
            if (dgv == targetGridView1)
                selectedCellColumn = targetGridView1.SelectedCells[0].ColumnIndex;

            if (columns2Highligt.Count == 1)
            {
                if (dgv != targetGridView1)
                {
                    dgv.CurrentCell = dgv.Rows[tagsRow].Cells[columns2Highligt[0]];
                }
                dgv.Rows[tagsRow].Cells[columns2Highligt[0]].Style.BackColor = Color.Gold;
            }
            else if (columns2Highligt.Count != 0)
            {
                for (int i = 0; i < columns2Highligt.Count; i++)
                {
                    dgv.Rows[tagsRow].Cells[columns2Highligt[i]].Style.BackColor = Color.Gold;
                }
            }
            if (dgv != targetGridView1)
                dgv.ClearSelection();
        }

        private void comboBoxReferences_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(comboBoxReferences.Text))
            {
                hebrewGridView1.Update(comboBoxReferences.Text);
                targetGridView1.Update(comboBoxReferences.Text);
            }
        }

        private void cbFirstBook_SelectedIndexChanged(object sender, EventArgs e)
        {
            string bookName = cbFirstBook.SelectedItem.ToString();
            BookDetails bookDetails = targetParser.bookCounts[bookName];
            List<string> chapterNumbers = new List<string>();

            int chapterCount = bookDetails.ChapterVerses.Count;
            for (int i = 1; i <= chapterCount; i++)
            {
                chapterNumbers.Add(i.ToString());
            }
            cbFirstChapter.Items.Clear();
            cbFirstChapter.Items.AddRange(chapterNumbers.ToArray());
            cbFirstChapter.Text = "1";
        }

        private void cbFirstChapter_SelectedIndexChanged(object sender, EventArgs e)
        {
            BookDetails bookDetails = targetParser.bookCounts[cbFirstBook.SelectedItem.ToString()];
            List<string> verseNumbers = new List<string>();

            int selectedChapter = 1;
            int.TryParse(cbFirstChapter.SelectedItem.ToString(), out selectedChapter);

            int verseCount = bookDetails.ChapterVerses[selectedChapter - 1];
            for (int i = 1; i <= verseCount; i++)
            {
                verseNumbers.Add(i.ToString());
            }


            cbFirstVerse.Items.Clear();
            cbFirstVerse.Items.AddRange(verseNumbers.ToArray());
            cbFirstVerse.Text = "1";
        }

        private void saveDBToolStripMenuItem_Click(object sender, EventArgs e)
        {
            examplesDatabase.Save();
        }

        private void buttonDelete_Click(object sender, EventArgs e)
        {
            string reference = comboBoxReferences.Text;
            int selectedIndex = comboBoxReferences.SelectedIndex;
            var itemToRemove = examplesDatabase.Delete(reference);

            // If an item was found, remove it from the list
            if (itemToRemove > 0)
            {
                //examplesDatabase.Examples.Remove(itemToRemove);
                targetGridView1.Remove(reference);
                targetGridView1.Clear();
                hebrewGridView1.Clear();

                // rebuild comboBoxReferences items
                comboBoxReferences.Items.Clear();
                List<string> references = targetGridView1.Examples.Keys.ToList();
                // let us sort the references in the Bible order
                SortedDictionary<int, string> sortedRefs = new();
                foreach (string r in references)
                {
                    int index = hebrewBibleParser.referenceIndices[r];
                    sortedRefs.Add(index, r);
                }

                comboBoxReferences.Items.AddRange(sortedRefs.Values.ToArray());
                //comboBoxReferences.Text = string.Empty;
                // restore selected index, but ensure it does not exeed the comboBoxReferences.Items
                comboBoxReferences.SelectedIndex = Math.Min(selectedIndex, comboBoxReferences.Items.Count -1);
            }
        }
    }
}
