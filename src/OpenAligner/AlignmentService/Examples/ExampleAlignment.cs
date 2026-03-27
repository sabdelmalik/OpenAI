using OpenAiAPI.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace AdvancedAligner.Examples
{
    public class ExampleAlignment
    {
        public string Reference { get; set; }
        public List<string> HebrewLemmas { get; set; }
        public List<string> POS { get; set; }
        public List<string> MorphPatterns { get; set; }
        public AlignmentResult Alignment { get; set; }
    }
}
