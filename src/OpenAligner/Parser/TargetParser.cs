using System.Windows.Forms;

namespace AdvancedAligner
{
    public class TargetParser
    {
        public SortedDictionary<int, ParserTargetVerse> TargetBible { get; } = new SortedDictionary<int, ParserTargetVerse>();

        public Dictionary<string, int> referenceIndices = new Dictionary<string, int>();

        public readonly Dictionary<string, BookDetails> bookCounts = new Dictionary<string, BookDetails>();


        bool splitPsalmsTitles = false;
        
        int referenceIndex = 0;

        public List<string> TargetBooks { get; } = new List<string>();
        public TargetParser(HebrewBibleParser hebrewBibleParser)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "text files (*.txt)|*.txt|All files (*.*)|*.*";
            openFileDialog1.Title = "Select the Target text";
            DialogResult result = openFileDialog1.ShowDialog();
            while (result != DialogResult.OK)
            {
                if (result == DialogResult.Cancel)
                {
                    throw new Exception("Cannot proceed without a Target Bible");
                }
                result = openFileDialog1.ShowDialog();
            }
            string versionFilePath = openFileDialog1.FileName;



            ParseTargetFile(versionFilePath, hebrewBibleParser.HebrewBooks);
        }

        Dictionary<string, string> psalmsTitles = new Dictionary<string, string>()
        {
{"Psa.3.0", "A psalm of David. When he fled from his son Absalom."},
{"Psa.4.0", "For the director of music. With stringed instruments. A psalm of David."},
{"Psa.5.0", "For the director of music. For pipes. A psalm of David."},
{"Psa.6.0", "For the director of music. With stringed instruments. According to sheminith. A psalm of David."},
{"Psa.7.0", "A shiggaion of David, which he sang to the Lord concerning Cush, a Benjaminite."},
{"Psa.8.0", "For the director of music. According to gittith. A psalm of David."},
{"Psa.9.0", "For the director of music. To the tune of ‘The Death of the Son’. A psalm of David."},
{"Psa.11.0", "For the director of music. Of David."},
{"Psa.12.0", "For the director of music. According to sheminith. A psalm of David."},
{"Psa.13.0", "For the director of music. A psalm of David."},
{"Psa.14.0", "For the director of music. Of David."},
{"Psa.15.0", "A psalm of David."},
{"Psa.16.0", "A miktam of David."},
{"Psa.17.0", "A prayer of David."},
{"Psa.18.0", "For the director of music. Of David the servant of the Lord. He sang to the Lord the words of this song when the Lord delivered him from the hand of all his enemies and from the hand of Saul. He said:"},
{"Psa.19.0", "For the director of music. A psalm of David."},
{"Psa.20.0", "For the director of music. A psalm of David."},
{"Psa.21.0", "For the director of music. A psalm of David."},
{"Psa.22.0", "For the director of music. To the tune of ‘The Doe of the Morning’. A psalm of David."},
{"Psa.23.0", "A psalm of David."},
{"Psa.24.0", "Of David. A psalm."},
{"Psa.25.0", "Of David."},
{"Psa.26.0", "Of David."},
{"Psa.27.0", "Of David."},
{"Psa.28.0", "Of David."},
{"Psa.29.0", "A psalm of David."},
{"Psa.30.0", "A psalm. A song. For the dedication of the temple. Of David."},
{"Psa.31.0", "For the director of music. A psalm of David."},
{"Psa.32.0", "Of David. A maskil."},
{"Psa.34.0", "Of David. When he pretended to be insane before Abimelek, who drove him away, and he left."},
{"Psa.35.0", "Of David."},
{"Psa.36.0", "For the director of music. Of David the servant of the Lord."},
{"Psa.37.0", "Of David."},
{"Psa.38.0", "A psalm of David. A petition."},
{"Psa.39.0", "For the director of music. For Jeduthun. A psalm of David."},
{"Psa.40.0", "For the director of music. Of David. A psalm."},
{"Psa.41.0", "For the director of music. A psalm of David."},
{"Psa.42.0", "For the director of music. A maskil of the Sons of Korah."},
{"Psa.44.0", "For the director of music. Of the Sons of Korah. A maskil."},
{"Psa.45.0", "For the director of music. To the tune of ‘Lilies’. Of the Sons of Korah. A maskil. A wedding song."},
{"Psa.46.0", "For the director of music. Of the Sons of Korah. According to alamoth. A song."},
{"Psa.47.0", "For the director of music. Of the Sons of Korah. A psalm."},
{"Psa.48.0", "A song. A psalm of the Sons of Korah."},
{"Psa.49.0", "For the director of music. Of the Sons of Korah. A psalm."},
{"Psa.50.0", "A psalm of Asaph."},
{"Psa.51.0", "For the director of music. A psalm of David. When the prophet Nathan came to him after David had committed adultery with Bathsheba."},
{"Psa.52.0", "For the director of music. A maskil of David. When Doeg the Edomite had gone to Saul and told him: ‘David has gone to the house of Ahimelek.’"},
{"Psa.53.0", "For the director of music. According to mahalath. A maskil of David."},
{"Psa.54.0", "For the director of music. With stringed instruments. A maskil of David. When the Ziphites had gone to Saul and said, ‘Is not David hiding among us?’"},
{"Psa.55.0", "For the director of music. With stringed instruments. A maskil of David."},
{"Psa.56.0", "For the director of music. To the tune of ‘A Dove on Distant Oaks’. Of David. A miktam. When the Philistines had seized him in Gath."},
{"Psa.57.0", "For the director of music. To the tune of ‘Do Not Destroy’. Of David. A miktam. When he had fled from Saul into the cave."},
{"Psa.58.0", "For the director of music. To the tune of ‘Do Not Destroy’. Of David. A miktam."},
{"Psa.59.0", "For the director of music. To the tune of ‘Do Not Destroy’. Of David. A miktam. When Saul had sent men to watch David’s house in order to kill him."},
{"Psa.60.0", "For the director of music. To the tune of ‘The Lily of the Covenant’. A miktam of David. For teaching. When he fought Aram Naharaim and Aram Zobah, and when Joab returned and struck down twelve thousand Edomites in the Valley of Salt."},
{"Psa.61.0", "For the director of music. With stringed instruments. Of David."},
{"Psa.62.0", "For the director of music. For Jeduthun. A psalm of David."},
{"Psa.63.0", "A psalm of David. When he was in the Desert of Judah."},
{"Psa.64.0", "For the director of music. A psalm of David."},
{"Psa.65.0", "For the director of music. A psalm of David. A song."},
{"Psa.66.0", "For the director of music. A song. A psalm."},
{"Psa.67.0", "For the director of music. With stringed instruments. A psalm. A song."},
{"Psa.68.0", "For the director of music. Of David. A psalm. A song."},
{"Psa.69.0", "For the director of music. To the tune of ‘Lilies’. Of David."},
{"Psa.70.0", "For the director of music. Of David. A petition."},
{"Psa.72.0", "Of Solomon."},
{"Psa.73.0", "A psalm of Asaph."},
{"Psa.74.0", "A maskil of Asaph."},
{"Psa.75.0", "For the director of music. To the tune of ‘Do Not Destroy’. A psalm of Asaph. A song."},
{"Psa.76.0", "For the director of music. With stringed instruments. A psalm of Asaph. A song."},
{"Psa.77.0", "For the director of music. For Jeduthun. Of Asaph. A psalm."},
{"Psa.78.0", "A maskil of Asaph."},
{"Psa.79.0", "A psalm of Asaph."},
{"Psa.80.0", "For the director of music. To the tune of ‘The Lilies of the Covenant’. Of Asaph. A psalm."},
{"Psa.81.0", "For the director of music. According to gittith. Of Asaph."},
{"Psa.82.0", "A psalm of Asaph."},
{"Psa.83.0", "A song. A psalm of Asaph."},
{"Psa.84.0", "For the director of music. According to gittith. Of the Sons of Korah. A psalm."},
{"Psa.85.0", "For the director of music. Of the Sons of Korah. A psalm."},
{"Psa.86.0", "A prayer of David."},
{"Psa.87.0", "Of the Sons of Korah. A psalm. A song."},
{"Psa.88.0", "A song. A psalm of the Sons of Korah. For the director of music. According to mahalath leannoth. A maskil of Heman the Ezrahite."},
{"Psa.89.0", "A maskil of Ethan the Ezrahite."},
{"Psa.90.0", "A prayer of Moses the man of God."},
{"Psa.92.0", "A psalm. A song. For the Sabbath day."},
{"Psa.98.0", "A psalm."},
{"Psa.100.0", "A psalm. For giving grateful praise."},
{"Psa.101.0", "Of David. A psalm."},
{"Psa.102.0", "A prayer of an afflicted person who has grown weak and pours out a lament before the Lord."},
{"Psa.103.0", "Of David."},
{"Psa.108.0", "A song. A psalm of David."},
{"Psa.109.0", "For the director of music. Of David. A psalm."},
{"Psa.110.0", "Of David. A psalm."},
{"Psa.120.0", "A song of ascents."},
{"Psa.121.0", "A song of ascents."},
{"Psa.122.0", "A song of ascents. Of David."},
{"Psa.123.0", "A song of ascents."},
{"Psa.124.0", "A song of ascents. Of David."},
{"Psa.125.0", "A song of ascents."},
{"Psa.126.0", "A song of ascents."},
{"Psa.127.0", "A song of ascents. Of Solomon."},
{"Psa.128.0", "A song of ascents."},
{"Psa.129.0", "A song of ascents."},
{"Psa.130.0", "A song of ascents."},
{"Psa.131.0", "A song of ascents. Of David."},
{"Psa.132.0", "A song of ascents."},
{"Psa.133.0", "A song of ascents. Of David."},
{"Psa.134.0", "A song of ascents."},
{"Psa.138.0", "Of David."},
{"Psa.139.0", "For the director of music. Of David. A psalm."},
{"Psa.140.0", "For the director of music. A psalm of David."},
{"Psa.141.0", "A psalm of David."},
{"Psa.142.0", "A maskil of David. When he was in the cave. A prayer."},
{"Psa.143.0", "A psalm of David."},
{"Psa.144.0", "Of David."},
{"Psa.145.0", "A psalm of praise. Of David."},
        };

        private void ParseTargetFile(string versionFilePath, List<string> tahotBooks)
        {
            string[] lines = File.ReadAllLines(versionFilePath);
            Dictionary<string, BookDetails> tempBookCounts = new Dictionary<string, BookDetails>();

            SortedDictionary<int, ParserTargetVerse> tempBible = new SortedDictionary<int, ParserTargetVerse>();
            Dictionary<string, int> tempIndices = new Dictionary<string, int>();

            string lastBook = string.Empty;
            int lastChapter = 0;
            int lastVerseNumber = 0;

            // The version file is structured as:
            // Reference<space>TranslationText
            // The reference format is Book Chapter:Verse
            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }
                int firstSpaceIndex = line.IndexOf(' ');
                if (firstSpaceIndex < 0)
                {
                    continue; // invalid line
                }
                int secondSpaceIndex = line.IndexOf(' ', firstSpaceIndex + 1);
                if (secondSpaceIndex < 0)
                {
                    continue; // invalid line
                }
                string bookName = line.Substring(0, firstSpaceIndex).Trim();
                string reference = line.Substring(0, secondSpaceIndex).Trim();
                string chapterVerse = reference.Substring(firstSpaceIndex + 1, secondSpaceIndex - firstSpaceIndex - 1).Trim();
                string chapterS = "1";
                string verseNumberS = chapterVerse;
                if (chapterVerse.Contains(':'))
                {
                    chapterS = chapterVerse.Split(':')[0].Trim();
                    verseNumberS =chapterVerse.Split(':')[1].Trim();
                }
                int chapter = int.Parse(chapterS);
                int verseNumber = int.Parse(verseNumberS);

                string translationText = line.Substring(secondSpaceIndex + 1).Trim();
                ParserTargetVerse verse = new ParserTargetVerse(reference, string.Empty, translationText);

                tempIndices[reference] = referenceIndex;
                tempBible[referenceIndex] = verse;
                if (!TargetBooks.Contains(bookName))
                {
                    TargetBooks.Add(bookName);
                }

                // update tempBookCounts
                if (!tempBookCounts.ContainsKey(bookName))
                {
                    // This is a new book,
                    // before adding it to tempBookCounts with an empty list of chapter verses
                    // wee need to fill in the chapter verses for the previous book if it exists
                    if (!string.IsNullOrEmpty(lastBook))
                    {
                        // fill in chapter verses for lastBook
                        BookDetails details = tempBookCounts[lastBook];
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
                    tempBookCounts[bookName] = new BookDetails
                    {
                        BookName = bookName,
                        ChapterVerses = new List<int>()
                    };
                }
                else
                {
                    // This is an existing book, update chapter verses count
                    BookDetails details = tempBookCounts[bookName];
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
                referenceIndex++;
            }

            // replace book names in BookDetails keys with the corresponding names in tahotBooks
            foreach (var kvp in tempBookCounts)
            {
                string versionBookName = kvp.Key;
                BookDetails details = kvp.Value;
                int versionBookIndex = TargetBooks.IndexOf(versionBookName);
                if (versionBookIndex < 0 || versionBookIndex >= TargetBooks.Count)
                {
                    continue; // book not found
                }
                string tahotBookName = tahotBooks[versionBookIndex];
                bookCounts[tahotBookName] = details;
            }

            // copy tempBible to TargetBible after translating reference to dot notation
            int updatedIndex = 0;
            for (referenceIndex = 0; referenceIndex < tempBible.Count; referenceIndex++)
            {
                ParserTargetVerse verse = tempBible[referenceIndex];
                string reference = verse.Reference;
                // translate reference from Book Chapter:Verse to Book.Chapter.Verse
                int firstSpaceIndex = reference.IndexOf(' ');
                if (firstSpaceIndex < 0)
                {
                    continue; // invalid reference
                }
                string versionBookName = reference.Substring(0, firstSpaceIndex).Trim();
                // translate bookName to match tahotBooks
                int versionBookIndex = TargetBooks.IndexOf(versionBookName);
                if (versionBookIndex < 0 || versionBookIndex >= TargetBooks.Count)
                {
                    continue; // book not found
                }
                string verseText = verse.TranslationText;
                string bookName = tahotBooks[versionBookIndex];
                string chapterAndVerse = reference.Substring(firstSpaceIndex + 1).Trim();
                string[] ch_vs = chapterAndVerse.Split(':');
                string chapter = (ch_vs.Length == 1) ? "1" : ch_vs[0];
                string verseNumber = (ch_vs.Length == 1) ? ch_vs[0] : ch_vs[1];
                if (splitPsalmsTitles && bookName == "Psa" && verseNumber == "1")
                {
                    // check if psalmsTitles contains the key
                    string psalmKey = $"{bookName}.{chapter}.0";
                    if (psalmsTitles.ContainsKey(psalmKey))
                    {
                        // ensure verse texts starts with the title
                        if (!verse.TranslationText.StartsWith(psalmsTitles[psalmKey]))
                        {
                            throw new Exception($"Psalms chapter {chapter} is missing title in verse 1.");
                        }
                        // add the title as a separate verse with verse number 0
                        string titleReferenceDN = $"{bookName}.{chapter}.0";
                        string titleReference = $"{versionBookName} {chapter}:0";
                        TargetBible[updatedIndex] = new ParserTargetVerse(titleReference, titleReferenceDN, psalmsTitles[psalmKey]);
                        referenceIndices[titleReference] = updatedIndex++;
                        // remove title text from begining of verse
                        verseText = verse.TranslationText.Substring(psalmsTitles[psalmKey].Length).Trim();
                    }
                }
                string versionReference = $"{versionBookName} {chapter}:{verseNumber}";
                string dotNotationReference = $"{bookName}.{chapter}.{verseNumber}";

                TargetBible[updatedIndex] = new ParserTargetVerse(versionReference, dotNotationReference, verseText);
                referenceIndices[dotNotationReference] = updatedIndex++;
            }
        }

        public int GetBookVerseCount(string bookName)
        {
            int count = 0;
            foreach (var kvp in TargetBible)
            {
                string reference = kvp.Value.DotNotationReference;
                if (reference.StartsWith(bookName + "."))
                {
                    count++;
                }
            }
            return count;

        }

    }

}
