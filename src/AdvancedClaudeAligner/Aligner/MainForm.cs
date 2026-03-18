using System.Security.Cryptography.Xml;
using System.Text;
using System.Text.Json;

namespace AdvancedAligner
{
    public partial class MainForm : Form
    {
        HebrewBibleParser hebrewBibleParser;
        VersionParser versionParser;
        OshbParser oshbParser;

        ReferenceListSelection referenceListSelection;
        ReferenceRangeSelection referenceRangeSelection;

        public MainForm()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            hebrewBibleParser = new HebrewBibleParser();
            oshbParser = new OshbParser();
        }
        private void openTheVersionToAlignToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenVersion();
        }

        private bool OpenVersion()
        {
            bool versionLoaded = false;
            openFileDialog1.Filter = "text files (*.txt)|*.txt|All files (*.*)|*.*";
            DialogResult result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                string file = openFileDialog1.FileName;
                versionParser = new VersionParser(file, hebrewBibleParser.HebrewBooks);
                versionLoaded = true;
            }
            return versionLoaded;
        }

        private void listToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (referenceListSelection == null)
            {
                referenceListSelection = new ReferenceListSelection();
                if (versionParser == null || versionParser.VersionBible.Count == 0)
                {
                    bool versionLoaded = OpenVersion();
                    if (!versionLoaded)
                        return;
                }


                referenceListSelection.Initialise(versionParser);
                UserPrompt userPrompt = new UserPrompt(hebrewBibleParser, versionParser);
                // Psa.10.9, Psa.17.14, Psa.18.6, Pro.4.22, Pro.6.27
                if (referenceListSelection.ShowDialog() == DialogResult.OK)
                {
                    userPrompt.BuildPrompt(referenceListSelection.ReferencesList, false);
                }

                referenceListSelection.Dispose();
                referenceListSelection = null;
            }
        }


        private void parseResultToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*";
            DialogResult result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                string file = openFileDialog1.FileName;
                ParseJsonfile(file);
            }
        }

        /// <summary>
        /// The JSON file is expected to contain the alignment results, 
        /// including the aligned tokens and any notes or comments about the alignment. 
        /// The method will read the JSON file, parse its contents,
        /// The JSON file is expected to contain An array of VerseAlignment objects, where each VerseAlignment object contains the verse reference,
        /// and a list of Alignment objects representing the alignments for that verse. 
        /// Each Alignment object contains an array of hebrew indices, an array of english indeces and a notes string.
        /// <param name="jsonFile">JSON File</param>
        private void ParseJsonfile(string jsonFile)
        {
            if (versionParser == null || versionParser.VersionBible.Count == 0)
            {
                bool versionLoaded = OpenVersion();
                if (!versionLoaded)
                    return;
            }

            string jsonContent = File.ReadAllText(jsonFile);
            List<VerseAignments> verseAlignments = JsonSerializer.Deserialize<List<VerseAignments>>(jsonContent);

            StringBuilder sb = new StringBuilder();
            // Process the verseAlignments as needed
            foreach (var verseAlignment in verseAlignments)
            {
                int index = hebrewBibleParser.referenceIndices[verseAlignment.reference];
                VersionVerse versionVerse = versionParser.VersionBible[index];
                OtVerse hebrewVerse = hebrewBibleParser.HebrewBible[index];

                sb.AppendLine($"Verse: {verseAlignment.reference}");
                foreach (var alignment in verseAlignment.alignments)
                {
                    string versionWord = string.Empty;
                    string strongs = string.Empty;
                    string hebrewWord = string.Empty;
                    string notes = string.Empty;
                    if (alignment.english_indices != null && alignment.english_indices.Length > 0)
                    {
                        foreach (int englishIndex in alignment.english_indices)
                        {
                            if (englishIndex >= 0 && englishIndex < versionVerse.Tokens.Count)
                            {
                                versionWord += versionVerse.Tokens[englishIndex].surface + " ";
                            }
                        }
                    }
                    versionWord = versionWord.Trim();
                    if (alignment.hebrew_indices != null && alignment.hebrew_indices.Length > 0)
                    {
                        foreach (int hebrewIndex in alignment.hebrew_indices)
                        {
                            if (hebrewIndex >= 0 && hebrewIndex < hebrewVerse.Tokens.Count)
                            {
                                hebrewWord += hebrewVerse.Tokens[hebrewIndex].surface + " ";
                                strongs += hebrewVerse.Tokens[hebrewIndex].strong + " ";
                            }
                        }
                    }
                    hebrewWord = hebrewWord.Trim();
                    notes = alignment.notes;

                    sb.AppendLine($"{versionWord}\t{strongs}\t{hebrewWord}\t{notes}");
                }

            }
            File.WriteAllText("alignment_results_haiku-No-Thinking-Ps23.txt", sb.ToString());
        }

        private void specialSearchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // find all verses where the pos is verb and the morphology contains "hophal" and "imperfect" 
            StringBuilder sb = new StringBuilder();
            foreach (var verse in hebrewBibleParser.HebrewBible)
            {
                foreach (var token in verse.Value.Tokens)
                {
                    if (token.pos == "verb" && token.morph.Contains("hophal") && token.morph.Contains("imperfect"))
                    {
                        sb.AppendLine($"{verse.Value.Reference}: {token.surface} - {token.morph} - {verse.Value.VerseText}");
                    }
                }
            }
            File.WriteAllText("hophal_imperfect_verbs.txt", sb.ToString());
        }

        private void rangeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (referenceRangeSelection == null)
            {
                referenceRangeSelection = new ReferenceRangeSelection();
                if (versionParser == null || versionParser.VersionBible.Count == 0)
                {
                    bool versionLoaded = OpenVersion();
                    if (!versionLoaded)
                        return;
                }


                referenceRangeSelection.Initialise(versionParser);
                UserPrompt userPrompt = new UserPrompt(hebrewBibleParser, versionParser);
                // Psa.10.9, Psa.17.14, Psa.18.6, Pro.4.22, Pro.6.27
                if (referenceRangeSelection.ShowDialog() == DialogResult.OK)
                {
                    userPrompt.BuildPrompt(referenceRangeSelection.FirstReference, referenceRangeSelection.LastReference);
                }

                referenceRangeSelection.Dispose();
                referenceRangeSelection = null;

            }
        }
    }
}
