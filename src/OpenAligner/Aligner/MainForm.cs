using AdvancedAligner.ExampleEditor;
using AdvancedAligner.Examples;
using AdvancedAligner.Recovery;
using Google.GenAI;
using Google.GenAI.Types;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenAiAPI;
using OpenAiAPI.InterlinearExport;
using OpenAiAPI.Models;
using System.Reflection;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using TAParser;
using static Google.Apis.Requests.BatchRequest;

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
        private readonly ILogger<MainForm> logger;

        public MainForm(ILogger<MainForm> logger,
                        HebrewBibleParser hebrewBibleParser,
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
            this.logger = logger;
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

            //test();
        }

        private List<TaggedVerse>  ParseJsonResult(List<PromptResult> results, string outName)
        {
            logger.LogInformation($"Entring ParseJsonResult with {results.Count} verses");

            List<TaggedVerse> taggedVerses = new List<TaggedVerse>();

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
                    return taggedVerses;
                fileNameIndex++;
                if (Properties.OpenAiSettings.Default.OutputPromptFiles && !string.IsNullOrEmpty(verseAlignments.prompt))
                {
                    string promptFileName = $"{folderName}\\Prompt-{outName}({fileNameIndex})-{timestamp}.txt";
                    System.IO.File.WriteAllText(promptFileName, verseAlignments.prompt);
                }
                if (Properties.OpenAiSettings.Default.OutputResultFiles && !string.IsNullOrEmpty(verseAlignments.result))
                {
                    string resultFileName = $"{folderName}\\Result-{outName}({fileNameIndex})-{timestamp}.json";
                    System.IO.File.WriteAllText(resultFileName, verseAlignments.result);
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
                        model = Properties.OpenAiSettings.Default.AiModel;
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
                        TaggedVerse taggedVerse = new TaggedVerse
                        {
                            VerseReference = verseAlignment.reference,
                            ReferenceIndex = index,
                            TaggedWords = new List<TaggedWord>()
                        };
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
                                        string word = targetVerse.Tokens[englishIndex].surface;
                                        targetWord += word + " ";
                                    }
                                }
                            }
                            targetWord = targetWord.Trim();
                            TaggedWord taggedtWord = new TaggedWord
                            {
                                Text = targetWord,
                                Strongs = new List<string>(),
                                Morphology = new List<string>()
                            };

                            if (alignment.h != null && alignment.h.Count > 0)
                            {
                                foreach (int hebrewIndex in alignment.h)
                                {
                                    if (hebrewIndex >= 0 && hebrewIndex < hebrewVerse.Tokens.Count)
                                    {   
                                        string strong = hebrewVerse.Tokens[hebrewIndex].strong;
                                        string morph = hebrewVerse.RawTokens[hebrewIndex].morphology;
                                        if (morph.StartsWith("H:") || morph.StartsWith("A:"))
                                        {
                                            // remove the H: or A: prefix from the morphology
                                            morph = morph.Substring(2);
                                        }
                                        taggedtWord.Strongs.Add(strong);
                                        taggedtWord.Morphology.Add(morph);
                                        
                                        hebrewWord += hebrewVerse.Tokens[hebrewIndex].surface + " ";
                                        strongs += strong + " ";
                                    }
                                }
                            }
                            hebrewWord = hebrewWord.Trim();
                            notes = alignment.notes;

                            versesSB.AppendLine($"{targetWord}\t{strongs}\t{hebrewWord}\t{notes}");
                            taggedVerse.TaggedWords.Add(taggedtWord);
                        }
                        taggedVerses.Add(taggedVerse);
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
            System.IO.File.WriteAllText($@"{outFileName}", headerSB.ToString());
        
            return taggedVerses;
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
            logger.LogInformation($"Entering ParseJsonfile with file: {jsonFile}");
            //if (targetParser == null || targetParser.TargetBible.Count == 0)
            //{
            //    bool versionLoaded = OpenVersion();
            //    if (!versionLoaded)
            //        return;
            //}

            string jsonContent = System.IO.File.ReadAllText(jsonFile);
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
            System.IO.File.WriteAllText("alignment_results_haiku-No-Thinking-Ps23.txt", sb.ToString());
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
            System.IO.File.WriteAllText("hophal_imperfect_verbs.txt", sb.ToString());
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
            logger.LogInformation($"Entering NewAlign with {references.Count} verses and outName: {outName}");

            AiModel model = AiModel.gpt_4__1_mini;
            string currentModel = Properties.OpenAiSettings.Default.AiModel;
            bool success = Enum.TryParse(currentModel, out model);
            if (!success)
            {
                // default to gpt_4_1_mini
                model = AiModel.gpt_4__1_mini;
            }

            bool requestNotes = Properties.OpenAiSettings.Default.RequestNotes;
            int maxPromptVerses = Properties.OpenAiSettings.Default.MaxPromptVerses;
            List<PromptResult> result = await alignmentService.Align(references, maxPromptVerses, model, requestNotes);


            AddToExamplesDB(result);

            ParseJsonResult(result, outName);

            // InterlinearBuilder.Print(result, verse);

        }


        private void AddToExamplesDB(List<PromptResult> result)
        {
            logger.LogInformation($"Entering AddToExamplesDB with {result.Count} verses");

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


                List<CombinedVerse> verses = GetVersesFromPrompt(resultItem.prompt);
                if (verses is null)
                    return;

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

                    try
                    {
                        examplesDatabase.AddExample(new ExampleAlignment
                        {
                            Reference = verse.reference,
                            HebrewLemmas = ExtractLemmasFromVerse(verse.hebrew.tokens),
                            POS = ExtractPOSFromVerse(verse.hebrew.tokens),
                            MorphPatterns = ExtractMorphFromVerse(verse.hebrew.tokens),
                            Alignment = thisVersealignments
                        });
                    }
                    catch (Exception ex)
                    {
                        MyTraceError("AddToExamplesDB", $"Error adding example for verse {verse.reference}: {ex.Message}");
                        return;
                    }
                    examplesDatabase.Save();
                }
                examplesDatabase.Save();
            }
        }

        private List<CombinedVerse>? GetVersesFromPrompt(string prompt)
        {
            logger.LogInformation($"Entering GetVersesFromPrompt with prompt length: {prompt.Length}");

            List<CombinedVerse> verses = null;

            string inputMarker = "Input:";
            int versesIndex = prompt.LastIndexOf(inputMarker);
            if (versesIndex == -1)
                return verses;

            string promptVerses = string.Empty;
            try
            {
                promptVerses = prompt.Substring(versesIndex + inputMarker.Length);
                promptVerses = promptVerses.Replace(@"\r\n", "").Replace("\\\"", "\"");
            }
            catch (Exception ex)
            {
                MyTraceError("AddToExamplesDB", $"Error extracting prompt verses: {ex.Message}");
                return verses;
            }

            try
            {
                verses = JsonSerializer.Deserialize<List<CombinedVerse>>(promptVerses);
            }
            catch (Exception ex)
            {
                MyTraceError("AddToExamplesDB", $"Error deserializing prompt verses: {ex.Message}");
                return verses;
            }

            return verses;
        }

        private List<string> ExtractLemmasFromVerse(List<HebrewToken> tokens)
        {
            logger.LogInformation($"Entering ExtractLemmasFromVerse with {tokens.Count} tokens");

            var lemmas = new List<string>();

            foreach (var token in tokens)
            {
                lemmas.Add(token.lemma);
            }

            return lemmas;
        }
        private List<string> ExtractPOSFromVerse(List<HebrewToken> tokens)
        {
            logger.LogInformation($"Entering ExtractPOSFromVerse with {tokens.Count} tokens");

            var pos = new List<string>();

            foreach (var token in tokens)
            {
                pos.Add(token.pos);
            }

            return pos;
        }
        private List<string> ExtractMorphFromVerse(List<HebrewToken> tokens)
        {
            logger.LogInformation($"Entering ExtractMorphFromVerse with {tokens.Count} tokens");

            var morph = new List<string>();

            foreach (var token in tokens)
            {
                morph.Add(token.morph);
            }

            return morph;
        }

        private async Task NewAlign(string firstRef, string lastRef, string outName)
        {
            logger.LogInformation($"Entering NewAlign with firstRef: {firstRef}, lastRef: {lastRef}, outName: {outName}");

            AiModel model = AiModel.gpt_4__1_mini;
            string currentModel = Properties.OpenAiSettings.Default.AiModel;
            bool success = Enum.TryParse(currentModel, out model);
            if (!success)
            {
                // default to gpt_4_1_mini
                model = AiModel.gpt_4__1_mini;
            }

            bool requestNotes = Properties.OpenAiSettings.Default.RequestNotes;
            int maxPromptVerses = Properties.OpenAiSettings.Default.MaxPromptVerses;
            List<PromptResult> result = await alignmentService.Align(firstRef, lastRef, maxPromptVerses, model, requestNotes);

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
//                "Gen.1.26","Gen.1.28","Gen.24.7","Gen.24.30","Gen.36.6",
                "Exo.3.8","Exo.13.5","Exo.29.20","Exo.29.21","Exo.29.22",
                //"Lev.8.25","Lev.8.30","Lev.14.6","Lev.14.51","Lev.16.15",
                //"Num.4.15","Num.4.16","Num.8.19","Num.20.8","Num.32.33",
                //"Deu.4.34","Deu.5.14","Deu.12.18","Deu.13.5","Deu.16.11",
                //"Jos.7.24","Jos.8.33","Jos.9.1","Jos.17.11","Jos.23.13",
                //"Jdg.1.27","Jdg.7.13","Jdg.10.6","Jdg.16.3","Jdg.18.17",
                //"Rut.2.11","Rut.2.14","Rut.4.7","Rut.4.10","Rut.4.11",
                //"1Sa.10.1","1Sa.13.15","1Sa.14.34","1Sa.15.9","1Sa.23.26",
                //"2Sa.2.23","2Sa.3.8","2Sa.11.11","2Sa.14.19","2Sa.18.9",
                //"1Ki.1.25","1Ki.2.22","1Ki.3.6","1Ki.8.64","1Ki.15.18",
                //"2Ki.6.32","2Ki.16.15","2Ki.20.13","2Ki.23.3","2Ki.23.4",
                //"1Ch.7.2","1Ch.15.18","1Ch.16.5","1Ch.24.6","1Ch.28.1",
                //"2Ch.5.13","2Ch.23.13","2Ch.24.11","2Ch.31.1","2Ch.34.31",
                //"Ezr.3.8","Ezr.8.16","Ezr.8.33","Ezr.9.1","Ezr.9.9",
                //"Neh.7.73","Neh.8.4","Neh.9.32","Neh.10.39","Neh.13.5",
                //"Est.3.12","Est.4.11","Est.6.9","Est.7.8","Est.8.9",
                ////"Job.1.19","Job.2.3","Job.2.11","Job.42.8","Job.42.11",
                //"Psa.18.1","Psa.18.6","Psa.27.4","Psa.54.1","Psa.63.1",
                //"Pro.1.27","Pro.27.10","Pro.27.27","Pro.30.8","Pro.30.19",
                //"Ecc.3.19","Ecc.6.2","Ecc.8.17","Ecc.9.2","Ecc.9.11",
                //"Sng.2.3","Sng.2.14","Sng.3.4","Sng.5.1","Sng.5.2",
                //"Isa.9.7","Isa.11.11","Isa.39.2","Isa.59.21","Isa.66.20",
                //"Jer.21.7","Jer.25.9","Jer.38.4","Jer.40.4","Jer.52.25",
                //"Lam.1.2","Lam.1.7","Lam.1.22","Lam.2.11","Lam.2.19",
                //"Ezk.38.20","Ezk.43.11","Ezk.45.7","Ezk.45.17","Ezk.48.21",
                //"Dan.4.23","Dan.4.25","Dan.5.7","Dan.5.21","Dan.5.23",
                //"Hos.1.7","Hos.2.15","Hos.2.18","Hos.2.19","Hos.5.13",
                //"Jol.2.2","Jol.2.17","Jol.2.19","Jol.2.20","Jol.3.16",
                //"Amo.3.9","Amo.4.7","Amo.6.10","Amo.8.10","Amo.9.1",
                //"Oba.1.7","Oba.1.11","Oba.1.12","Oba.1.13","Oba.1.18",
                //"Jon.1.3","Jon.1.5","Jon.3.7","Jon.4.2","Jon.4.8",
                //"Mic.3.11","Mic.4.2","Mic.4.3","Mic.5.7","Mic.7.12",
                //"Nam.2.10","Nam.2.13","Nam.3.3","Nam.3.5","Nam.3.17",
                //"Hab.1.8","Hab.1.15","Hab.2.5","Hab.2.6","Hab.3.16",
                //"Zep.1.3","Zep.2.9","Zep.3.8","Zep.3.19","Zep.3.20",
                //"Hag.1.1","Hag.1.11","Hag.1.12","Hag.2.12","Hag.2.19",
                //"Zec.8.10","Zec.8.19","Zec.12.10","Zec.14.4","Zec.14.21",
                //"Mal.1.6","Mal.1.13","Mal.2.2","Mal.3.5","Mal.3.10"
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

        private void importExampleResultsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Do you want to import json responses into the examples database?", "Import Examples Database", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                return;

            string folderPath = @"ExampleResults";
            if (!Directory.Exists(folderPath))
                return;

            string[] responseFiles = Directory.GetFiles(folderPath);
            // Each JSON file contains a list of AlignmentResult objects
            foreach (string responseFile in responseFiles)
            {
                string json = System.IO.File.ReadAllText(responseFile);
                List<AlignmentResult> alignmentResults = JsonSerializer.Deserialize<List<AlignmentResult>>(json);
                foreach (var result in alignmentResults)
                {
                    int index = hebrewBibleParser.referenceIndices[result.reference];
                    var verse = hebrewBibleParser.HebrewBible[index];
                    List<HebrewToken> hebrewTokens = new();
                    foreach (var token in verse.Tokens)
                    {
                        hebrewTokens.Add(new HebrewToken
                        {
                            i = token.index,
                            surface = token.surface,
                            pos = token.pos,
                            lemma = token.lemma,
                            morph = token.morph
                        });
                    }
                    examplesDatabase.AddExample(new ExampleAlignment
                    {
                        Reference = result.reference,
                        HebrewLemmas = ExtractLemmasFromVerse(hebrewTokens),
                        POS = ExtractPOSFromVerse(hebrewTokens),
                        MorphPatterns = ExtractMorphFromVerse(hebrewTokens),
                        Alignment = result
                    });
                }

            }
            examplesDatabase.Save();
        }

        private void test()
        {
            string prompt = @"
You are aligning multiple Biblical Hebrew verses to their English target translations given as Input.\r\nThe input Hebrew verse is given as a complete text, followed by the same text tokenised. Each token has an index indicating its position in the verse. Each token may represent word, an affix, a conjunction, … Each token includes morphological information to help in the alignment\r\nThe input target verse is given as a complete text, followed by the same text tokenised. Each token has an index indicating its position in the verse.\r\n\r\n\r\n\r\nTask:\r\nFor EACH verse:\r\nAlign Hebrew tokens to target tokens. Pay attention to the Hebrew token details: lemma, pos, morphology and gloss.\r\n\r\nRules:\r\n- Align by meaning\r\n- Use token indices\r\n- A Hebrew token index MUST NOT be used more than once in the alignment\r\n- A Target token index MUST NOT be used more than once in the alignment\r\n- Do not combine articles and conjunctions with other words if they have a Hebrew token to align to\r\n- Combine target tokens when it makes morphological sense\r\n- if a target pronoun DOES NOT HAVE a corresponding Hebrew token, but is implied in the Hebrew verb, combine it with the target verb\r\n- Include ALL target words\r\n- Treat Hebrew words connected with a maqqef as separate words (ignore the maqqef)\r\n- The Hebrew אֵת should not be mapped\r\n- Try to always have Hebrew prepositions morph=inseparable_prep, appear in the output mapping\r\n  For example if לָ֭מָּה is represented by tokens 0=לָ֭ and 1=מָּה a corresponding English token \""why\"" would have its hebrew indices as [0,1] \r\n\r\nIn the output JSON \r\n- tokens are identified by their indices\r\n- All target indices must appear in the map\r\n- Target indices should appear in the same order as in the input\r\n- If a target token does not have a Hebrew equivalent, its Hebrew indices will be empty []\r\n- Ignore a Hebrew token if it does not correspond to any of the target tokens.\r\nFalse\r\n\r\nThe output returned should be JSON in this format:\r\n[\r\n{\r\n\""reference\"": \""Genesis 1:1\"",\r\n\""alignments\"": [\r\n{\""t\"":[...],\""h\"":[..]}, \r\n…\r\n]\r\n},\r\n{\r\n\""reference\"": \""Genesis 1:2\"",\r\n\""alignments\"": [\r\n{\""t\"":[...],\""h\"":[..]}, \r\n…\r\n]\r\n},\r\n…\r\n]\r\nwhere \""\""t\""\"" = list of target token indices, \""\""h\""\"" = list of Hebrew token indices   \r\n\r\nReturn a raw JSON array with NO markdown formatting, NO code blocks, NO backticks, NO explanation.\r\nDo NOT wrap in ```json or ``` markers.\r\nYour response must be valid, parseable JSON that can be processed by JsonSerializer.Deserialize()\r\nStart directly with [ and end directly with ]\r\n\r\n\r\n\r\nInput:\r\n[{\""reference\"":\""Psa.3.1\"",\""hebrew\"":{\""text\"":\""מִזְמ֥וֹר לְדָוִ֑ד בְּ֝בָרְח֗וֹ מִפְּנֵ֤י׀ אַבְשָׁל֬וֹם בְּנֽוֹ יְ֭הוָה מָֽה־ רַבּ֣וּ צָרָ֑י רַ֝בִּ֗ים קָמִ֥ים עָלָֽי\"",\""tokens\"":[{\""i\"":0,\""surface\"":\""מִזְמ֥וֹר\"",\""lemma\"":\""מִזְמוֹר\"",\""pos\"":\""noun\"",\""morph\"":\""common-ms-abs\"",\""gloss\"":\""a psalm\""},{\""i\"":1,\""surface\"":\""לְ\"",\""lemma\"":\""ל\"",\""pos\"":\""preposition\"",\""morph\"":\""inseparable_prep\"",\""gloss\"":\""of\""},{\""i\"":2,\""surface\"":\""דָוִ֑ד\"",\""lemma\"":\""דָּוִד\"",\""pos\"":\""noun\"",\""morph\"":\""proper\"",\""gloss\"":\""David\""},{\""i\"":3,\""surface\"":\""בְּ֝\"",\""lemma\"":\""ב\"",\""pos\"":\""preposition\"",\""morph\"":\""inseparable_prep\"",\""gloss\"":\""when\""},{\""i\"":4,\""surface\"":\""בָרְח֗\"",\""lemma\"":\""בָּרַח\"",\""pos\"":\""verb\"",\""morph\"":\""qal-infinitive const\"",\""gloss\"":\""fled\""},{\""i\"":5,\""surface\"":\""וֹ\"",\""lemma\"":\""נּוּ\"",\""pos\"":\""suffix\"",\""morph\"":\""pronominal-3ms\"",\""gloss\"":\""he\""},{\""i\"":6,\""surface\"":\""מִ\"",\""lemma\"":\""מ\"",\""pos\"":\""preposition\"",\""morph\"":\""inseparable_prep\"",\""gloss\"":\""from\""},{\""i\"":7,\""surface\"":\""פְּנֵ֤י\"",\""lemma\"":\""פָּנֶה\"",\""pos\"":\""noun\"",\""morph\"":\""common-mp-const\"",\""gloss\"":\""before\""},{\""i\"":8,\""surface\"":\""אַבְשָׁל֬וֹם\"",\""lemma\"":\""אֲבִישָׁלוֹם\"",\""pos\"":\""noun\"",\""morph\"":\""proper\"",\""gloss\"":\""Absalom\""},{\""i\"":9,\""surface\"":\""בְּנֽ\"",\""lemma\"":\""בֵּן\"",\""pos\"":\""noun\"",\""morph\"":\""common-ms-const\"",\""gloss\"":\""son\""},{\""i\"":10,\""surface\"":\""וֹ\"",\""lemma\"":\""הוּ\"",\""pos\"":\""suffix\"",\""morph\"":\""pronominal-3ms\"",\""gloss\"":\""his\""},{\""i\"":11,\""surface\"":\""יְ֭הוָה\"",\""lemma\"":\""יהוה\"",\""pos\"":\""noun\"",\""morph\"":\""proper\"",\""gloss\"":\""¶O Yahweh\""},{\""i\"":12,\""surface\"":\""מָֽה\"",\""lemma\"":\""מָה\"",\""pos\"":\""pronoun\"",\""morph\"":\""interrogative\"",\""gloss\"":\""how!\""},{\""i\"":13,\""surface\"":\""רַבּ֣וּ\"",\""lemma\"":\""רָבַב\"",\""pos\"":\""verb\"",\""morph\"":\""qal-perfect(qatal)-3cp\"",\""gloss\"":\""they are many\""},{\""i\"":14,\""surface\"":\""צָרָ֑\"",\""lemma\"":\""צַר\"",\""pos\"":\""noun\"",\""morph\"":\""common-mp-const\"",\""gloss\"":\""opponents\""},{\""i\"":15,\""surface\"":\""י\"",\""lemma\"":\""י\"",\""pos\"":\""suffix\"",\""morph\"":\""pronominal-1bs\"",\""gloss\"":\""my\""},{\""i\"":16,\""surface\"":\""רַ֝בִּ֗ים\"",\""lemma\"":\""רַב\"",\""pos\"":\""adjective\"",\""morph\"":\""common-mp-abs\"",\""gloss\"":\""many [people]\""},{\""i\"":17,\""surface\"":\""קָמִ֥ים\"",\""lemma\"":\""קוּם\"",\""pos\"":\""verb\"",\""morph\"":\""qal-participle active-mp-abs\"",\""gloss\"":\""[are] rising up\""},{\""i\"":18,\""surface\"":\""עָלָֽ\"",\""lemma\"":\""עַל\"",\""pos\"":\""preposition\"",\""morph\"":\""inseparable_prep\"",\""gloss\"":\""on\""},{\""i\"":19,\""surface\"":\""י\"",\""lemma\"":\""נִי\"",\""pos\"":\""suffix\"",\""morph\"":\""pronominal-1bs\"",\""gloss\"":\""me\""}]},\""target\"":{\""text\"":\""A psalm of David. When he fled from his son Absalom. Lord , how many are my foes! How many rise up against me!\"",\""tokens\"":[{\""i\"":0,\""word\"":\""A\""},{\""i\"":1,\""word\"":\""psalm\""},{\""i\"":2,\""word\"":\""of\""},{\""i\"":3,\""word\"":\""David.\""},{\""i\"":4,\""word\"":\""When\""},{\""i\"":5,\""word\"":\""he\""},{\""i\"":6,\""word\"":\""fled\""},{\""i\"":7,\""word\"":\""from\""},{\""i\"":8,\""word\"":\""his\""},{\""i\"":9,\""word\"":\""son\""},{\""i\"":10,\""word\"":\""Absalom.\""},{\""i\"":11,\""word\"":\""Lord\""},{\""i\"":12,\""word\"":\"",\""},{\""i\"":13,\""word\"":\""how\""},{\""i\"":14,\""word\"":\""many\""},{\""i\"":15,\""word\"":\""are\""},{\""i\"":16,\""word\"":\""my\""},{\""i\"":17,\""word\"":\""foes!\""},{\""i\"":18,\""word\"":\""How\""},{\""i\"":19,\""word\"":\""many\""},{\""i\"":20,\""word\"":\""rise\""},{\""i\"":21,\""word\"":\""up\""},{\""i\"":22,\""word\"":\""against\""},{\""i\"":23,\""word\"":\""me!\""}]}},{\""reference\"":\""Psa.3.2\"",\""hebrew\"":{\""text\"":\""רַבִּים֮ אֹמְרִ֪ים לְנַ֫פְשִׁ֥י אֵ֤ין יְֽשׁוּעָ֓תָה לּ֬וֹ בֵֽאלֹהִ֬ים סֶֽלָה\"",\""tokens\"":[{\""i\"":0,\""surface\"":\""רַבִּים֮\"",\""lemma\"":\""רַב\"",\""pos\"":\""adjective\"",\""morph\"":\""common-mp-abs\"",\""gloss\"":\""many [people]\""},{\""i\"":1,\""surface\"":\""אֹמְרִ֪ים\"",\""lemma\"":\""אָמַר\"",\""pos\"":\""verb\"",\""morph\"":\""qal-participle active-mp-abs\"",\""gloss\"":\""[are] saying\""},{\""i\"":2,\""surface\"":\""לְ\"",\""lemma\"":\""ל\"",\""pos\"":\""preposition\"",\""morph\"":\""inseparable_prep\"",\""gloss\"":\""of\""},{\""i\"":3,\""surface\"":\""נַ֫פְשִׁ֥\"",\""lemma\"":\""נֶ֫פֶשׁ\"",\""pos\"":\""noun\"",\""morph\"":\""common-fs-const\"",\""gloss\"":\""self\""},{\""i\"":4,\""surface\"":\""י\"",\""lemma\"":\""י\"",\""pos\"":\""suffix\"",\""morph\"":\""pronominal-1bs\"",\""gloss\"":\""my\""},{\""i\"":5,\""surface\"":\""אֵ֤ין\"",\""lemma\"":\""אַ֫יִן\"",\""pos\"":\""noun\"",\""morph\"":\""common-ms-const\"",\""gloss\"":\""there not\""},{\""i\"":6,\""surface\"":\""יְֽשׁוּעָ֓תָה\"",\""lemma\"":\""יְשׁוּעָה\"",\""pos\"":\""noun\"",\""morph\"":\""common-fs-abs\"",\""gloss\"":\""[is] deliverance\""},{\""i\"":7,\""surface\"":\""לּ֬\"",\""lemma\"":\""ל\"",\""pos\"":\""preposition\"",\""morph\"":\""inseparable_prep\"",\""gloss\"":\""for\""},{\""i\"":8,\""surface\"":\""וֹ\"",\""lemma\"":\""וֹ\"",\""pos\"":\""suffix\"",\""morph\"":\""pronominal-3ms\"",\""gloss\"":\""him\""},{\""i\"":9,\""surface\"":\""בֵֽ\"",\""lemma\"":\""ב\"",\""pos\"":\""preposition\"",\""morph\"":\""inseparable_prep\"",\""gloss\"":\""in\""},{\""i\"":10,\""surface\"":\""אלֹהִ֬ים\"",\""lemma\"":\""אֱלֹהִים\"",\""pos\"":\""noun\"",\""morph\"":\""common-mp-abs\"",\""gloss\"":\""God\""},{\""i\"":11,\""surface\"":\""סֶֽלָה\"",\""lemma\"":\""סֶ֫לָה\"",\""pos\"":\""particle\"",\""morph\"":\""interjection\"",\""gloss\"":\""Selah\""}]},\""target\"":{\""text\"":\""Many are saying of me, ‘God will not deliver him.’\"",\""tokens\"":[{\""i\"":0,\""word\"":\""Many\""},{\""i\"":1,\""word\"":\""are\""},{\""i\"":2,\""word\"":\""saying\""},{\""i\"":3,\""word\"":\""of\""},{\""i\"":4,\""word\"":\""me,\""},{\""i\"":5,\""word\"":\""‘God\""},{\""i\"":6,\""word\"":\""will\""},{\""i\"":7,\""word\"":\""not\""},{\""i\"":8,\""word\"":\""deliver\""},{\""i\"":9,\""word\"":\""him.’\""}]}},{\""reference\"":\""Psa.3.3\"",\""hebrew\"":{\""text\"":\""וְאַתָּ֣ה יְ֭הוָה מָגֵ֣ן בַּעֲדִ֑י כְּ֝בוֹדִ֗י וּמֵרִ֥ים רֹאשִֽׁי\"",\""tokens\"":[{\""i\"":0,\""surface\"":\""וְ\"",\""lemma\"":\""ו\"",\""pos\"":\""conjunction\"",\""morph\"":\""conjunction\"",\""gloss\"":\""and\""},{\""i\"":1,\""surface\"":\""אַתָּ֣ה\"",\""lemma\"":\""אַתָּ֫ה\"",\""pos\"":\""pronoun\"",\""morph\"":\""personal-2ms\"",\""gloss\"":\""you\""},{\""i\"":2,\""surface\"":\""יְ֭הוָה\"",\""lemma\"":\""יהוה\"",\""pos\"":\""noun\"",\""morph\"":\""proper\"",\""gloss\"":\""O Yahweh\""},{\""i\"":3,\""surface\"":\""מָגֵ֣ן\"",\""lemma\"":\""מָגֵן\"",\""pos\"":\""noun\"",\""morph\"":\""common-bs-abs\"",\""gloss\"":\""[are] a shield\""},{\""i\"":4,\""surface\"":\""בַּעֲדִ֑\"",\""lemma\"":\""בַּ֫עַד\"",\""pos\"":\""adjective\"",\""morph\"":\""num-bs-const\"",\""gloss\"":\""behind\""},{\""i\"":5,\""surface\"":\""י\"",\""lemma\"":\""נִי\"",\""pos\"":\""suffix\"",\""morph\"":\""pronominal-1bs\"",\""gloss\"":\""me\""},{\""i\"":6,\""surface\"":\""כְּ֝בוֹדִ֗\"",\""lemma\"":\""כָּבוֹד\"",\""pos\"":\""noun\"",\""morph\"":\""common-ms-const\"",\""gloss\"":\""honor\""},{\""i\"":7,\""surface\"":\""י\"",\""lemma\"":\""י\"",\""pos\"":\""suffix\"",\""morph\"":\""pronominal-1ms\"",\""gloss\"":\""my\""},{\""i\"":8,\""surface\"":\""וּ\"",\""lemma\"":\""ו\"",\""pos\"":\""conjunction\"",\""morph\"":\""conjunction\"",\""gloss\"":\""and\""},{\""i\"":9,\""surface\"":\""מֵרִ֥ים\"",\""lemma\"":\""רוּם\"",\""pos\"":\""verb\"",\""morph\"":\""hiphil-participle active-ms-abs\"",\""gloss\"":\""[the one who] lifts up\""},{\""i\"":10,\""surface\"":\""רֹאשִֽׁ\"",\""lemma\"":\""רֹאשׁ\"",\""pos\"":\""noun\"",\""morph\"":\""common-ms-const\"",\""gloss\"":\""head\""},{\""i\"":11,\""surface\"":\""י\"",\""lemma\"":\""י\"",\""pos\"":\""suffix\"",\""morph\"":\""pronominal-1bs\"",\""gloss\"":\""my\""}]},\""target\"":{\""text\"":\""But you, Lord , are a shield around me, my glory, the One who lifts my head high.\"",\""tokens\"":[{\""i\"":0,\""word\"":\""But\""},{\""i\"":1,\""word\"":\""you,\""},{\""i\"":2,\""word\"":\""Lord\""},{\""i\"":3,\""word\"":\"",\""},{\""i\"":4,\""word\"":\""are\""},{\""i\"":5,\""word\"":\""a\""},{\""i\"":6,\""word\"":\""shield\""},{\""i\"":7,\""word\"":\""around\""},{\""i\"":8,\""word\"":\""me,\""},{\""i\"":9,\""word\"":\""my\""},{\""i\"":10,\""word\"":\""glory,\""},{\""i\"":11,\""word\"":\""the\""},{\""i\"":12,\""word\"":\""One\""},{\""i\"":13,\""word\"":\""who\""},{\""i\"":14,\""word\"":\""lifts\""},{\""i\"":15,\""word\"":\""my\""},{\""i\"":16,\""word\"":\""head\""},{\""i\"":17,\""word\"":\""high.\""}]}},{\""reference\"":\""Psa.3.4\"",\""hebrew\"":{\""text\"":\""ק֭וֹלִי אֶל־ יְהוָ֣ה אֶקְרָ֑א וַיַּֽעֲנֵ֨נִי מֵהַ֖ר קָדְשׁ֣וֹ סֶֽלָה\"",\""tokens\"":[{\""i\"":0,\""surface\"":\""ק֭וֹלִ\"",\""lemma\"":\""קוֹל\"",\""pos\"":\""noun\"",\""morph\"":\""common-ms-const\"",\""gloss\"":\""voice\""},{\""i\"":1,\""surface\"":\""י\"",\""lemma\"":\""י\"",\""pos\"":\""suffix\"",\""morph\"":\""pronominal-1bs\"",\""gloss\"":\""my\""},{\""i\"":2,\""surface\"":\""אֶל\"",\""lemma\"":\""אֶל\"",\""pos\"":\""preposition\"",\""morph\"":\""inseparable_prep\"",\""gloss\"":\""to\""},{\""i\"":3,\""surface\"":\""יְהוָ֣ה\"",\""lemma\"":\""יהוה\"",\""pos\"":\""noun\"",\""morph\"":\""proper\"",\""gloss\"":\""Yahweh\""},{\""i\"":4,\""surface\"":\""אֶקְרָ֑א\"",\""lemma\"":\""קָרָא\"",\""pos\"":\""verb\"",\""morph\"":\""qal-imperfect(yiqtol)-1cs\"",\""gloss\"":\""I called out\""},{\""i\"":5,\""surface\"":\""וַ\"",\""lemma\"":\""ו\"",\""pos\"":\""conjunction\"",\""morph\"":\""conjunction\"",\""gloss\"":\""and\""},{\""i\"":6,\""surface\"":\""יַּֽעֲנֵ֨\"",\""lemma\"":\""עָנָה\"",\""pos\"":\""verb\"",\""morph\"":\""qal-sequential imperfect(wayyiqtol)-3ms\"",\""gloss\"":\""he answered\""},{\""i\"":7,\""surface\"":\""נִי\"",\""lemma\"":\""נִי\"",\""pos\"":\""suffix\"",\""morph\"":\""pronominal-1bs\"",\""gloss\"":\""me\""},{\""i\"":8,\""surface\"":\""מֵ\"",\""lemma\"":\""מ\"",\""pos\"":\""preposition\"",\""morph\"":\""inseparable_prep\"",\""gloss\"":\""from\""},{\""i\"":9,\""surface\"":\""הַ֖ר\"",\""lemma\"":\""הַר\"",\""pos\"":\""noun\"",\""morph\"":\""common-ms-const\"",\""gloss\"":\""[the] mountain of\""},{\""i\"":10,\""surface\"":\""קָדְשׁ֣\"",\""lemma\"":\""קֹ֫דֶשׁ\"",\""pos\"":\""noun\"",\""morph\"":\""common-ms-const\"",\""gloss\"":\""holiness\""},{\""i\"":11,\""surface\"":\""וֹ\"",\""lemma\"":\""הוּ\"",\""pos\"":\""suffix\"",\""morph\"":\""pronominal-3ms\"",\""gloss\"":\""his\""},{\""i\"":12,\""surface\"":\""סֶֽלָה\"",\""lemma\"":\""סֶ֫לָה\"",\""pos\"":\""particle\"",\""morph\"":\""interjection\"",\""gloss\"":\""Selah\""}]},\""target\"":{\""text\"":\""I call out to the Lord , and he answers me from his holy mountain.\"",\""tokens\"":[{\""i\"":0,\""word\"":\""I\""},{\""i\"":1,\""word\"":\""call\""},{\""i\"":2,\""word\"":\""out\""},{\""i\"":3,\""word\"":\""to\""},{\""i\"":4,\""word\"":\""the\""},{\""i\"":5,\""word\"":\""Lord\""},{\""i\"":6,\""word\"":\"",\""},{\""i\"":7,\""word\"":\""and\""},{\""i\"":8,\""word\"":\""he\""},{\""i\"":9,\""word\"":\""answers\""},{\""i\"":10,\""word\"":\""me\""},{\""i\"":11,\""word\"":\""from\""},{\""i\"":12,\""word\"":\""his\""},{\""i\"":13,\""word\"":\""holy\""},{\""i\"":14,\""word\"":\""mountain.\""}]}},{\""reference\"":\""Psa.3.5\"",\""hebrew\"":{\""text\"":\""אֲנִ֥י שָׁכַ֗בְתִּי וָֽאִ֫ישָׁ֥נָה הֱקִיצ֑וֹתִי כִּ֖י יְהוָ֣ה יִסְמְכֵֽנִי\"",\""tokens\"":[{\""i\"":0,\""surface\"":\""אֲנִ֥י\"",\""lemma\"":\""אֲנִי, אָֽנֹכִ֫י\"",\""pos\"":\""pronoun\"",\""morph\"":\""personal-1bs\"",\""gloss\"":\""I\""},{\""i\"":1,\""surface\"":\""שָׁכַ֗בְתִּי\"",\""lemma\"":\""שָׁכַב\"",\""pos\"":\""verb\"",\""morph\"":\""qal-perfect(qatal)-1cs\"",\""gloss\"":\""I lay down\""},{\""i\"":2,\""surface\"":\""וָֽ\"",\""lemma\"":\""ו\"",\""pos\"":\""conjunction\"",\""morph\"":\""conjunction\"",\""gloss\"":\""and\""},{\""i\"":3,\""surface\"":\""אִ֫ישָׁ֥נָ\"",\""lemma\"":\""יָשֵׁן\"",\""pos\"":\""verb\"",\""morph\"":\""qal-sequential imperfect(wayyiqtol)-1cs\"",\""gloss\"":\""I slept\""},{\""i\"":4,\""surface\"":\""ה\"",\""lemma\"":\""ה\"",\""pos\"":\""suffix\"",\""morph\"":\""paragogic he\"",\""gloss\"":\""!\""},{\""i\"":5,\""surface\"":\""הֱקִיצ֑וֹתִי\"",\""lemma\"":\""קִיץ\"",\""pos\"":\""verb\"",\""morph\"":\""hiphil-perfect(qatal)-1cs\"",\""gloss\"":\""I awoke\""},{\""i\"":6,\""surface\"":\""כִּ֖י\"",\""lemma\"":\""כִּי\"",\""pos\"":\""particle\"",\""morph\"":\""conditional\"",\""gloss\"":\""for\""},{\""i\"":7,\""surface\"":\""יְהוָ֣ה\"",\""lemma\"":\""יהוה\"",\""pos\"":\""noun\"",\""morph\"":\""proper\"",\""gloss\"":\""Yahweh\""},{\""i\"":8,\""surface\"":\""יִסְמְכֵֽ\"",\""lemma\"":\""סָמַךְ\"",\""pos\"":\""verb\"",\""morph\"":\""qal-imperfect(yiqtol)-3ms\"",\""gloss\"":\""he sustains\""},{\""i\"":9,\""surface\"":\""נִי\"",\""lemma\"":\""נִי\"",\""pos\"":\""suffix\"",\""morph\"":\""pronominal-1bs\"",\""gloss\"":\""me\""}]},\""target\"":{\""text\"":\""I lie down and sleep; I wake again, because the Lord sustains me.\"",\""tokens\"":[{\""i\"":0,\""word\"":\""I\""},{\""i\"":1,\""word\"":\""lie\""},{\""i\"":2,\""word\"":\""down\""},{\""i\"":3,\""word\"":\""and\""},{\""i\"":4,\""word\"":\""sleep;\""},{\""i\"":5,\""word\"":\""I\""},{\""i\"":6,\""word\"":\""wake\""},{\""i\"":7,\""word\"":\""again,\""},{\""i\"":8,\""word\"":\""because\""},{\""i\"":9,\""word\"":\""the\""},{\""i\"":10,\""word\"":\""Lord\""},{\""i\"":11,\""word\"":\""sustains\""},{\""i\"":12,\""word\"":\""me.\""}]}},{\""reference\"":\""Psa.3.6\"",\""hebrew\"":{\""text\"":\""לֹֽא־ אִ֭ירָא מֵרִבְב֥וֹת עָ֑ם אֲשֶׁ֥ר סָ֝בִ֗יב שָׁ֣תוּ עָלָֽי\"",\""tokens\"":[{\""i\"":0,\""surface\"":\""לֹֽא\"",\""lemma\"":\""לֹא\"",\""pos\"":\""particle\"",\""morph\"":\""negative\"",\""gloss\"":\""not\""},{\""i\"":1,\""surface\"":\""אִ֭ירָא\"",\""lemma\"":\""יָרֵא\"",\""pos\"":\""verb\"",\""morph\"":\""qal-imperfect(yiqtol)-1cs\"",\""gloss\"":\""I will be afraid\""},{\""i\"":2,\""surface\"":\""מֵ\"",\""lemma\"":\""מ\"",\""pos\"":\""preposition\"",\""morph\"":\""inseparable_prep\"",\""gloss\"":\""from\""},{\""i\"":3,\""surface\"":\""רִבְב֥וֹת\"",\""lemma\"":\""רְבָבָה\"",\""pos\"":\""noun\"",\""morph\"":\""common-fp-const\"",\""gloss\"":\""ten thousands of\""},{\""i\"":4,\""surface\"":\""עָ֑ם\"",\""lemma\"":\""עַם\"",\""pos\"":\""noun\"",\""morph\"":\""common-ms-abs\"",\""gloss\"":\""people\""},{\""i\"":5,\""surface\"":\""אֲשֶׁ֥ר\"",\""lemma\"":\""אֲשֶׁר\"",\""pos\"":\""particle\"",\""morph\"":\""relative\"",\""gloss\"":\""who\""},{\""i\"":6,\""surface\"":\""סָ֝בִ֗יב\"",\""lemma\"":\""סָבִיב\"",\""pos\"":\""noun\"",\""morph\"":\""common-bs-abs\"",\""gloss\"":\""all around\""},{\""i\"":7,\""surface\"":\""שָׁ֣תוּ\"",\""lemma\"":\""שִׁית\"",\""pos\"":\""verb\"",\""morph\"":\""qal-perfect(qatal)-3cp\"",\""gloss\"":\""they have taken a stand\""},{\""i\"":8,\""surface\"":\""עָלָֽ\"",\""lemma\"":\""עַל\"",\""pos\"":\""preposition\"",\""morph\"":\""inseparable_prep\"",\""gloss\"":\""on\""},{\""i\"":9,\""surface\"":\""י\"",\""lemma\"":\""נִי\"",\""pos\"":\""suffix\"",\""morph\"":\""pronominal-1bs\"",\""gloss\"":\""me\""}]},\""target\"":{\""text\"":\""I will not fear though tens of thousands assail me on every side.\"",\""tokens\"":[{\""i\"":0,\""word\"":\""I\""},{\""i\"":1,\""word\"":\""will\""},{\""i\"":2,\""word\"":\""not\""},{\""i\"":3,\""word\"":\""fear\""},{\""i\"":4,\""word\"":\""though\""},{\""i\"":5,\""word\"":\""tens\""},{\""i\"":6,\""word\"":\""of\""},{\""i\"":7,\""word\"":\""thousands\""},{\""i\"":8,\""word\"":\""assail\""},{\""i\"":9,\""word\"":\""me\""},{\""i\"":10,\""word\"":\""on\""},{\""i\"":11,\""word\"":\""every\""},{\""i\"":12,\""word\"":\""side.\""}]}},{\""reference\"":\""Psa.3.7\"",\""hebrew\"":{\""text\"":\""ק֘וּמָ֤ה יְהוָ֨ה׀ הוֹשִׁ֘יעֵ֤נִי אֱלֹהַ֗י כִּֽי־ הִכִּ֣יתָ אֶת־ כָּל־ אֹיְבַ֣י לֶ֑חִי שִׁנֵּ֖י רְשָׁעִ֣ים שִׁבַּֽרְתָּ\"",\""tokens\"":[{\""i\"":0,\""surface\"":\""ק֘וּמָ֤\"",\""lemma\"":\""קוּם\"",\""pos\"":\""verb\"",\""morph\"":\""qal-imperative-2ms\"",\""gloss\"":\""arise!\""},{\""i\"":1,\""surface\"":\""ה\"",\""lemma\"":\""ה\"",\""pos\"":\""suffix\"",\""morph\"":\""paragogic he\"",\""gloss\"":\""!\""},{\""i\"":2,\""surface\"":\""יְהוָ֨ה\"",\""lemma\"":\""יהוה\"",\""pos\"":\""noun\"",\""morph\"":\""proper\"",\""gloss\"":\""O Yahweh\""},{\""i\"":3,\""surface\"":\""הוֹשִׁ֘יעֵ֤\"",\""lemma\"":\""יָשַׁע\"",\""pos\"":\""verb\"",\""morph\"":\""hiphil-imperative-2ms\"",\""gloss\"":\""save\""},{\""i\"":4,\""surface\"":\""נִי\"",\""lemma\"":\""נִי\"",\""pos\"":\""suffix\"",\""morph\"":\""pronominal-1bs\"",\""gloss\"":\""me!\""},{\""i\"":5,\""surface\"":\""אֱלֹהַ֗\"",\""lemma\"":\""אֱלֹהִים\"",\""pos\"":\""noun\"",\""morph\"":\""common-mp-const\"",\""gloss\"":\""O God\""},{\""i\"":6,\""surface\"":\""י\"",\""lemma\"":\""י\"",\""pos\"":\""suffix\"",\""morph\"":\""pronominal-1bs\"",\""gloss\"":\""my\""},{\""i\"":7,\""surface\"":\""כִּֽי\"",\""lemma\"":\""כִּי\"",\""pos\"":\""particle\"",\""morph\"":\""conditional\"",\""gloss\"":\""for\""},{\""i\"":8,\""surface\"":\""הִכִּ֣יתָ\"",\""lemma\"":\""נָכָה\"",\""pos\"":\""verb\"",\""morph\"":\""hiphil-perfect(qatal)-2ms\"",\""gloss\"":\""you have struck\""},{\""i\"":9,\""surface\"":\""אֶת\"",\""lemma\"":\""אֵת\"",\""pos\"":\""particle\"",\""morph\"":\""direct object marker\"",\""gloss\"":\""\\u003Cobj.\\u003E\""},{\""i\"":10,\""surface\"":\""כָּל\"",\""lemma\"":\""כֹּל\"",\""pos\"":\""noun\"",\""morph\"":\""common-ms-const\"",\""gloss\"":\""all\""},{\""i\"":11,\""surface\"":\""אֹיְבַ֣\"",\""lemma\"":\""אֹיֵב\"",\""pos\"":\""verb\"",\""morph\"":\""qal-participle active-mp-const\"",\""gloss\"":\""enemies\""},{\""i\"":12,\""surface\"":\""י\"",\""lemma\"":\""י\"",\""pos\"":\""suffix\"",\""morph\"":\""pronominal-1bs\"",\""gloss\"":\""my\""},{\""i\"":13,\""surface\"":\""לֶ֑חִי\"",\""lemma\"":\""לְחִי\"",\""pos\"":\""noun\"",\""morph\"":\""common-ms-abs\"",\""gloss\"":\""jaw\""},{\""i\"":14,\""surface\"":\""שִׁנֵּ֖י\"",\""lemma\"":\""שֵׁן\"",\""pos\"":\""noun\"",\""morph\"":\""common-bd-const\"",\""gloss\"":\""[the] teeth of\""},{\""i\"":15,\""surface\"":\""רְשָׁעִ֣ים\"",\""lemma\"":\""רָשָׁע\"",\""pos\"":\""adjective\"",\""morph\"":\""common-mp-abs\"",\""gloss\"":\""wicked [people]\""},{\""i\"":16,\""surface\"":\""שִׁבַּֽרְתָּ\"",\""lemma\"":\""שָׁבַר\"",\""pos\"":\""verb\"",\""morph\"":\""piel-perfect(qatal)-2ms\"",\""gloss\"":\""you have broken\""}]},\""target\"":{\""text\"":\""Arise, Lord ! Deliver me, my God! Strike all my enemies on the jaw; break the teeth of the wicked.\"",\""tokens\"":[{\""i\"":0,\""word\"":\""Arise,\""},{\""i\"":1,\""word\"":\""Lord\""},{\""i\"":2,\""word\"":\""!\""},{\""i\"":3,\""word\"":\""Deliver\""},{\""i\"":4,\""word\"":\""me,\""},{\""i\"":5,\""word\"":\""my\""},{\""i\"":6,\""word\"":\""God!\""},{\""i\"":7,\""word\"":\""Strike\""},{\""i\"":8,\""word\"":\""all\""},{\""i\"":9,\""word\"":\""my\""},{\""i\"":10,\""word\"":\""enemies\""},{\""i\"":11,\""word\"":\""on\""},{\""i\"":12,\""word\"":\""the\""},{\""i\"":13,\""word\"":\""jaw;\""},{\""i\"":14,\""word\"":\""break\""},{\""i\"":15,\""word\"":\""the\""},{\""i\"":16,\""word\"":\""teeth\""},{\""i\"":17,\""word\"":\""of\""},{\""i\"":18,\""word\"":\""the\""},{\""i\"":19,\""word\"":\""wicked.\""}]}},{\""reference\"":\""Psa.3.8\"",\""hebrew\"":{\""text\"":\""לַיהוָ֥ה הַיְשׁוּעָ֑ה עַֽל־ עַמְּךָ֖ בִרְכָתֶ֣ךָ סֶּֽלָה\"",\""tokens\"":[{\""i\"":0,\""surface\"":\""לַ\"",\""lemma\"":\""ל\"",\""pos\"":\""preposition\"",\""morph\"":\""inseparable_prep\"",\""gloss\"":\""[belongs] to\""},{\""i\"":1,\""surface\"":\""יהוָ֥ה\"",\""lemma\"":\""יהוה\"",\""pos\"":\""noun\"",\""morph\"":\""proper\"",\""gloss\"":\""Yahweh\""},{\""i\"":2,\""surface\"":\""הַ\"",\""lemma\"":\""ה\"",\""pos\"":\""particle\"",\""morph\"":\""definite article\"",\""gloss\"":\""\\u003Cthe\\u003E\""},{\""i\"":3,\""surface\"":\""יְשׁוּעָ֑ה\"",\""lemma\"":\""יְשׁוּעָה\"",\""pos\"":\""noun\"",\""morph\"":\""common-fs-abs\"",\""gloss\"":\""deliverance\""},{\""i\"":4,\""surface\"":\""עַֽל\"",\""lemma\"":\""עַל\"",\""pos\"":\""preposition\"",\""morph\"":\""inseparable_prep\"",\""gloss\"":\""[is] towards\""},{\""i\"":5,\""surface\"":\""עַמְּ\"",\""lemma\"":\""עַם\"",\""pos\"":\""noun\"",\""morph\"":\""common-ms-const\"",\""gloss\"":\""people\""},{\""i\"":6,\""surface\"":\""ךָ֖\"",\""lemma\"":\""ךָ\"",\""pos\"":\""suffix\"",\""morph\"":\""pronominal-2ms\"",\""gloss\"":\""your\""},{\""i\"":7,\""surface\"":\""בִרְכָתֶ֣\"",\""lemma\"":\""בְּרָכָה\"",\""pos\"":\""noun\"",\""morph\"":\""common-fs-const\"",\""gloss\"":\""blessing\""},{\""i\"":8,\""surface\"":\""ךָ\"",\""lemma\"":\""ךָ\"",\""pos\"":\""suffix\"",\""morph\"":\""pronominal-2ms\"",\""gloss\"":\""your\""},{\""i\"":9,\""surface\"":\""סֶּֽלָה\"",\""lemma\"":\""סֶ֫לָה\"",\""pos\"":\""particle\"",\""morph\"":\""interjection\"",\""gloss\"":\""Selah\""}]},\""target\"":{\""text\"":\""From the Lord comes deliverance. May your blessing be on your people.\"",\""tokens\"":[{\""i\"":0,\""word\"":\""From\""},{\""i\"":1,\""word\"":\""the\""},{\""i\"":2,\""word\"":\""Lord\""},{\""i\"":3,\""word\"":\""comes\""},{\""i\"":4,\""word\"":\""deliverance.\""},{\""i\"":5,\""word\"":\""May\""},{\""i\"":6,\""word\"":\""your\""},{\""i\"":7,\""word\"":\""blessing\""},{\""i\"":8,\""word\"":\""be\""},{\""i\"":9,\""word\"":\""on\""},{\""i\"":10,\""word\"":\""your\""},{\""i\"":11,\""word\"":\""people.\""}]}}]
";

            string resp = @"{
  ""candidates"": [
    {
      ""content"": {
        ""parts"": [
          {
            ""text"": ""[\n  {\n    \""reference\"": \""Psa.3.1\"",\n    \""alignments\"": [\n      {\n        \""t\"": [\n          0\n        ],\n        \""h\"": []\n      },\n      {\n        \""t\"": [\n          1\n        ],\n        \""h\"": [\n          0\n        ]\n      },\n      {\n        \""t\"": [\n          2\n        ],\n        \""h\"": [\n          1\n        ]\n      },\n      {\n        \""t\"": [\n          3\n        ],\n        \""h\"": [\n          2\n        ]\n      },\n      {\n        \""t\"": [\n          4\n        ],\n        \""h\"": [\n          3\n        ]\n      },\n      {\n        \""t\"": [\n          5\n        ],\n        \""h\"": [\n          5\n        ]\n      },\n      {\n        \""t\"": [\n          6\n        ],\n        \""h\"": [\n          4\n        ]\n      },\n      {\n        \""t\"": [\n          7\n        ],\n        \""h\"": [\n          6,\n          7\n        ]\n      },\n      {\n        \""t\"": [\n          8\n        ],\n        \""h\"": [\n          10\n        ]\n      },\n      {\n        \""t\"": [\n          9\n        ],\n        \""h\"": [\n          9\n        ]\n      },\n      {\n        \""t\"": [\n          10\n        ],\n        \""h\"": [\n          8\n        ]\n      },\n      {\n        \""t\"": [\n          11\n        ],\n        \""h\"": [\n          11\n        ]\n      },\n      {\n        \""t\"": [\n          12\n        ],\n        \""h\"": []\n      },\n      {\n        \""t\"": [\n          13\n        ],\n        \""h\"": [\n          12\n        ]\n      },\n      {\n        \""t\"": [\n          14\n        ],\n        \""h\"": [\n          13\n        ]\n      },\n      {\n        \""t\"": [\n          15\n        ],\n        \""h\"": []\n      },\n      {\n        \""t\"": [\n          16\n        ],\n        \""h\"": [\n          15\n        ]\n      },\n      {\n        \""t\"": [\n          17\n        ],\n        \""h\"": [\n          14\n        ]\n      },\n      {\n        \""t\"": [\n          18\n        ],\n        \""h\"": []\n      },\n      {\n        \""t\"": [\n          19\n        ],\n        \""h\"": [\n          16\n        ]\n      },\n      {\n        \""t\"": [\n          20,\n          21\n        ],\n        \""h\"": [\n          17\n        ]\n      },\n      {\n        \""t\"": [\n          22\n        ],\n        \""h\"": [\n          18\n        ]\n      },\n      {\n        \""t\"": [\n          23\n        ],\n        \""h\"": [\n          19\n        ]\n      }\n    ]\n  },\n  {\n    \""reference\"": \""Psa.3.2\"",\n    \""alignments\"": [\n      {\n        \""t\"": [\n          0\n        ],\n        \""h\"": [\n          0\n        ]\n      },\n      {\n        \""t\"": [\n          1,\n          2\n        ],\n        \""h\"": [\n          1\n        ]\n      },\n      {\n        \""t\"": [\n          3,\n          4\n        ],\n        \""h\"": [\n          2,\n          3,\n          4\n        ]\n      },\n      {\n        \""t\"": [\n          5\n        ],\n        \""h\"": [\n          10\n        ]\n      },\n      {\n        \""t\"": [\n          6,\n          7,\n          8\n        ],\n        \""h\"": [\n          5,\n          6\n        ]\n      },\n      {\n        \""t\"": [\n          9\n        ],\n        \""h\"": [\n          7,\n          8\n        ]\n      }\n    ]\n  },\n  {\n    \""reference\"": \""Psa.3.3\"",\n    \""alignments\"": [\n      {\n        \""t\"": [\n          0\n        ],\n        \""h\"": [\n          0\n        ]\n      },\n      {\n        \""t\"": [\n          1\n        ],\n        \""h\"": [\n          1\n        ]\n      },\n      {\n        \""t\"": [\n          2\n        ],\n        \""h\"": [\n          2\n        ]\n      },\n      {\n        \""t\"": [\n          3\n        ],\n        \""h\"": []\n      },\n      {\n        \""t\"": [\n          4,\n          5,\n          6\n        ],\n        \""h\"": [\n          3\n        ]\n      },\n      {\n        \""t\"": [\n          7,\n          8\n        ],\n        \""h\"": [\n          4,\n          5\n        ]\n      },\n      {\n        \""t\"": [\n          9\n        ],\n        \""h\"": [\n          7\n        ]\n      },\n      {\n        \""t\"": [\n          10\n        ],\n        \""h\"": [\n          6\n        ]\n      },\n      {\n        \""t\"": [\n          11,\n          12,\n          13,\n          14\n        ],\n        \""h\"": [\n          8,\n          9\n        ]\n      },\n      {\n        \""t\"": [\n          15\n        ],\n        \""h\"": [\n          11\n        ]\n      },\n      {\n        \""t\"": [\n          16\n        ],\n        \""h\"": [\n          10\n        ]\n      },\n      {\n        \""t\"": [\n          17\n        ],\n        \""h\"": []\n      }\n    ]\n  },\n  {\n    \""reference\"": \""Psa.3.4\"",\n    \""alignments\"": [\n      {\n        \""t\"": [\n          0,\n          1,\n          2\n        ],\n        \""h\"": [\n          4\n        ]\n      },\n      {\n        \""t\"": [\n          3\n        ],\n        \""h\"": [\n          2\n        ]\n      },\n      {\n        \""t\"": [\n          4\n        ],\n        \""h\"": []\n      },\n      {\n        \""t\"": [\n          5\n        ],\n        \""h\"": [\n          3\n        ]\n      },\n      {\n        \""t\"": [\n          6\n        ],\n        \""h\"": []\n      },\n      {\n        \""t\"": [\n          7\n        ],\n        \""h\"": [\n          5\n        ]\n      },\n      {\n        \""t\"": [\n          8\n        ],\n        \""h\"": []\n      },\n      {\n        \""t\"": [\n          9\n        ],\n        \""h\"": [\n          6\n        ]\n      },\n      {\n        \""t\"": [\n          10\n        ],\n        \""h\"": [\n          7\n        ]\n      },\n      {\n        \""t\"": [\n          11\n        ],\n        \""h\"": [\n          8\n        ]\n      },\n      {\n        \""t\"": [\n          12\n        ],\n        \""h\"": [\n          11\n        ]\n      },\n      {\n        \""t\"": [\n          13\n        ],\n        \""h\"": [\n          10\n        ]\n      },\n      {\n        \""t\"": [\n          14\n        ],\n        \""h\"": [\n          9\n        ]\n      }\n    ]\n  },\n  {\n    \""reference\"": \""Psa.3.5\"",\n    \""alignments\"": [\n      {\n        \""t\"": [\n          0\n        ],\n        \""h\"": [\n          0\n        ]\n      },\n      {\n        \""t\"": [\n          1,\n          2\n        ],\n        \""h\"": [\n          1\n        ]\n      },\n      {\n        \""t\"": [\n          3\n        ],\n        \""h\"": [\n          2\n        ]\n      },\n      {\n        \""t\"": [\n          4\n        ],\n        \""h\"": [\n          3,\n          4\n        ]\n      },\n      {\n        \""t\"": [\n          5\n        ],\n        \""h\"": []\n      },\n      {\n        \""t\"": [\n          6\n        ],\n        \""h\"": [\n          5\n        ]\n      },\n      {\n        \""t\"": [\n          7\n        ],\n        \""h\"": []\n      },\n      {\n        \""t\"": [\n          8\n        ],\n        \""h\"": [\n          6\n        ]\n      },\n      {\n        \""t\"": [\n          9\n        ],\n        \""h\"": []\n      },\n      {\n        \""t\"": [\n          10\n        ],\n        \""h\"": [\n          7\n        ]\n      },\n      {\n        \""t\"": [\n          11\n        ],\n        \""h\"": [\n          8\n        ]\n      },\n      {\n        \""t\"": [\n          12\n        ],\n        \""h\"": [\n          9\n        ]\n      }\n    ]\n  },\n  {\n    \""reference\"": \""Psa.3.6\"",\n    \""alignments\"": [\n      {\n        \""t\"": [\n          0,\n          1,\n          2,\n          3\n        ],\n        \""h\"": [\n          0,\n          1\n        ]\n      },\n      {\n        \""t\"": [\n          4\n        ],\n        \""h\"": [\n          5\n        ]\n      },\n      {\n        \""t\"": [\n          5,\n          6,\n          7\n        ],\n        \""h\"": [\n          3\n        ]\n      },\n      {\n        \""t\"": [\n          8\n        ],\n        \""h\"": [\n          7\n        ]\n      },\n      {\n        \""t\"": [\n          9\n        ],\n        \""h\"": [\n          9\n        ]\n      },\n      {\n        \""t\"": [\n          10,\n          11,\n          12\n        ],\n        \""h\"": [\n          6,\n          8\n        ]\n      }\n    ]\n  },\n  {\n    \""reference\"": \""Psa.3.7\"",\n    \""alignments\"": [\n      {\n        \""t\"": [\n          0\n        ],\n        \""h\"": [\n          0,\n          1\n        ]\n      },\n      {\n        \""t\"": [\n          1\n        ],\n        \""h\"": [\n          2\n        ]\n      },\n      {\n        \""t\"": [\n          2\n        ],\n        \""h\"": []\n      },\n      {\n        \""t\"": [\n          3\n        ],\n        \""h\"": [\n          3\n        ]\n      },\n      {\n        \""t\"": [\n          4\n        ],\n        \""h\"": [\n          4\n        ]\n      },\n      {\n        \""t\"": [\n          5\n        ],\n        \""h\"": [\n          6\n        ]\n      },\n      {\n        \""t\"": [\n          6\n        ],\n        \""h\"": [\n          5\n        ]\n      },\n      {\n        \""t\"": [\n          7\n        ],\n        \""h\"": [\n          8\n        ]\n      },\n      {\n        \""t\"": [\n          8\n        ],\n        \""h\"": [\n          10\n        ]\n      },\n      {\n        \""t\"": [\n          9\n        ],\n        \""h\"": [\n          12\n        ]\n      },\n      {\n        \""t\"": [\n          10\n        ],\n        \""h\"": [\n          11\n        ]\n      },\n      {\n        \""t\"": [\n          11\n        ],\n        \""h\"": []\n      },\n      {\n        \""t\"": [\n          12\n        ],\n        \""h\"": []\n      },\n      {\n        \""t\"": [\n          13\n        ],\n        \""h\"": [\n          13\n        ]\n      },\n      {\n        \""t\"": [\n          14\n        ],\n        \""h\"": [\n          16\n        ]\n      },\n      {\n        \""t\"": [\n          15\n        ],\n        \""h\"": []\n      },\n      {\n        \""t\"": [\n          16\n        ],\n        \""h\"": [\n          14\n        ]\n      },\n      {\n        \""t\"": [\n          17\n        ],\n        \""h\"": []\n      },\n      {\n        \""t\"": [\n          18\n        ],\n        \""h\"": []\n      },\n      {\n        \""t\"": [\n          19\n        ],\n        \""h\"": [\n          15\n        ]\n      }\n    ]\n  },\n  {\n    \""reference\"": \""Psa.3.8\"",\n    \""alignments\"": [\n      {\n        \""t\"": [\n          0\n        ],\n        \""h\"": [\n          0\n        ]\n      },\n      {\n        \""t\"": [\n          1\n        ],\n        \""h\"": [\n          2\n        ]\n      },\n      {\n        \""t\"": [\n          2\n        ],\n        \""h\"": [\n          1\n        ]\n      },\n      {\n        \""t\"": [\n          3\n        ],\n        \""h\"": []\n      },\n      {\n        \""t\"": [\n          4\n        ],\n        \""h\"": [\n          3\n        ]\n      },\n      {\n        \""t\"": [\n          5\n        ],\n        \""h\"": []\n      },\n      {\n        \""t\"": [\n          6\n        ],\n        \""h\"": [\n          8\n        ]\n      },\n      {\n        \""t\"": [\n          7\n        ],\n        \""h\"": [\n          7\n        ]\n      },\n      {\n        \""t\"": [\n          8\n        ],\n        \""h\"": []\n      },\n      {\n        \""t\"": [\n          9\n        ],\n        \""h\"": [\n          4\n        ]\n      },\n      {\n        \""t\"": [\n          10\n        ],\n        \""h\"": [\n          6\n        ]\n      },\n      {\n        \""t\"": [\n          11\n        ],\n        \""h\"": [\n          5\n        ]\n      }\n    ]\n  }\n]""
          }
        ],
        ""role"": ""model""
      },
      ""finishReason"": ""STOP""
    }
  ],
  ""usageMetadata"": {
    ""promptTokenCount"": 6496,
    ""candidatesTokenCount"": 3424,
    ""totalTokenCount"": 23532,
    ""promptTokensDetails"": [
      {
        ""modality"": ""TEXT"",
        ""tokenCount"": 6496
      }
    ],
    ""thoughtsTokenCount"": 13612
  },
  ""turnToken"": ""v1_ChdNaV9QYWQzRkZadlUxTWtQNUphR2tRTRIXTWlfUGFkM0ZGWnZVMU1rUDVKYUdrUU0""
}";
            bool success = true;
            StringBuilder errorString = new StringBuilder();

            string output = "{}"; // return empty JSON on error
            int inputTokens = 0;
            int outputTokens = 0;


            GenerateContentResponse response =
                GenerateContentResponse.FromJson(resp);

            var metadata = response.UsageMetadata;
            int? promptTokenCount = metadata.PromptTokenCount; // The total number of tokens in the prompt 
            int? candidatesTokenCount = metadata.CandidatesTokenCount; // the total number of tokens in the generated candidates
            int? thoughtsTokenCount = metadata.ThoughtsTokenCount; // output only. The number of tokens that were part of the model's generated "thoughts" output if applicable.  
            int? x = metadata.TotalTokenCount; // promptTokenCount + candidatesTokenCount + thoughtsTokenCount
            int promptCount = metadata.PromptTokensDetails.Count;
            inputTokens = promptTokenCount ?? 0;
            outputTokens = thoughtsTokenCount ?? 0;
            output = response.Text;

            PromptResult result = new PromptResult(success, prompt, output, inputTokens, outputTokens, "gemini-2.5-flash", errorString.ToString());
            result.time = new TimeSpan(0, 0, 0).ToString();
            result.ParsedResult = JsonSerializer.Deserialize<List<AlignmentResult>>(result.result);

            List<PromptResult> finalResult = new List<PromptResult> { result };


            //AddToExamplesDB(finalResult);

            ParseJsonResult(finalResult, "Test");
        }

        private void recoverToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<Dataset> list = RecoveryCode.Recover();
            foreach (Dataset dataset in list)
            {
                ProcessDataset(dataset);
            }
        }

        private void ProcessDataset(Dataset dataset)
        {
            logger.LogInformation($"Processing dataset");
            bool success = true;
            StringBuilder errorString = new StringBuilder();

            string output = "{}"; // return empty JSON on error
            int inputTokens = 0;
            int outputTokens = 0;


            Response response = dataset.response[0];



            //var metadata = response.UsageMetadata;
            //int? promptTokenCount = metadata.PromptTokenCount; // The total number of tokens in the prompt 
            //int? candidatesTokenCount = metadata.CandidatesTokenCount; // the total number of tokens in the generated candidates
            //int? thoughtsTokenCount = metadata.ThoughtsTokenCount; // output only. The number of tokens that were part of the model's generated "thoughts" output if applicable.  
            //int? x = metadata.TotalTokenCount; // promptTokenCount + candidatesTokenCount + thoughtsTokenCount
            //int promptCount = metadata.PromptTokensDetails.Count;
            //inputTokens = promptTokenCount ?? 0;
            //outputTokens = thoughtsTokenCount ?? 0;
            //output = response.candidates[0].content.parts[0].text;

            //PromptResult result = new PromptResult(success, prompt, output, inputTokens, outputTokens, "gemini-2.5-flash", errorString.ToString());
            //result.time = new TimeSpan(0, 0, 0).ToString();
            //result.ParsedResult = JsonSerializer.Deserialize<List<AlignmentResult>>(result.result);

            //List<PromptResult> finalResult = new List<PromptResult> { result };


            ////AddToExamplesDB(finalResult);

            //ParseJsonResult(finalResult, "Test");

        }

        private void recoverXToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<PromptResponse> list = RecoveryCode.RecoverX();
            totalTokens = 0;
            totalOutputTokens = 0;
            totalInputTokens = 0;

            List<PromptResult> finalResult = new List<PromptResult>();
            foreach (PromptResponse dataset in list)
            {
                List<PromptResult> res = ProcessPromptResponse(dataset);
                finalResult.AddRange(res);
            }
            string name = "Recoverd";
            List<TaggedVerse> taggedVerses = ParseJsonResult(finalResult, name);
        
            AddTaggedVerseToBible(taggedVerses);
            SaveTaggedBible();
        }

        SortedDictionary<int, TaggedVerse> taggedVerseDict = new SortedDictionary<int, TaggedVerse>();

        private void SaveTaggedBible()
        {
            StringBuilder sb = new StringBuilder();
            // for each reference index in taggedVerseDict,
            // get the reference string value from the target parser refernce indices dictionary,
            // if the refernce is in dot format (book.chapter.verse), convert it to (book chapter:verse) format
            // abbend a space to the reference string and then the tagged verse text,
            // the tagged verse text by appending the verse's tagged words (ToString()), seperated by spaces
            foreach (var kvp in taggedVerseDict)
            {
                var reference = targetParser.referenceIndices.FirstOrDefault(x => x.Value == kvp.Key).Key;
                if (reference != null)
                {
                    if (reference.Contains("."))
                    {
                        reference = reference.Replace(".", " ");
                        int lastSpaceIndex = reference.LastIndexOf(" ");
                        if (lastSpaceIndex != -1)
                        {
                            reference = reference.Substring(0, lastSpaceIndex) + ":" + reference.Substring(lastSpaceIndex + 1);
                        }
                    }
                    sb.Append(reference);
                    foreach (var taggedWord in kvp.Value.TaggedWords)
                    {
                        sb.Append(" " + taggedWord.ToString());
                    }
                    sb.AppendLine();
                }

            }
            System.IO.File.WriteAllText("TaggedBible.txt", sb.ToString());
        }

        private void AddTaggedVerseToBible(List<TaggedVerse> taggedVerses)
        {
            foreach(TaggedVerse verse in taggedVerses)
            {
                int referenceIndex = verse.ReferenceIndex;
                if(taggedVerseDict.ContainsKey(referenceIndex))
                {
                    taggedVerseDict[referenceIndex]=verse;
                }
                else
                {
                    taggedVerseDict.Add(referenceIndex, verse);
                }
            }
        }

        int totalTokens = 0;
        int totalOutputTokens = 0;
        int totalInputTokens = 0;
        private List<PromptResult> ProcessPromptResponse(PromptResponse dataset)
        {
            logger.LogInformation($"Processing prompt response");

            bool success = true;
            StringBuilder errorString = new StringBuilder();

            string output = "{}"; // return empty JSON on error
            int inputTokens = 0;
            int outputTokens = 0;
            totalTokens = 0;

            string prompt = dataset.prompt.contents[0].parts[0].text;
            List<CombinedVerse>? verses = GetVersesFromPrompt(prompt);

            var metadata = dataset.response.usageMetadata;
            int totalTokenCount = metadata.totalTokenCount; // promptTokenCount + candidatesTokenCount + thoughtsTokenCount
            int? candidatesTokenCount = metadata.candidatesTokenCount; // the total number of tokens in the generated candidates
            int? thoughtsTokenCount = metadata.thoughtsTokenCount; // output only. The number of tokens that were part of the model's generated "thoughts" output if applicable.  
            int promptCount = metadata.promptTokensDetails[0].tokenCount;
            inputTokens = promptCount;
            outputTokens = thoughtsTokenCount ?? 0;
            outputTokens += candidatesTokenCount ?? 0;
            output = dataset.response.candidates[0].content.parts[0].text;
            if(totalTokenCount != inputTokens + outputTokens)
            {
                int x = 0;
            }
            totalTokens += totalTokenCount;
            totalOutputTokens += outputTokens;
            totalInputTokens += inputTokens;
            PromptResult result = new PromptResult(success, prompt, output, inputTokens, outputTokens, "gemini-2.5-flash", errorString.ToString());
            result.time = new TimeSpan(0, 0, 0).ToString();
            result.ParsedResult = JsonSerializer.Deserialize<List<AlignmentResult>>(result.result);

            List<PromptResult> finalResult = new List<PromptResult> { result };

            string name = result.ParsedResult[0].reference.Replace(".","_");


            AddToExamplesDB(finalResult);

            // ParseJsonResult(finalResult, name);
            return finalResult;
        }
    }
}
