// COMPLETE LLM-BASED HEBREW ALIGNMENT PIPELINE
// NOW WITH RAG (Automatic Example Selector)

namespace LLMHebrewAlignment
{
    public class Verse
    {
        public string reference {  get; set; }

        public HebrewVerse hebrew { get; set; }

        public TargetVerse target { get; set; }

    }

    public class TargetVerse
    {
        public string text { get; set; }    
        public List<TargetToken> tokens {  get; set; }
    }

    public class TargetToken
    {
        public int i { get; set; }
        public string word { get; set; }
    }

    public class HebrewVerse
    {
        public string text { get; set; }
        public List<HebrewToken> tokens { get; set; }
    }

    public class HebrewToken
    {
        public int i { get; set; }
        public string surface { get; set; }                // Original text
        public string lemma { get; set; }         // underlying, abstract unit of meaning, typically listed in a lexicon
        public string pos { get; set; }        // verb, noun, prep, conj, pron_suffix, etc.
        public string morph { get; set; }      // human-readable morphology (e.g., "Qal, Perfect, 3rd person, masculine singular")
        public string gloss { get; set; }
    }
}