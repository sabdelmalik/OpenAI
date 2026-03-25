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

        public MainForm(HebrewBibleParser hebrewBibleParser,
                        TahotParser tahotParser,
                        OshbParser oshbParser,
                         TargetParser targetParser,
                         AlignmentService alignmentService)
        {
            InitializeComponent();
            this.hebrewBibleParser = hebrewBibleParser;
            this.tahotParser = tahotParser;
            this.targetParser = targetParser;
            this.oshbParser = oshbParser;
            this.alignmentService = alignmentService;
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
            TimeSpan time = new TimeSpan(0, 0, 0, 0); ;
            bool success = false;

            string folderName = "Alignments";
            if (!Directory.Exists(folderName))
                Directory.CreateDirectory(folderName);
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd hh-mm-ss");
            int fileNameIndex = 0;

            foreach (PromptResult verseAlignments in results)
            {
                fileNameIndex++;
                if (!string.IsNullOrEmpty(verseAlignments.prompt))
                {
                    string promptFileName = $"{folderName}\\Prompt-{outName}({fileNameIndex})-{timestamp}.txt";
                    File.WriteAllText(promptFileName, verseAlignments.prompt);
                }
                if (!string.IsNullOrEmpty(verseAlignments.result))
                {
                    string resultFileName = $"{folderName}\\Result-{outName}({fileNameIndex})-{timestamp}.txt";
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
                        TargetVerse targetVerse = targetParser.TargetBible[index];
                        OtVerse hebrewVerse = hebrewBibleParser.HebrewBible[index];

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
            if (referenceListSelection == null)
            {
                referenceListSelection = new ReferenceListSelection();

                referenceListSelection.Initialise(targetParser);
                //alignmentService = new UserPrompt(hebrewBibleParser, tahotParser, targetParser);
                // Psa.10.9, Psa.17.14, Psa.18.6, Pro.4.22, Pro.6.27
                if (referenceListSelection.ShowDialog() == DialogResult.OK)
                {
                    List<string> list = referenceListSelection.ReferencesList;
                    //string json = alignmentService.BuildPromptOpenAI(list);
                    string outName = $"{list[0]}-{list[list.Count - 1]}";
                    if(list.Count==1)
                        outName = list[0];

                    NewAlign(list, outName);
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
                TargetVerse targetVerse = targetParser.TargetBible[index];
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
            if (referenceRangeSelection == null)
            {
                referenceRangeSelection = new ReferenceRangeSelection();
                //if (targetParser == null || targetParser.VersionBible.Count == 0)
                //{
                //    bool versionLoaded = OpenVersion();
                //    if (!versionLoaded)
                //        return;
                //}


                referenceRangeSelection.Initialise(targetParser);
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

                referenceRangeSelection.Dispose();
                referenceRangeSelection = null;

            }
        }

        private async Task NewAlign(List<string>  references, string outName)
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
            ParseJsonResult(result, outName);

            // InterlinearBuilder.Print(result, verse);

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
    }
}
