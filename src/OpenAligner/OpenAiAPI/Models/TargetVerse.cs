using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAiAPI.Models
{
    public class TargetVerse
    {
        public string text { get; set; }
        public List<TargetToken> tokens { get; set; }
    }
}
