namespace AdvancedAligner
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using System.Text.RegularExpressions;

    public class Program
    {


        // ------------------------------------------------------------
        // Hebrew normalization utilities
        // ------------------------------------------------------------

        // Cantillation marks: U+0591–U+05AF
        static readonly Regex CantillationRegex = new Regex("[\u0591-\u05AF]", RegexOptions.Compiled);

        // Niqqud: U+05B0–U+05BD, U+05BF, U+05C1–U+05C2, U+05C4–U+05C5, U+05C7
        static readonly Regex NiqqudRegex = new Regex("[\u05B0-\u05BD\u05BF\u05C1-\u05C2\u05C4-\u05C5\u05C7]", RegexOptions.Compiled);

        static string NormalizeHebrew(string input, int mode)
        {
            if (mode == 1)
                return input; // Keep everything

            string result = input;

            if (mode >= 2)
                result = CantillationRegex.Replace(result, ""); // Remove cantillation

            if (mode == 3)
                result = NiqqudRegex.Replace(result, ""); // Remove niqqud, keep final forms

            return result;
        }

        // ------------------------------------------------------------
        // Main program
        // ------------------------------------------------------------
        public static async Task Main(string[] args)
        {
            
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;

            if (args.Length < 1)
            {
                Console.WriteLine("Please provide the version to align file path as a command line argument.");
                return;
            }

            string versionPath = args[0];
            if (!File.Exists(versionPath))
            {
                Console.WriteLine($"The file path '{versionPath}' does not exist.");
                return;
            }
            try
            {
                string key = Environment.GetEnvironmentVariable("CLAUDE_API_KEY");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error retrieving CLAUDE_API_KEY from environment variables: " + ex.Message);
                return;
            }

            HebrewBibleParser hebrewBibleParser = new HebrewBibleParser();
            VersionParser versionParser = new VersionParser(versionPath, hebrewBibleParser.HebrewBooks);

            if(versionParser.VersionBooks.Count != hebrewBibleParser.HebrewBooks.Count)
            {
                Console.WriteLine("Book count mismatch between input version and hebrew version");
                return;
            }

            if (versionParser.referenceIndices.Count != hebrewBibleParser.referenceIndices.Count)
            {
                Console.WriteLine("Verse count mismatch between input version and hebrew version");
                FindFirstMismatch(hebrewBibleParser, versionParser);
                return;
            }

            UserPrompt userPrompt = new UserPrompt(hebrewBibleParser, versionParser);
            //userPrompt.BuildPrompt("Pro.9.8", "Pro.9.8");
            // Psa.10.9, Psa.17.14, Psa.18.6, Pro.4.22, Pro.6.27
            userPrompt.BuildPrompt(new List<string>() { "Psa.10.9", "Psa.17.14" });

            int mode = 1;
//            int.TryParse(Console.ReadLine(), out mode);
//            if (mode < 1 || mode > 3) mode = 1;

            // אַל תּ֣וֹכַח לֵ֭ץ פֶּן יִשְׂנָאֶ֑/ךָּ הוֹכַ֥ח לְ֝/חָכָ֗ם וְ/יֶאֱהָבֶֽ/ךָּ
            Console.WriteLine("\nEnter complete Hebrew verse (with / separators):");
            string hebrewInput = Console.ReadLine();
            // reverse the input characters order because it is supposed to be right to left
            // this is just a quick fix for the console input, the AI model will receive the correct order of characters in the JSON input
//            hebrewInput = new string(hebrewInput.Reverse().ToArray());

            // Do not rebuke mockers or they will hate you; rebuke the wise and they will love you.
            Console.WriteLine("\nEnter complete English verse:");
            string englishInput = Console.ReadLine();

            // ------------------------------------------------------------
            // Hebrew tokenization: split on whitespace, then on slashes
            // ------------------------------------------------------------
            var hebrewWords = hebrewInput
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .ToList();

            var hebrewTokens = new List<HebrewAramaicToken>();
            int hIndex = 1;

            //foreach (var word in hebrewWords)
            //{
            //    var parts = word.Split('/', StringSplitOptions.RemoveEmptyEntries);

            //    foreach (var part in parts)
            //    {
            //        string trimmed = part.Trim();
            //        if (trimmed.Length == 0) continue;

            //        hebrewTokens.Add(new ClaudeHebrewAramaicToken
            //        {
            //            Id = $"H{hIndex}",
            //            Surface = trimmed,
            //            Normalized = NormalizeHebrew(trimmed, mode),
            //            PositionInVerse = hIndex
            //        });

            //        hIndex++;
            //    }
            //}

            // ------------------------------------------------------------
            // English tokenization
            // ------------------------------------------------------------
            var englishWords = englishInput
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .ToList();

            var englishTokens = englishWords
                .Select((w, i) => new VersionToken
                {
                    index = i + 1,
                    surface = w.Trim(),
                })
                .ToList();

            // ------------------------------------------------------------
            // Alignment placeholder (AI model will fill this later)
            // ------------------------------------------------------------
            var alignments = new List<AlignmentLink>();

            // ------------------------------------------------------------
            // Build result
            // ------------------------------------------------------------
            var result = new AlignmentResult
            {
                HebrewTokens = hebrewTokens,
                EnglishTokens = englishTokens,
                Alignments = alignments
            };

            // ------------------------------------------------------------
            // Output JSON
            // ------------------------------------------------------------
            var json = JsonSerializer.Serialize(result, new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });

            Console.WriteLine("\nGenerated JSON:\n");
            Console.WriteLine(json);
            File.WriteAllText("alignment_result.json", json);
        }

        private static void FindFirstMismatch(HebrewBibleParser hebrewBibleParser, VersionParser versionParser)
        {
            // versionParser.referenceIndices.Count != hebrewBibleParser.referenceIndices.Count
            // find the first verse reference that is in one but not the other
            for (int index = 0;  index < versionParser.referenceIndices.Keys.Count; index++)
            {
                string versionReference = versionParser.referenceIndices.Keys.ElementAt(index);
            //    convert verse reference to hebrew format(e.g.Genesis 1:1->Bereshit 1:1) using the hebrewBibleParser.HebrewBooks list
            //    string bookName = reference.Split(' ')[0];
            //string chapter = reference.Split(' ')[1].Split(':')[0];
            //string verse = reference.Split(':')[1];
            //bookName = hebrewBibleParser.HebrewBooks[Array.IndexOf(versionParser.VersionBooks.ToArray(), bookName)];

            //string versionReference = $"{bookName}.{chapter}.{verse}";
            string hebrewReference = hebrewBibleParser.referenceIndices.Keys.ElementAt(index);
                if (versionReference != hebrewReference)
                {
                    Console.WriteLine($"At index {index}, version Reference =  {versionReference}, hebrew  reference = {hebrewReference}");
                    return;
                }
            }
        }
    }
}
