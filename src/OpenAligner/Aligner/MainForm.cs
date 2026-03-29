using AdvancedAligner.ExampleEditor;
using AdvancedAligner.Examples;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenAiAPI;
using OpenAiAPI.InterlinearExport;
using OpenAiAPI.Models;
using System.Reflection;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Text.Json;
using TAParser;

namespace AdvancedAligner
{
    public partial class MainForm : Form
    {
        private string execFolder = string.Empty;

        HebrewBibleParser hebrewBibleParser;
        OshbParser oshbParser;
        TahotParser tahotParser;
        TargetParser targetParser;
        AlignmentService alignmentService;

        ReferenceListSelection referenceListSelection;
        ReferenceRangeSelection referenceRangeSelection;
        SettingsForm settingsForm;
        ExamplesDatabase examplesDatabase;
        ExampleEditorForm exampleEditorForm;

        public MainForm(HebrewBibleParser hebrewBibleParser,
                        TahotParser tahotParser,
                        OshbParser oshbParser,
                        TargetParser targetParser,
                        AlignmentService alignmentService,
                        ExamplesDatabase examplesDatabase,
                        ReferenceListSelection referenceListSelection,
                        ReferenceRangeSelection referenceRangeSelection,
                        ExampleEditorForm exampleEditorForm)
        {
            InitializeComponent();
            this.hebrewBibleParser = hebrewBibleParser;
            this.tahotParser = tahotParser;
            this.targetParser = targetParser;
            this.oshbParser = oshbParser;
            this.alignmentService = alignmentService;
            this.examplesDatabase = examplesDatabase;
            this.referenceListSelection = referenceListSelection;
            this.referenceRangeSelection = referenceRangeSelection;
            this.exampleEditorForm = exampleEditorForm;
        }

        #region MyTrace
        delegate void MyTraceDelegate(string text, Color color);
        delegate void ClearMyTraceDelegate();
        delegate void SetMyTraceRTLDelegate(bool rtl);

        public void clearMyTrace()
        {
            if (InvokeRequired)
            {
                Invoke(new ClearMyTraceDelegate(clearMyTrace));
            }
            else
            {
                traceBox.Clear();
                traceBox.ScrollToCaret();
            }
        }

        public void SetMyTraceRTL(bool rtl)
        {
            if (InvokeRequired)
            {
                Invoke(new SetMyTraceRTLDelegate(SetMyTraceRTL), new object[] { rtl });
            }
            else
            {
                traceBox.RightToLeft = rtl ? RightToLeft.Yes : RightToLeft.No;
            }
        }

        public void MyTrace(string text, Color color)
        {
            if (InvokeRequired)
            {
                Invoke(new MyTraceDelegate(MyTrace), new object[] { text, color });
            }
            else
            {
                traceBox.SelectionColor = color;
                if (text.Length > 0)
                {
                    string txt = string.Format("{0}: {1}v", DateTime.Now.ToString("dd-MMM-yyyy hh:mm:ss.fff"), text);
                    //traceBox.AppendText(txt);
                    traceBox.SelectedText = text + "\r\n"; // txt;
                }
                else
                {
                    traceBox.AppendText("\r\n");
                }
                traceBox.ScrollToCaret();
            }
        }

        public void MyTraceError(string method, string text)
        {
            MyTrace(string.Format("Error: {0}::{1}", method, text), Color.Red);
        }


        #endregion MyTrace

        private void Form1_Load(object sender, EventArgs e)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            AssemblyName assemblyName = assembly.GetName();
            Version version = assemblyName.Version;
            this.Text = "OpenAI Aligner " + version.ToString();

            var cm = System.Reflection.MethodBase.GetCurrentMethod();
            var name = cm.DeclaringType.FullName + "." + cm.Name;

            execFolder = Path.GetDirectoryName(assembly.Location);
        }

        private void ParseJsonResult(List<PromptResult> results, string outName)
        {

            StringBuilder versesSB = new StringBuilder();
            StringBuilder headerSB = new StringBuilder();
            string model = string.Empty;
            string error = string.Empty;
            int totalVerses = 0;
            int inputTokens = 0;
            int outputTokens = 0;
            double cost = 0;
            TimeSpan time = new TimeSpan(0, 0, 0, 0);
            bool success = false;

            string folderName = "Alignments";
            if (!Directory.Exists(folderName))
                Directory.CreateDirectory(folderName);
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd hh-mm-ss");
            int fileNameIndex = 0;

            foreach (PromptResult verseAlignments in results)
            {
                if (verseAlignments is null)
                    return;
                fileNameIndex++;
                if (Properties.OpenAiSettings.Default.OutputPromptFiles && !string.IsNullOrEmpty(verseAlignments.prompt))
                {
                    string promptFileName = $"{folderName}\\Prompt-{outName}({fileNameIndex})-{timestamp}.txt";
                    File.WriteAllText(promptFileName, verseAlignments.prompt);
                }
                if (Properties.OpenAiSettings.Default.OutputResultFiles && !string.IsNullOrEmpty(verseAlignments.result))
                {
                    string resultFileName = $"{folderName}\\Result-{outName}({fileNameIndex})-{timestamp}.json";
                    File.WriteAllText(resultFileName, verseAlignments.result);
                }

                success = verseAlignments.success;
                if (!success)
                {
                    error = $"calll to gpt failed\t{verseAlignments.errorString}";
                    break;
                }
                else
                {
                    if (string.IsNullOrEmpty(model))
                        model = Properties.OpenAiSettings.Default.GptModel;
                    inputTokens += verseAlignments.inputTokens;
                    outputTokens += verseAlignments.outputTokens;
                    cost += verseAlignments.cost;
                    time += TimeSpan.Parse(verseAlignments.time);
                    totalVerses += verseAlignments.ParsedResult.Count;

                    // Process the verseAlignments as needed
                    foreach (var verseAlignment in verseAlignments.ParsedResult)
                    {
                        int index = hebrewBibleParser.referenceIndices[verseAlignment.reference];
                        ParserTargetVerse targetVerse = targetParser.TargetBible[index];
                        ParserHebrewVerse hebrewVerse = hebrewBibleParser.HebrewBible[index];

                        versesSB.AppendLine($"Verse: {verseAlignment.reference}");
                        foreach (var alignment in verseAlignment.alignments)
                        {
                            string targetWord = string.Empty;
                            string strongs = string.Empty;
                            string hebrewWord = string.Empty;
                            string notes = string.Empty;
                            if (alignment.t != null && alignment.t.Count > 0)
                            {
                                foreach (int englishIndex in alignment.t)
                                {
                                    if (englishIndex >= 0 && englishIndex < targetVerse.Tokens.Count)
                                    {
                                        targetWord += targetVerse.Tokens[englishIndex].surface + " ";
                                    }
                                }
                            }
                            targetWord = targetWord.Trim();
                            if (alignment.h != null && alignment.h.Count > 0)
                            {
                                foreach (int hebrewIndex in alignment.h)
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

                            versesSB.AppendLine($"{targetWord}\t{strongs}\t{hebrewWord}\t{notes}");
                        }

                    }
                }
            }

            // Create the header
            headerSB.AppendLine($"Total verses:\t{totalVerses}");
            headerSB.AppendLine($"Total Time:\t{time.ToString()}");
            headerSB.AppendLine($"Total cost:\t${cost}");
            headerSB.AppendLine($"gpt model:\t{model}");
            headerSB.AppendLine($"Total input tokens:\t{inputTokens}");
            headerSB.AppendLine($"Total output tokens:\t{outputTokens}");
            if (!success)
            {
                headerSB.AppendLine("Errors encountered");
                headerSB.AppendLine("Error:\t{error}");
            }
            headerSB.AppendLine("===================================");
            // Append the verse
            headerSB.Append(versesSB.ToString());

            // Appened a timestamp to the outName to form the output file name
            string outFileName = $"{folderName}\\{outName}-{timestamp}.txt";
            File.WriteAllText($@"{outFileName}", headerSB.ToString());
        }

        private void listToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //referenceListSelection.Initialise(targetParser);
            //alignmentService = new UserPrompt(hebrewBibleParser, tahotParser, targetParser);
            // Psa.10.9, Psa.17.14, Psa.18.6, Pro.4.22, Pro.6.27
            if (referenceListSelection.ShowDialog() == DialogResult.OK)
            {
                List<string> list = referenceListSelection.ReferencesList;
                //string json = alignmentService.BuildPromptOpenAI(list);
                string outName = $"{list[0]}-{list[list.Count - 1]}";
                if (list.Count == 1)
                    outName = list[0];

                NewAlign(list, outName);
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
            //if (targetParser == null || targetParser.TargetBible.Count == 0)
            //{
            //    bool versionLoaded = OpenVersion();
            //    if (!versionLoaded)
            //        return;
            //}

            string jsonContent = File.ReadAllText(jsonFile);
            List<VerseAignments> verseAlignments = JsonSerializer.Deserialize<List<VerseAignments>>(jsonContent);

            StringBuilder sb = new StringBuilder();
            // Process the verseAlignments as needed
            foreach (var verseAlignment in verseAlignments)
            {
                int index = hebrewBibleParser.referenceIndices[verseAlignment.reference];
                ParserTargetVerse targetVerse = targetParser.TargetBible[index];
                ParserHebrewVerse hebrewVerse = hebrewBibleParser.HebrewBible[index];

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
                            if (englishIndex >= 0 && englishIndex < targetVerse.Tokens.Count)
                            {
                                versionWord += targetVerse.Tokens[englishIndex].surface + " ";
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
            //referenceRangeSelection.Initialise(targetParser);
            //alignmentService = new UserPrompt(hebrewBibleParser, tahotParser, targetParser);
            // Psa.10.9, Psa.17.14, Psa.18.6, Pro.4.22, Pro.6.27
            if (referenceRangeSelection.ShowDialog() == DialogResult.OK)
            {
                //string json = alignmentService.BuildPromptOpenAI(referenceRangeSelection.FirstReference, referenceRangeSelection.LastReference);
                string outName = referenceRangeSelection.FirstReference;
                if (referenceRangeSelection.FirstReference != referenceRangeSelection.LastReference)
                    outName += $"-{referenceRangeSelection.LastReference}";

                NewAlign(referenceRangeSelection.FirstReference, referenceRangeSelection.LastReference, outName);
            }

        }

        private async Task NewAlign(List<string> references, string outName)
        {
            GptModel model = GptModel.gpt_4_1_mini;
            string currentModel = Properties.OpenAiSettings.Default.GptModel;
            bool success = Enum.TryParse(currentModel, out model);
            if (!success)
            {
                // default to gpt_4_1_mini
                model = GptModel.gpt_4_1_mini;
            }

            int maxPromptVerses = Properties.OpenAiSettings.Default.MaxPromptVerses;
            List<PromptResult> result = await alignmentService.Align(references, maxPromptVerses, model);

            AddToExamplesDB(result);

            ParseJsonResult(result, outName);

            // InterlinearBuilder.Print(result, verse);

        }

        private void AddToExamplesDB(List<PromptResult> result)
        {
            foreach (var resultItem in result)
            {
                if (resultItem is null)
                    return;
                // Each resultItem is a PromptResult
                // The ParsedResult property of PromptResult is a List of AlignmentResult
                // the properties of AlignmentResult are
                //      Reference
                //      a List of Alignment pairs
                // get the prompt verses

                string inputMarker = "Input:";
                int versesIndex = resultItem.prompt.LastIndexOf(inputMarker);
                if (versesIndex == -1)
                    return;
                string promptVerses = resultItem.prompt.Substring(versesIndex + inputMarker.Length);

                var verses = JsonSerializer.Deserialize<List<CombinedVerse>>(promptVerses);

                foreach (var verse in verses)
                {
                    // get the Alignment result for the verse
                    var thisVersealignments = resultItem.ParsedResult[0];
                    foreach (var alignmentResult in resultItem.ParsedResult)
                    {
                        if (alignmentResult.reference == verse.reference)
                        {
                            thisVersealignments = alignmentResult;
                        }
                    }
                    examplesDatabase.AddExample(new ExampleAlignment
                    {
                        Reference = verse.reference,
                        HebrewLemmas = ExtractLemmasFromVerse(verse.hebrew.tokens),
                        POS = ExtractPOSFromVerse(verse.hebrew.tokens),
                        MorphPatterns = ExtractMorphFromVerse(verse.hebrew.tokens),
                        Alignment = thisVersealignments
                    });
                }
            }
            examplesDatabase.Save();
        }
        private List<string> ExtractLemmasFromVerse(List<HebrewToken> tokens)
        {
            var lemmas = new List<string>();

            foreach (var token in tokens)
            {
                lemmas.Add(token.lemma);
            }

            return lemmas;
        }
        private List<string> ExtractPOSFromVerse(List<HebrewToken> tokens)
        {
            var pos = new List<string>();

            foreach (var token in tokens)
            {
                pos.Add(token.pos);
            }

            return pos;
        }
        private List<string> ExtractMorphFromVerse(List<HebrewToken> tokens)
        {
            var morph = new List<string>();

            foreach (var token in tokens)
            {
                morph.Add(token.morph);
            }

            return morph;
        }

        private async Task NewAlign(string firstRef, string lastRef, string outName)
        {
            GptModel model = GptModel.gpt_4_1_mini;
            string currentModel = Properties.OpenAiSettings.Default.GptModel;
            bool success = Enum.TryParse(currentModel, out model);
            if (!success)
            {
                // default to gpt_4_1_mini
                model = GptModel.gpt_4_1_mini;
            }

            int maxPromptVerses = Properties.OpenAiSettings.Default.MaxPromptVerses;
            List<PromptResult> result = await alignmentService.Align(firstRef, lastRef, maxPromptVerses, model);

            AddToExamplesDB(result);

            ParseJsonResult(result, outName);

            // InterlinearBuilder.Print(result, verse);

        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (settingsForm == null)
            {
                settingsForm = new SettingsForm();

                //settingsForm.Initialise(targetParser);

                DialogResult result = settingsForm.ShowDialog();

                settingsForm.Dispose();
                settingsForm = null;

            }
        }

        private void top5ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<string> list = new List<string>
            {
                "Gen.1.26","Gen.1.28","Gen.24.7","Gen.24.30","Gen.36.6",
                "Exo.3.8","Exo.13.5","Exo.29.20","Exo.29.21","Exo.29.22",
                "Lev.8.25","Lev.8.30","Lev.14.6","Lev.14.51","Lev.16.15",
                "Num.4.15","Num.4.16","Num.8.19","Num.20.8","Num.32.33",
                "Deu.4.34","Deu.5.14","Deu.12.18","Deu.13.5","Deu.16.11",
                "Jos.7.24","Jos.8.33","Jos.9.1","Jos.17.11","Jos.23.13",
                "Jdg.1.27","Jdg.7.13","Jdg.10.6","Jdg.16.3","Jdg.18.17",
                "Rut.2.11","Rut.2.14","Rut.4.7","Rut.4.10","Rut.4.11",
                "1Sa.10.1","1Sa.13.15","1Sa.14.34","1Sa.15.9","1Sa.23.26",
                "2Sa.2.23","2Sa.3.8","2Sa.11.11","2Sa.14.19","2Sa.18.9",
                "1Ki.1.25","1Ki.2.22","1Ki.3.6","1Ki.8.64","1Ki.15.18",
                "2Ki.6.32","2Ki.16.15","2Ki.20.13","2Ki.23.3","2Ki.23.4",
                "1Ch.7.2","1Ch.15.18","1Ch.16.5","1Ch.24.6","1Ch.28.1",
                "2Ch.5.13","2Ch.23.13","2Ch.24.11","2Ch.31.1","2Ch.34.31",
                "Ezr.3.8","Ezr.8.16","Ezr.8.33","Ezr.9.1","Ezr.9.9",
                "Neh.7.73","Neh.8.4","Neh.9.32","Neh.10.39","Neh.13.5",
                "Est.3.12","Est.4.11","Est.6.9","Est.7.8","Est.8.9",
                "Job.1.19","Job.2.3","Job.2.11","Job.42.8","Job.42.11",
                "Psa.18.1","Psa.18.6","Psa.27.4","Psa.54.1","Psa.63.1",
                "Pro.1.27","Pro.27.10","Pro.27.27","Pro.30.8","Pro.30.19",
                "Ecc.3.19","Ecc.6.2","Ecc.8.17","Ecc.9.2","Ecc.9.11",
                "Sng.2.3","Sng.2.14","Sng.3.4","Sng.5.1","Sng.5.2",
                "Isa.9.7","Isa.11.11","Isa.39.2","Isa.59.21","Isa.66.20",
                "Jer.21.7","Jer.25.9","Jer.38.4","Jer.40.4","Jer.52.25",
                "Lam.1.2","Lam.1.7","Lam.1.22","Lam.2.11","Lam.2.19",
                "Ezk.38.20","Ezk.43.11","Ezk.45.7","Ezk.45.17","Ezk.48.21",
                "Dan.4.23","Dan.4.25","Dan.5.7","Dan.5.21","Dan.5.23",
                "Hos.1.7","Hos.2.15","Hos.2.18","Hos.2.19","Hos.5.13",
                "Jol.2.2","Jol.2.17","Jol.2.19","Jol.2.20","Jol.3.16",
                "Amo.3.9","Amo.4.7","Amo.6.10","Amo.8.10","Amo.9.1",
                "Oba.1.7","Oba.1.11","Oba.1.12","Oba.1.13","Oba.1.18",
                "Jon.1.3","Jon.1.5","Jon.3.7","Jon.4.2","Jon.4.8",
                "Mic.3.11","Mic.4.2","Mic.4.3","Mic.5.7","Mic.7.12",
                "Nam.2.10","Nam.2.13","Nam.3.3","Nam.3.5","Nam.3.17",
                "Hab.1.8","Hab.1.15","Hab.2.5","Hab.2.6","Hab.3.16",
                "Zep.1.3","Zep.2.9","Zep.3.8","Zep.3.19","Zep.3.20",
                "Hag.1.1","Hag.1.11","Hag.1.12","Hag.2.12","Hag.2.19",
                "Zec.8.10","Zec.8.19","Zec.12.10","Zec.14.4","Zec.14.21",
                "Mal.1.6","Mal.1.13","Mal.2.2","Mal.3.5","Mal.3.10"
            };

            string outName = $"Top5Verses";
            NewAlign(list, outName);

        }

        private void clearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Do you want to clear the examples data?", "Clear Examples Data", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                examplesDatabase.Clear();
            }
        }

        private void reloadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Do you want to reload the examples database?", "Reload Examples Database", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                examplesDatabase.Load();
            }
        }

        private void editToolStripMenuItem_Click(object sender, EventArgs e)
        {
            exampleEditorForm.ShowDialog();
        }
    }
}
