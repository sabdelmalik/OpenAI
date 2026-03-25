using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAiAPI.Models
{
    public class AlignmentResult
    {
        public string reference { get; set; }
        public List<AlignmentPair> alignments { get; set; }
    }
}
