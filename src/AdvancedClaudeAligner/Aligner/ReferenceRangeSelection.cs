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
    public partial class ReferenceRangeSelection : Form
    {
        VersionParser versionParser;

        public ReferenceRangeSelection()
        {
            InitializeComponent();
        }

        private void ReferenceRangeSelection_Load(object sender, EventArgs e)
        {
            labelFirstError.Visible = false;
            labelLastError.Visible = false;
        }

        public void Initialise(VersionParser versionParser)
        {
            this.versionParser = versionParser;

            // populate the combo boxes with the bookCounts from the version parser
            List<string> bookNames = new List<string>();

            foreach (var bookCount in versionParser.bookCounts)
            {
                bookNames.Add(bookCount.Key);
            }
            cbFirstBook.Items.Clear();
            cbFirstBook.Items.AddRange(bookNames.ToArray());
            cbFirstBook.Text = bookNames[0];

            cbLastBook.Items.Clear();
            cbLastBook.Items.AddRange(bookNames.ToArray());
            cbLastBook.Text = bookNames[0];
        }

        private void cbFirstBook_SelectedIndexChanged(object sender, EventArgs e)
        {
            string bookName = cbFirstBook.SelectedItem.ToString();
            BookDetails bookDetails = versionParser.bookCounts[bookName];
            List<string> chapterNumbers = new List<string>();

            int chapterCount = bookDetails.ChapterVerses.Count;
            for (int i = 1; i <= chapterCount; i++)
            {
                chapterNumbers.Add(i.ToString());
            }
            cbFirstChapter.Items.Clear();
            cbFirstChapter.Items.AddRange(chapterNumbers.ToArray());
            cbFirstChapter.Text = "1";
            labelFirstError.Visible = false;
        }

        private void cbLastBook_SelectedIndexChanged(object sender, EventArgs e)
        {
            string bookName = cbLastBook.SelectedItem.ToString();
            BookDetails bookDetails = versionParser.bookCounts[bookName];
            List<string> chapterNumbers = new List<string>();

            int chapterCount = bookDetails.ChapterVerses.Count;
            for (int i = 1; i <= chapterCount; i++)
            {
                chapterNumbers.Add(i.ToString());
            }
            cbLastChapter.Items.Clear();
            cbLastChapter.Items.AddRange(chapterNumbers.ToArray());
            cbLastChapter.Text = "1";
            labelLastError.Visible = false;
        }

        private void cbFirstChapter_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbFirstBook.SelectedIndex == -1)
            {
                labelFirstError.Text = "Invalid Book";
                labelFirstError.Visible = true;
                return;
            }
            labelFirstError.Visible = false;
            BookDetails bookDetails = versionParser.bookCounts[cbFirstBook.SelectedItem.ToString()];
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

        private void cbLastChapter_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbLastBook.SelectedIndex == -1)
            {
                labelLastError.Text = "Invalid Book";
                labelLastError.Visible = true;
                return;
            }
            labelLastError.Visible = false;
            BookDetails bookDetails = versionParser.bookCounts[cbLastBook.SelectedItem.ToString()];
            List<string> verseNumbers = new List<string>();

            int selectedChapter = 1;
            int.TryParse(cbLastChapter.SelectedItem.ToString(), out selectedChapter);

            int verseCount = bookDetails.ChapterVerses[selectedChapter - 1];
            for (int i = 1; i <= verseCount; i++)
            {
                verseNumbers.Add(i.ToString());
            }
            cbLastVerse.Items.Clear();
            cbLastVerse.Items.AddRange(verseNumbers.ToArray());
            cbLastVerse.Text = "1";

        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            labelFirstError.Visible = false;
            labelLastError.Visible = false;

            // Vlidate the First Reference
            if (cbFirstBook.SelectedIndex == -1)
            {
                labelFirstError.Text = "Invalid Book";
                labelFirstError.Visible = true;
                return;
            }
            if (cbFirstChapter.SelectedIndex == -1)
            {
                labelFirstError.Text = "Invalid Chapter";
                labelFirstError.Visible = true;
                return;
            }
            if (cbFirstVerse.SelectedIndex == -1)
            {
                labelFirstError.Text = "Invalid Verse";
                labelFirstError.Visible = true;
                return;
            }

            // Validate the Last Reference
            if (cbLastBook.SelectedIndex == -1)
            {
                labelLastError.Text = "Invalid Book";
                labelLastError.Visible = true;
                return;
            }
            if (cbLastChapter.SelectedIndex == -1)
            {
                labelLastError.Text = "Invalid Chapter";
                labelLastError.Visible = true;
                return;
            }
            if (cbLastVerse.SelectedIndex == -1)
            {
                labelLastError.Text = "Invalid Verse";
                labelLastError.Visible = true;
                return;
            }

            // Validate that the first reference is before the last reference
            // that is, the index of the selected firstBook, firstChapter and firstVerse
            // are less than the index of the selected lastBook, lastChapter and lastVerse
                int firstBookIndex = cbFirstBook.SelectedIndex;
                int firstChapterIndex = cbFirstChapter.SelectedIndex;
                int firstVerseIndex = cbFirstVerse.SelectedIndex;
    
                int lastBookIndex = cbLastBook.SelectedIndex;
                int lastChapterIndex = cbLastChapter.SelectedIndex;
                int lastVerseIndex = cbLastVerse.SelectedIndex;
            if(firstBookIndex > lastBookIndex ||
                (firstBookIndex == lastBookIndex && firstChapterIndex > lastChapterIndex) ||
                (firstBookIndex == lastBookIndex && firstChapterIndex == lastChapterIndex && firstVerseIndex > lastVerseIndex))
            {
                labelLastError.Text = "Last reference must be after first reference";
                labelLastError.Visible = true;
                return;
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        public string FirstReference
        {
            get
            {
                return $"{cbFirstBook.SelectedItem}.{cbFirstChapter.SelectedItem}.{cbFirstVerse.SelectedItem}";
            }
        }
        public string LastReference
        {
            get
            {
                return $"{cbLastBook.SelectedItem}.{cbLastChapter.SelectedItem}.{cbLastVerse.SelectedItem}";
            }
        }
    }
}

