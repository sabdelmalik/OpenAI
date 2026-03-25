using System;
using System.Collections.Generic;
using System.Text;

namespace AdvancedAligner
{
    public class VerseTokenCompact
    {
        public string reference { get; set; }
        public List<TargetToken> target_tokens { get; set; }
        public List<HebrewAramaicTokenCompact> hebrew_tokens { get; set; }
    }
}
