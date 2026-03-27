using AdvancedAligner;
using System.Runtime.CompilerServices;
using System.Text;

namespace HebrewBibleMorphology
{
    public partial class MainForm : Form
    {
        /// <summary>
        /// key: bookName
        /// value: morph count dictionary
        ///        key: morph
        ///        value: count
        /// </summary>
        private Dictionary<string, Dictionary<string, int>> morphologyCounts = new();
        private Dictionary<string, Dictionary<string, int>> lemmaCounts = new();

        HebrewBibleParser hebrewBibleParser;

        StringBuilder sb = new();
        public MainForm(HebrewBibleParser hebrewBibleParser)
        {
            this.hebrewBibleParser = hebrewBibleParser;
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // for each book in Hebrew bible bookCounts
            foreach (var counts in hebrewBibleParser.bookCounts.Values)
            {
                // 1. get the index of the the first and last verse
                string book = counts.BookName;
                int lastChapter = counts.ChapterVerses.Count;
                int lastVerse = counts.ChapterVerses[lastChapter - 1];
                string firstReference = $"{book}.1.1";
                string lastReference = $"{book}.{lastChapter}.{lastVerse}";
                int firstIndex = hebrewBibleParser.referenceIndices[firstReference];
                int lastIndex = hebrewBibleParser.referenceIndices[lastReference];

                // 2. build morphology and lemma counts
                Dictionary<string, int> morphCounts = new();
                Dictionary<string, int> lemmas = new();
                for (int i = firstIndex; i <= lastIndex; i++)
                {
                    var verse = hebrewBibleParser.HebrewBible[i];
                    foreach (var word in verse.RawTokens)
                    {
                        string morph = word.morphology; //.Substring(2);
                        string lemma = word.lemma;
                        //if (new string[] { "R", "c", "C"}.Contains(morph))
                        //    continue; 
                        if (morphCounts.ContainsKey(morph))
                            morphCounts[morph]++;
                        else
                            morphCounts[morph] = 1;

                        if (lemmas.ContainsKey(lemma))
                            lemmas[lemma]++;
                        else
                            lemmas[lemma] = 1;
                    }
                }
                // sort morph counts decending
                morphCounts = morphCounts
                          .OrderByDescending(kvp => kvp.Value)
                          //.Take(10)
                          .ToDictionary();

                lemmas = lemmas
                          .OrderByDescending(kvp => kvp.Value)
                          //.Take(10)
                          .ToDictionary();
                // 3. Add the counts to the morphologyCounts
                morphologyCounts[book] = morphCounts;
                lemmaCounts[book] = lemmas;

                Dictionary<string, int> verseScores = new();
                for (int i = firstIndex; i <= lastIndex; i++)
                {
                    var verse = hebrewBibleParser.HebrewBible[i];
                    verseScores[verse.Reference] = GetScore(verse);
                }
                var topVerses = verseScores
                        .OrderByDescending(kvp => kvp.Value)
                        .Take(5)
                        .ToDictionary();
                Dictionary<string, int> top5Verses = new();
                foreach (var v in topVerses)
                {
                    top5Verses[v.Key] = hebrewBibleParser.referenceIndices[v.Key];
                }
                top5Verses = top5Verses
                        .OrderBy(kvp => kvp.Value)
                        .ToDictionary();

                foreach (string r in top5Verses.Keys)
                    sb.Append($"\"{r}\",");
                sb.AppendLine();
            }

            File.WriteAllText("Top5.txt", sb.ToString());
        }

        private int GetScore(OtVerse verse)
        {
            int score = 0;
            string book = verse.Reference.Split('.')[0];
            foreach(var word in verse.RawTokens)
            {
                foreach(var morph in morphologyCounts[book])
                {
                    if(morph.Key == word.morphology)
                    {
                        score += morph.Value;
                        break;
                    }
                }
                foreach (var lemma in lemmaCounts[book])
                {
                    if (lemma.Key == word.lemma)
                    {
                        score += lemma.Value;
                        break;
                    }
                }
            }
            return score;
        }
    }

}
