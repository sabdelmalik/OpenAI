using AdvancedAligner;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace AdvancedAligner
{
    public partial class ReferenceListSelection : Form
    {
        TargetParser targetParser;

        public ReferenceListSelection(TargetParser targetParser)
        {
            this.targetParser = targetParser;
            InitializeComponent();
        }

        private void ReferenceListSelection_Load(object sender, EventArgs e)
        {
            labelError.Visible = false;
            Initialise();
        }

        private void Initialise()
        {

            // populate the combo boxes with the bookCounts from the version parser
            List<string> bookNames = new List<string>();

            foreach (var bookCount in targetParser.bookCounts)
            {
                bookNames.Add(bookCount.Key);
            }
            cbBook.Items.Clear();
            cbBook.Items.AddRange(bookNames.ToArray());
            cbBook.Text = bookNames[0];

            string refList = Properties.OpenAiSettings.Default.ReferenceList;
            if (!string.IsNullOrEmpty(refList))
            {
                // convert comma seperated refs to a string array
                string[] referenceList = refList.Split(new char[] { ',' } );
                listBoxReferences.Items.Clear();
                listBoxReferences.Items.AddRange(referenceList);    
            }
        }

        private void cbBook_SelectedIndexChanged(object sender, EventArgs e)
        {
            string bookName = cbBook.SelectedItem.ToString();
            BookDetails bookDetails = targetParser.bookCounts[bookName];
            List<string> chapterNumbers = new List<string>();

            int chapterCount = bookDetails.ChapterVerses.Count;
            for (int i = 1; i <= chapterCount; i++)
            {
                chapterNumbers.Add(i.ToString());
            }
            cbChapter.Items.Clear();
            cbChapter.Items.AddRange(chapterNumbers.ToArray());
            cbChapter.Text = "1";
            labelError.Visible = false;
        }

        private void cbChapter_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbBook.SelectedIndex == -1)
            {
                labelError.Text = "Invalid Book";
                labelError.Visible = true;
                return;
            }
            labelError.Visible = false;
            BookDetails bookDetails = targetParser.bookCounts[cbBook.SelectedItem.ToString()];
            List<string> verseNumbers = new List<string>();

            int selectedChapter = 1;
            int.TryParse(cbChapter.SelectedItem.ToString(), out selectedChapter);

            int verseCount = bookDetails.ChapterVerses[selectedChapter - 1];
            for (int i = 1; i <= verseCount; i++)
            {
                verseNumbers.Add(i.ToString());
            }
            cbVerse.Items.Clear();
            cbVerse.Items.AddRange(verseNumbers.ToArray());
            cbVerse.Text = "1";

        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            if (cbBook.SelectedIndex == -1)
            {
                labelError.Text = "Invalid Book";
                labelError.Visible = true;
                return;
            }
            if (cbChapter.SelectedIndex == -1)
            {
                labelError.Text = "Invalid Chapter";
                labelError.Visible = true;
                return;
            }
            if (cbVerse.SelectedIndex == -1)
            {
                labelError.Text = "Invalid Verse";
                labelError.Visible = true;
                return;
            }
            labelError.Visible = false;

            string reference = $"{cbBook.SelectedItem}.{cbChapter.SelectedItem}.{cbVerse.SelectedItem}";
            // If the reference is already in the list box, do not add it again
            if (listBoxReferences.Items.Contains(reference))
            {
                labelError.Text = "Reference already in list";
                labelError.Visible = true;
                return;
            }
            listBoxReferences.Items.Add(reference);
        }


        /// <summary>
        /// returns a list of the references in the list box in the format "Book.Chapter.Verse", e.g. "Genesis.1.1"
        /// </summary>
        /// <returns></returns>
        public List<string> ReferencesList
        {
            get
            {
                List<string> references = new List<string>();
                foreach (var item in listBoxReferences.Items)
                {
                    references.Add(item.ToString());
                }
                return references;
            }
        }

        private void buttonDeleteSelection_Click(object sender, EventArgs e)
        {
            // Iterate backwards through the selected indices
            for (int i = listBoxReferences.SelectedIndices.Count - 1; i >= 0; i--)
            {
                // Get the index of the selected item in the original Items collection
                int indexToRemove = listBoxReferences.SelectedIndices[i];

                // Remove the item at that index
                listBoxReferences.Items.RemoveAt(indexToRemove);
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            if(ReferencesList.Count > 0)
            {
                // convert the list to comma seperated strings
                string listStrings = string.Join(",", ReferencesList);
                Properties.OpenAiSettings.Default.ReferenceList = listStrings;
                Properties.OpenAiSettings.Default.Save();
            }
        }
    }
}
