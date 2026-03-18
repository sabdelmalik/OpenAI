using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvancedAligner
{
    public class HebrewBibleParser
    {
        /// <summary>
        /// A dictionary to store the mapping of verse references to their corresponding indices in the HebrewBible collection.
        /// </summary>
        public Dictionary<string, int> referenceIndices = new Dictionary<string, int>();

        /// <summary>
        /// A sorted dictionary to store the verses of the Hebrew Bible, where the key is an integer index and the value is an OtVerse object containing the verse reference and its associated tokens.
        /// </summary>
        public SortedDictionary<int, OtVerse> HebrewBible { get; } = new SortedDictionary<int, OtVerse>();

        SortedDictionary<string, string> morphMap = new SortedDictionary<string, string>();
        /// <summary>
        /// A list to store the names of the books of the Hebrew Bible in the order they appear in the input file. 
        /// This is used to translate the book names from the translation being tagged to those of the raw tokens file
        /// </summary>
        public List<string> HebrewBooks { get; } = new List<string>();

        /// <summary>
        /// The input raw tokens file contains all the verses of the OT 
        /// As follows:
        /// A $ sign marks the start of a verse. It is followed by the verse reference in the format Book.Chapter.Verse (Gen.1.1).
        /// following the referencel line, are the verse words
        /// each word is on aline in the format
        /// surface=strongs=lemma=morphology=gloss
        /// a blank line marks the end of the verse
        /// 
        public HebrewBibleParser()
        {
            string source = @"Tags\OT\AI input Tag Hebrew OT-tokenised2.txt";
            string[] lines = System.IO.File.ReadAllLines(source);
            int verseIndex = -1;
            int wordIndex = 0;
            foreach (string line in lines)
            {
                if (line.StartsWith("$"))
                {
                    verseIndex++;
                    wordIndex = 0;
                    string reference = line.Substring(1).Trim();
                    // extract book name
                    string bookName = reference.Split('.')[0];
                    if (!HebrewBooks.Contains(bookName))
                    {
                        HebrewBooks.Add(bookName);
                    }
                    referenceIndices[reference] = verseIndex;
                    HebrewBible[verseIndex] = new OtVerse(reference);
                }
                else if (!string.IsNullOrWhiteSpace(line))
                {
                    string[] parts = line.Split('=');
                    if (parts.Length == 5)
                    {
                        AiRawToken rawToken = new AiRawToken(wordIndex++, parts[0], parts[1], parts[2], parts[3], parts[4]);
                        HebrewBible[verseIndex].AddRawToken(rawToken);
                    }
                }
            }

            // for debugging
            // extract all raw morf and parsed morph
            foreach (var verse in HebrewBible.Values)
            {
                for (int i = 0; i < verse.RawTokens.Count; i++)
                {
                    var rawToken = verse.RawTokens[i];
                    var parsedToken = verse.Tokens[i];
                    string rawMorph = rawToken.morphology;
                    string parsedMorph = $"{parsedToken.pos}: {parsedToken.morph}";
                    if (!morphMap.ContainsKey(rawMorph))
                    {
                        morphMap[rawMorph] = parsedMorph;
                    }
                }
            }
            StringBuilder sb = new StringBuilder();
            foreach (var kvp in morphMap)
            {
                sb.AppendLine($"{kvp.Key}\t{kvp.Value}");
            }
            System.IO.File.WriteAllText(@"ParsedMorph.txt", sb.ToString());



        }
    }
}
