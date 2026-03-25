using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAiAPI.Models
{
    public class HebrewToken
    {
        public int i { get; set; }
        public string surface { get; set; }
        public string lemma { get; set; }
        public string pos { get; set; }
        public string morph { get; set; }
        public string gloss { get; set; }
    }
}
