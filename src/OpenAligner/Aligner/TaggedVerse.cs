using System;
using System.Collections.Generic;
using System.Text;

namespace AdvancedAligner
{
    public class TaggedVerse
    {
        public string VerseReference { get; set; }
        public int ReferenceIndex { get; set; }
        public List<TaggedWord> TaggedWords { get; set; }
    }

    public class TaggedWord
    {
        public string Text { get; set; }
        public List<string> Strongs { get; set; } 
        public List<string> Morphology { get; set; }

        override public string ToString()
        {
            string tag = string.Empty;
            for (int i = 0; i < Strongs.Count; i++)
            {
                string strong = Strongs[i];
                // if strong contains a dash, we take only the part before the dash, e.g., G1234-5678 becomes G1234.
                string strongWithoutDash = strong.Split('-')[0];
                //tag += $"<{Strongs[i]}:{Morphology[i]}> ";
                tag += $"<{strongWithoutDash}> ";
            }
            if (tag == string.Empty)
                tag = "<>";
            return $"{Text} {tag}".Trim();
        }
    }
}
