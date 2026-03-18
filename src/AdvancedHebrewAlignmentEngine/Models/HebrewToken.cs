using System;
using System.Collections.Generic;
using System.Text;

namespace AdvancedHebrewAlignmentEngine.Models
{
    public class HebrewToken
    {
        public string Surface { get; set; }
        public string Lemma { get; set; }
        public string POS { get; set; }
        public string Morphology { get; set; }
        public string Gloss { get; set; }
        public List<string> Prefixes { get; set; }
    }

}
