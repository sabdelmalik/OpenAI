using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAiAPI.Models
{
    public class HebrewVerse
    {
        public string text { get; set; }
        public List<HebrewToken> tokens { get; set; }
    }
}
