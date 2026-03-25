using AdvancedAligner;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace AdvancedAligner
{
    public partial class ReferenceRangeSelection : Form
    {
        TargetParser versionParser;
        int lastSelectedBookOffset = 0;

        bool loadInProgress = true;

        public ReferenceRangeSelection()
        {
            InitializeComponent();
        }

        private void ReferenceRangeSelection_Load(object sender, EventArgs e)
        {
            labelFirstError.Visible = false;
            labelLastError.Visible = false;
        }

        List<string> bookNames = new List<string>();

        public void Initialise(TargetParser versionParser)
        {
            this.versionParser = versionParser;

            // populate the combo boxes with the bookCounts from the version parser
            bookNames = new List<string>();

            foreach (var bookCount in versionParser.bookCounts)
            {
                bookNames.Add(bookCount.Key);
            }
            cbFirstBook.Items.Clear();
            cbFirstBook.Items.AddRange(bookNames.ToArray());
            cbFirstBook.SelectedItem = bookNames[0];

            cbLastBook.Items.Clear();
            cbLastBook.Items.AddRange(bookNames.ToArray());

            cbFirstBook.SelectedItem = bookNames[0];
            cbLastBook.SelectedItem = bookNames[0];
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

            if (cbLastBook.Items.Count > 0)
            {
                // This is not the first time
                List<string> lastBookNames = new List<string>();
                bool bookMatch = false;
                lastSelectedBookOffset = 0;
                foreach (string book in bookNames)
                {
                    if (bookMatch)
                        lastBookNames.Add(book);
                    else if (book == bookName)
                    {
                        bookMatch = true;
                        lastBookNames.Add(book);
                    }
                    else
                        lastSelectedBookOffset++;

                }
                string lastBbookName = cbLastBook.SelectedItem.ToString();

                cbLastBook.Items.Clear();
                cbLastBook.Items.AddRange(lastBookNames.ToArray());

                if (lastBookNames.Contains(lastBbookName))
                {
                    // The book did not change, do not trigger the book changed event
                    cbLastBook.SelectedIndexChanged -= cbLastBook_SelectedIndexChanged;
                    cbLastBook.SelectedItem = lastBbookName;
                    cbLastBook.SelectedIndexChanged += cbLastBook_SelectedIndexChanged;
                }
                else
                {
                    cbLastBook.SelectedItem = bookName;
                }
            }

            labelFirstError.Visible = false;
            labelLastError.Visible = false;
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

            if(loadInProgress)
            {
                loadInProgress = false;
                string previousFirstReference = Properties.OpenAiSettings.Default.FirstReference;
                string previousLastReference = Properties.OpenAiSettings.Default.LastReference;
                string[] firstParts = previousFirstReference.Split(new char[] { '.' } );
                string[] lastParts = previousLastReference.Split(new char[] {'.'});

                if (firstParts.Length == 3)
                {
                    cbFirstBook.SelectedItem = firstParts[0];
                    cbFirstChapter.SelectedItem = firstParts[1];
                    cbFirstVerse.SelectedItem = firstParts[2];
                }

                if (lastParts.Length == 3)
                {
                    cbLastBook.SelectedItem = lastParts[0];
                    cbLastChapter.SelectedItem = lastParts[1];
                    cbLastVerse.SelectedItem = lastParts[2];
                }
            }
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
    
                int lastBookIndex = cbLastBook.SelectedIndex + lastSelectedBookOffset;
                int lastChapterIndex = cbLastChapter.SelectedIndex;
                int lastVerseIndex = cbLastVerse.SelectedIndex;
            if(firstBookIndex > lastBookIndex ||
                (firstBookIndex == lastBookIndex && firstChapterIndex > lastChapterIndex) ||
                (firstBookIndex == lastBookIndex && firstChapterIndex == lastChapterIndex && firstVerseIndex > lastVerseIndex))
            {
                labelLastError.Text = "Last reference must be the same or after the first";
                labelLastError.Visible = true;
                return;
            }

            Properties.OpenAiSettings.Default.FirstReference = $"{cbFirstBook.SelectedItem}.{cbFirstChapter.SelectedItem}.{cbFirstVerse.SelectedItem}";
            Properties.OpenAiSettings.Default.LastReference = $"{cbLastBook.SelectedItem}.{cbLastChapter.SelectedItem}.{cbLastVerse.SelectedItem}";
            Properties.OpenAiSettings.Default.Save();

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

