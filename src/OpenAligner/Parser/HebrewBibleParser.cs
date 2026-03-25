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
        /// the bookCounts dictionary stores the count of verses for each book in the Hebrew Bible.
        /// </summary>
        public readonly Dictionary<string, BookDetails> bookCounts = new Dictionary<string, BookDetails>();

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

            string lastBook = string.Empty;
            int lastChapter = 0;
            int lastVerseNumber = 0;

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

                    int chapter = int.Parse(reference.Split('.')[1]);
                    int verseNumber = int.Parse(reference.Split('.')[2]);

                    referenceIndices[reference] = verseIndex;
                    HebrewBible[verseIndex] = new OtVerse(reference);

                    // update BookCounts
                    if (!bookCounts.ContainsKey(bookName))
                    {
                        // This is a new book,
                        // before adding it to tempBookCounts with an empty list of chapter verses
                        // wee need to fill in the chapter verses for the previous book if it exists
                        if (!string.IsNullOrEmpty(lastBook))
                        {
                            // fill in chapter verses for lastBook
                            BookDetails details = bookCounts[lastBook];
                            // ensure chapter verses list has enough entries for all chapters
                            while (details.ChapterVerses.Count < lastChapter)
                            {
                                details.ChapterVerses.Add(0);
                            }
                            // update verse count for last chapter
                            if (details.ChapterVerses.Count > 0)
                            {
                                details.ChapterVerses[lastChapter - 1] = lastVerseNumber;
                            }
                            lastBook = bookName;
                            lastChapter = 0;
                            lastVerseNumber = 0;
                        }
                        bookCounts[bookName] = new BookDetails
                        {
                            BookName = bookName,
                            ChapterVerses = new List<int>()
                        };
                    }
                    else
                    {
                        // This is an existing book, update chapter verses count
                        BookDetails details = bookCounts[bookName];
                        // ensure chapter verses list has enough entries for all chapters
                        while (details.ChapterVerses.Count < chapter)
                        {
                            details.ChapterVerses.Add(0);
                        }
                        // update verse count for current chapter
                        if (details.ChapterVerses.Count > 0)
                        {
                            details.ChapterVerses[chapter - 1] = Math.Max(details.ChapterVerses[chapter - 1], verseNumber);
                        }
                    }

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
