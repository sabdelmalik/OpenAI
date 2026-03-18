using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvancedAligner
{
    public class VersionVerse
    {
        public VersionVerse(string reference, string dotNotationReference, string translationText)
        {
            Reference = reference;
            DotNotationReference = dotNotationReference;
            TranslationText = translationText;
            // split the translation text into words and create a VersionToken for each word
                string[] words = translationText.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                int wordIndex = 0;
                foreach (string word in words)
                {
                    VersionToken token = new VersionToken
                    {
                        index = wordIndex,
                        surface = word,
                    };
                    Tokens.Add(token);
                    wordIndex++;
            }
        }

        public string Reference { get; } = string.Empty;    
        public string DotNotationReference { get; } = string.Empty;
        public string TranslationText { get; }
        public List<VersionToken> Tokens { get; set; } = new List<VersionToken>();


    }
}
