

using System.Text;

namespace TAParser
{
    public class TahotParser
    {
        public SortedDictionary<int, TahotVerse> HebrewBible { get; } = new SortedDictionary<int, TahotVerse>();
        public Dictionary<string, int> referenceIndices = new Dictionary<string, int>();
        private SortedDictionary<int, TahotVerse> HebrewBibleTemp { get; } = new SortedDictionary<int, TahotVerse>();
        private Dictionary<string, int> referenceIndicesTemp = new Dictionary<string, int>();
        public List<string> TahotBooks { get; } = new List<string>();

        private Dictionary<string, string> pslmsTitles = new Dictionary<string, string>();
        private List<int> psalmsWithTitles = new List<int>();

        private bool mergePsalmsTitles = true;
        public TahotParser() 
        {
            ParseTAHOT();
        }
        public bool ParseTAHOT()
        {
            // 1. Get all files under TA\TAHOT directory
            string[] fileNames = Directory.GetFiles("TA\\TAHOT", "*.*", SearchOption.AllDirectories);
            // ensure they are in alphabitical order
            List<string> files = new List<string>();
            files.AddRange(fileNames);
            files.Sort();
            foreach (string file in files)
            {
                Parse(file);
            }
            //StringBuilder sb = new StringBuilder();
            //foreach (var kvp in pslmsTitles)
            //{
            //    int lastDot = kvp.Key.LastIndexOf('.');
            //    string verseRef = kvp.Key.Substring(0, lastDot);
            //    sb.AppendLine($"{verseRef}");
            //}
            //File.WriteAllText(@"D:\Documents\MyProjects\Claude\Test\bin\Debug\PsalmsTitles.txt", sb.ToString());

            // copy referenceIndicesTemp to referenceIndices and HebrewBibleTemp to HebrewBible
            // merging psalms titles if required
            int indexOffset = 0;
            TahotVerse titleVerse = null;
            foreach (var kvp in referenceIndicesTemp)
            {
                string keyReference = kvp.Key;
                int index = kvp.Value;
                TahotVerse verse = HebrewBibleTemp[index];
                // the reference has the format book.chapter.verseNumber
                string book = keyReference.Split('.')[0];
                int chapter = int.Parse(keyReference.Split('.')[1]);
                int verseNumber = int.Parse(keyReference.Split(".")[2]);
                if(mergePsalmsTitles && book=="Psa" && psalmsWithTitles.Contains(chapter))
                {
                    // if verse number = 0, it is a title
                    if (verseNumber == 0)
                     {
                        titleVerse = verse;
                        indexOffset++;
                        continue;
                    }
                    if (titleVerse != null)
                    {
                        verse.MergeTitle(titleVerse);
                        titleVerse = null;
                    }
                    //verse.DecrementVerseNumber();
                }
                string reference = verse.Reference;
                referenceIndices[reference] = index - indexOffset;
                HebrewBible[index - indexOffset] = verse;

            }

            return true;
        }

        int currentLineIndex = -1;
        int referenceIndex = -1;
        private void Parse(string file)
        {
            string[]? lines = File.ReadAllLines(file);
            currentLineIndex = 0;
            SkipToHeaderLine(lines);
            ProcessVerses(lines);
        }

        private void ProcessVerses(string[]? lines)
        {
            if (lines == null || lines.Length == 0)
            {
                return;
            }
            // skip header line
            currentLineIndex++;
            while (currentLineIndex < lines.Length)
            {
                string line = lines[currentLineIndex];
                if (string.IsNullOrWhiteSpace(line))
                {
                    // end of verse
                    currentLineIndex++;
                    SkipToHeaderLine(lines);
                    currentLineIndex++;
                    continue;
                }

                HebrewWord hw = new HebrewWord(line);
                // book name is the text before the first dot in hw.Reference
                string bookName = hw.Reference.Split('.')[0];
                if (!referenceIndicesTemp.ContainsKey(hw.Reference))
                {
                    referenceIndex++;
                    HebrewBibleTemp[referenceIndex] = new TahotVerse(hw.Reference);
                    referenceIndicesTemp[hw.Reference] = referenceIndex;
                    if (!TahotBooks.Contains(bookName))
                    {
                        TahotBooks.Add(bookName);
                    }
                }
                HebrewBibleTemp[referenceIndex].Words.Add(hw);
                currentLineIndex++;

                // Handle Psalms Titles
                if (bookName == "Psa" && hw.Reference.EndsWith(".0") && hw.HebrewReference != string.Empty)
                {
                    if (!pslmsTitles.ContainsKey(hw.Reference))
                        pslmsTitles[hw.Reference] = hw.HebrewReference;

                    int ch = int.Parse(hw.Reference.Split('.')[1]);
                    if(!psalmsWithTitles.Contains(ch))
                        psalmsWithTitles.Add(ch);

                }
            }
        }

        private void SkipToHeaderLine(string[] lines)
        {
            // skip untile we find the first header: a line that starts with "Eng (Heb)"
            while (currentLineIndex < lines.Length)
            {
                string line = lines[currentLineIndex];
                if (line.StartsWith("Eng (Heb)"))
                {
                    return;
                }
                currentLineIndex++;
            }
        }
    }
}
