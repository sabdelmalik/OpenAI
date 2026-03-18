using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvancedAligner
{
    public class AiRawToken
    {
        public AiRawToken(int wordIndex, string surface, string strongs, string lemma, string morphology, string gloss)
        {
            this.index = wordIndex;
            this.surface = surface;
            this.strongs = strongs;
            this.lemma = lemma;
            this.morphology = morphology;
            this.gloss = gloss;
        }
        public int index { get; set; }
        public string surface { get; set; }
        public string strongs { get; set; }
        public string lemma { get; set; }
        public string morphology { get; set; }
        public string gloss { get; set; }
    }
}
