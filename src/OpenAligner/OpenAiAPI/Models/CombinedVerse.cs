using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAiAPI.Models
{
    public class CombinedVerse
    {
        public string reference { get; set; }
        public HebrewVerse hebrew { get; set; }
        public TargetVerse target { get; set; }
    }
}
