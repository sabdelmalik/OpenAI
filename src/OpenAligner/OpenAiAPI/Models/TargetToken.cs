using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAiAPI.Models
{
    /// <summary>
    /// example{ i = 0, surface = "בראשית", lemma = "ראשית", pos = "noun", morph = "construct", gloss = "beginning" },

    /// </summary>
    public class TargetToken
    {
        public int i { get; set; }
        public string word { get; set; }
    }
}
