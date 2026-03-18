using System;
using System.Collections.Generic;
using System.Text;

namespace AdvancedHebrewAlignmentEngine.Models
{
    public class ExpandedToken
    {
        public string Text { get; set; }
        public string Lemma { get; set; }
        public string POS { get; set; }
        public string Morph { get; set; }
        public string Context { get; set; }
        public string Gloss { get; set; }
    }

}
