using System;
using System.Collections.Generic;
using System.Text;

namespace AdvancedAligner
{
    public class VerseToken
    {
        public string reference { get; set; }
        public List<TargetToken> target_tokens { get; set; }
        public List<HebrewAramaicToken> hebrew_tokens { get; set; }
    }
}
