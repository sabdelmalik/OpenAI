using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TAParser
{
    public class TahotVerse
    {
        public string Reference { get; private set; }

        public string Book {  get;}
        public int Chapter {  get; }
        public int VerseNumber { get; private set; }
        public bool IsPsalmTitle { get;}
        public List<HebrewWord> Words { get; set; }
        
        /// <summary>
        /// the reference is in the format book.chapter.verseNumber
        /// </summary>
        /// <param name="reference"></param>
        public TahotVerse(string reference)
        {
            Reference = reference;
            Book = reference.Split('.')[0];
            Chapter = int.Parse(reference.Split(".")[1]);
            VerseNumber = int.Parse(reference.Split('.')[2]);

            IsPsalmTitle = (Book == "Psa" && VerseNumber == 0);

            Words = new List<HebrewWord>();
        }

        public void MergeTitle(TahotVerse titleVerse)
        {
            List<HebrewWord> newWords = new List<HebrewWord>();
            newWords.AddRange(titleVerse.Words);
            newWords.AddRange(this.Words);
            this.Words = newWords;

            //VerseNumber--;
            //Reference = $"{Book}.{Chapter}.{VerseNumber}"
        }

        public string VerseText
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                foreach (var word in Words)
                {
                    sb.Append(word.Hebrew);
                    sb.Append(' ');
                }
                return sb.ToString().Trim();
            }
        }

        public string VerseStrongs
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                foreach (var word in Words)
                {
                    if (!string.IsNullOrEmpty(word.StrongsNumber))
                    {
                        sb.Append(word.StrongsNumber);
                        sb.Append(' ');
                    }
                    else
                    {
                        sb.Append("----- ");
                    }
                }
                return sb.ToString().Trim();
            }
        }

        public Dictionary<string, string> WordToStrongsList
        {
            get
            {
                Dictionary<string, string> dict = new Dictionary<string, string>();
                foreach (var word in Words)
                {
                    if (!string.IsNullOrEmpty(word.StrongsNumber))
                    {
                        if(!dict.ContainsKey(word.Hebrew))
                            dict[word.Hebrew] = word.StrongsNumber;
                    }
                }
                return dict;
            }
        }

        override public string ToString()
        {
            return $"{Reference}: {VerseText} [{VerseStrongs}]";
        }

    }
}
