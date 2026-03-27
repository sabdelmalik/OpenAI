using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvancedAligner
{
    public class HebrewAramaicToken
    {
        public int index { get; set; }                     
        public string surface { get; set; }                // Original text
        public string strong { get; set; }        // e.g., H157
        public string lemma { get; set; }         // underlying, abstract unit of meaning, typically listed in a lexicon
        public string pos { get; set; }        // verb, noun, prep, conj, pron_suffix, etc.
        public string morph { get; set; }      // human-readable morphology (e.g., "Qal, Perfect, 3rd person, masculine singular")
        public string gloss { get; set; }
    }

    //public class ClaudeHebrewAramaicTokens
    //{
    //    internal static List<ClaudeHebrewAramaicToken> CreateClaudeTokenFromHebrewWord(HebrewWord hw)
    //    {
    //        List<ClaudeHebrewAramaicToken> resultTokens = new List<ClaudeHebrewAramaicToken>();
    //        // 1. decide if we need to split the Hebrew word into multiple tokens based on the presence of prefixes and suffixes
        
    //        return resultTokens;
    //    }
    //}
}

