using System;
using System.Collections.Generic;
using System.Text;

namespace AdvancedHebrewAlignmentEngine.Models
{
    public class Phrase
    {
        public string Text { get; set; }
        public string Lemma { get; set; }
        public string POS { get; set; }
        public string Morph { get; set; }
        public string Context { get; set; }
        public string Gloss { get; set; }

        public Phrase(ExpandedToken t)
        {
            Text = t.Text;
            Lemma = t.Lemma;
            POS = t.POS;
            Morph = t.Morph;
            Context = t.Context;
            Gloss = t.Gloss;
        }

        public Phrase(ExpandedToken a, ExpandedToken b)
        {
            Text = a.Text + " " + b.Text;
            Lemma = a.Lemma + " " + b.Lemma;
            POS = a.POS + " " + b.POS;
            Morph = a.Morph + "+" + b.Morph;
            Context = a.Context + " + " + b.Context;
            Gloss = a.Gloss + " " + b.Gloss;
        }

        public Phrase(ExpandedToken a, ExpandedToken b, ExpandedToken c)
        {
            Text = a.Text + " " + b.Text + " " + c.Text;
            Lemma = a.Lemma + " " + b.Lemma + " " + c.Lemma;
            POS = a.POS + " " + b.POS + " " + c.POS;    
            Morph = a.Morph + "+" + b.Morph + "+" + c.Morph;
            Context = a.Context + " + " + b.Context + " + " + c.Context;
            Gloss = a.Gloss + " " + b.Gloss + " " + c.Gloss;
        }
    }

}
