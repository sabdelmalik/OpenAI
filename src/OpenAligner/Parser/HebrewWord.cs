using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TAParser
{
    public class HebrewWord
    {
        // properties are Reference Hebrew  Transliteration Translation dStrongs Grammar Meaning_Variants Spelling_Variants   Root_dStrong_Instance Alternative_Strongs_Instance    Conjoin_word Expanded_Strong-tags

        public HebrewWord(string line)
        {
            var parts = line.Split('\t');

            string reference = parts.Length > 0 ? parts[0] : string.Empty;
            // Reference is the part before the first #
            Reference = reference.Split('#')[0];
            // HebrewReference is the part between parantheses
            int startIndex = Reference.IndexOf('(');
            int endIndex = Reference.IndexOf(')');
            HebrewReference = (startIndex >= 0 && endIndex > startIndex) ? Reference.Substring(startIndex + 1, endIndex - startIndex - 1) : string.Empty;
            if(startIndex >=0)
            {
                Reference = Reference.Substring(0, startIndex);
            }
            Hebrew = parts.Length > 1 ? parts[1] : string.Empty;
            Transliteration = parts.Length > 2 ? parts[2] : string.Empty;
            Translation = parts.Length > 3 ? parts[3] : string.Empty;
            DStrongs = parts.Length > 4 ? parts[4] : string.Empty;
            Grammar = parts.Length > 5 ? parts[5] : string.Empty;
            MeaningVariants = parts.Length > 6 ? parts[6] : string.Empty;
            SpellingVariants = parts.Length > 7 ? parts[7] : string.Empty;
            RootDStrongInstance = parts.Length > 8 ? parts[8] : string.Empty;
            AlternativeStrongsInstance = parts.Length > 9 ? parts[9] : string.Empty;
            ConjoinWord = parts.Length > 10 ? parts[10] : string.Empty;
            ExpandedStrongTags = parts.Length > 11 ? parts[11] : string.Empty;

            // handle the Hebrew word details
            int sofPasuq = Hebrew.IndexOf('׃');
            if (sofPasuq >= 0)
            {
                Hebrew = Hebrew.Substring(0, sofPasuq);
            }
            //remove '/' and '\' if present
            Hebrew = Hebrew.Replace("/", "").Replace("\\", "");

            // the main lex is pert of ExpandedStrongTags between curley braces {}
            string mainLex = ExtractMainLex(ExpandedStrongTags);
            if (!string.IsNullOrEmpty(mainLex))
            {
                string[] strings = mainLex.Split('=');
                if (strings.Length > 1)
                {
                    DStrongsNumber = strings[0].Trim();
                    StrongsNumber = DStrongsNumber;
                    if (StrongsNumber.Length > 5)
                    {
                        StrongsNumber = StrongsNumber.Substring(0, 5); // remove the leading letter
                    }
                    LexiconEntry = strings[1].Trim();
                }
            }
            if (!string.IsNullOrEmpty(Grammar))
            {
                if (Grammar.StartsWith("A"))
                {
                    Language = LanguageEnum.Aramaic;
                }
            }
        }

        private string ExtractMainLex(string expandedStrongTags)
        {
            // the main lex is pert of ExpandedStrongTags between curley braces {}
            int startIndex = expandedStrongTags.IndexOf('{');
            int endIndex = expandedStrongTags.IndexOf('}');
            if (startIndex >= 0 && endIndex > startIndex)
            {
                return expandedStrongTags.Substring(startIndex + 1, endIndex - startIndex - 1);
            }
            else
            {
                return string.Empty;
            }
        }

        public string Reference { get; set; }
        public string HebrewReference { get; set; }
        public string Hebrew { get; set; }
        public string Transliteration { get; set; }
        public string Translation { get; set; }
        public string DStrongs { get; set; }
        public string Grammar { get; set; }
        public string MeaningVariants { get; set; }
        public string SpellingVariants { get; set; }
        public string RootDStrongInstance { get; set; }
        public string AlternativeStrongsInstance { get; set; }
        public string ConjoinWord { get; set; }
        public string ExpandedStrongTags { get; set; }
        public string LexiconEntry { get; set; } = string.Empty;
        public string DStrongsNumber { get; set; } = string.Empty;
        public string StrongsNumber { get; set; } = string.Empty;
        public LanguageEnum Language { get; set; } = LanguageEnum.Hebrew;
        override public string ToString()
        {
            return $"{Hebrew} - {LexiconEntry}={StrongsNumber}";
        }
    }

    public enum LanguageEnum
    {
        Hebrew,
        Aramaic
    }
}
