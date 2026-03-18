// ADVANCED HEBREW ALIGNMENT ENGINE (RESEARCH-GRADE)
// Includes:
// - Prefix splitting
// - Construct chains
// - Verb subject expansion
// - Phrase alignment
// - Multi-match graph scoring
// - Confidence scoring

using AdvancedHebrewAlignmentEngine;
using AdvancedHebrewAlignmentEngine.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace HebrewAlignmentEngineAdvanced
{
    class Program
    {
        static async Task Main()
        {
            string apiKey = Environment.GetEnvironmentVariable("OPENAI_ALIGNER_KEY", EnvironmentVariableTarget.User);
            var engine = new AlignmentEngineEx(apiKey);

            /*
            
 new HebrewToken { Surface = "בְּ", strong  = "H9003-H7225G", Lemma  = "ב", POS  = "preposition", Morphology  = "inseparable_prep", gloss  = "in"},
 new HebrewToken { Surface = "רֵאשִׁ֖ית", strong  = "H7225G", Lemma  = "רֵאשִׁית", POS  = "noun", Morphology  = "common-fs-abs", gloss  = "beginning"},
 new HebrewToken { Surface = "בָּרָ֣א", strong  = "H1254A", Lemma  = "בָּרָא", POS  = "verb", Morphology  = "qal-perfect(qatal)-3ms", gloss  = "he created"},
 new HebrewToken { Surface = "אֱלֹהִ֑ים", strong  = "H0430G", Lemma  = "אֱלֹהִים", POS  = "noun", Morphology  = "common-mp-abs", gloss  = "God"},
 new HebrewToken { Surface = "אֵ֥ת", strong  = "H0853_A", Lemma  = "אֵת", POS  = "particle", Morphology  = "direct object marker", gloss  = "\u003Cobj.\u003E"},
 new HebrewToken { Surface = "הַ", strong  = "H9009-H8064", Lemma  = "ה", POS  = "particle", Morphology  = "definite article", gloss  = "the"},
 new HebrewToken { Surface = "שָּׁמַ֖יִם", strong  = "H8064", Lemma  = "שָׁמַיִם", POS  = "noun", Morphology  = "common-mp-abs", gloss  = "heavens"},
 new HebrewToken { Surface = "וְ", strong  = "H9002-H0853_B", Lemma  = "ו", POS  = "conjunction", Morphology  = "conjunction", gloss  = "and"},
 new HebrewToken { Surface = "אֵ֥ת", strong  = "H0853_B", Lemma  = "אֵת", POS  = "particle", Morphology  = "direct object marker", gloss  = "\u003Cobj.\u003E"},
 new HebrewToken { Surface = "הָ", strong  = "H9009-H0776G", Lemma  = "ה", POS  = "particle", Morphology  = "definite article", gloss  = "the"},
 new HebrewToken { Surface = "אָֽרֶץ", strong  = "H0776G", Lemma  = "אֶ֫רֶץ", POS  = "noun", Morphology  = "common-fs-abs", gloss  = "earth"}
            */
            var tokens = new List<HebrewToken>
            {
                 new HebrewToken { Surface = "בְּ", Lemma  = "ב", POS  = "preposition", Morphology  = "inseparable_prep", Gloss = "in"},
                 new HebrewToken { Surface = "רֵאשִׁ֖ית", Lemma  = "רֵאשִׁית", POS  = "noun", Morphology  = "common-fs-abs", Gloss = "beginning"},
                 new HebrewToken { Surface = "בָּרָ֣א", Lemma  = "בָּרָא", POS  = "verb", Morphology  = "qal-perfect(qatal)-3ms", Gloss = "he created"},
                 new HebrewToken { Surface = "אֱלֹהִ֑ים", Lemma  = "אֱלֹהִים", POS  = "noun", Morphology  = "common-mp-abs", Gloss = "God"},
                 new HebrewToken { Surface = "אֵ֥ת", Lemma  = "אֵת", POS  = "particle", Morphology  = "direct object marker", Gloss = ""},
                 new HebrewToken { Surface = "הַ", Lemma  = "ה", POS  = "particle", Morphology  = "definite article", Gloss = "the"},
                 new HebrewToken { Surface = "שָּׁמַ֖יִם", Lemma  = "שָׁמַיִם", POS  = "noun", Morphology  = "common-mp-abs", Gloss = "heavens"},
                 new HebrewToken { Surface = "וְ", Lemma  = "ו", POS  = "conjunction", Morphology  = "conjunction", Gloss = "and"},
                 new HebrewToken { Surface = "אֵ֥ת",Lemma  = "אֵת", POS  = "particle", Morphology  = "direct object marker", Gloss = ""},
                 new HebrewToken { Surface = "הָ", Lemma  = "ה", POS  = "particle", Morphology  = "definite article", Gloss = "the"},
                 new HebrewToken { Surface = "אָֽרֶץ", Lemma  = "אֶ֫רֶץ", POS  = "noun", Morphology  = "common-fs-abs", Gloss = "earth"}
            };

            var target = new List<string> { "In", "the", "beginning", "God", "created", "the", "heavens", "and", "the", "earth." };

            string verseReference = "Genesis 1:1";
            string hebrewVerse = "בְּרֵאשִׁ֖ית בָּרָ֣א אֱלֹהִ֑ים אֵ֥ת הַשָּׁמַ֖יִם וְאֵ֥ת הָאָֽרֶץ";
            string targetVerse = "In the beginning God created the heavens and the earth.";

            var results = await engine.AlignAsync(tokens, target, null, verseReference, hebrewVerse, targetVerse);

            Console.OutputEncoding = Encoding.UTF8;
            //foreach (var r in results.Edges)
            //{
            //    Console.WriteLine($"{r.Source} -> {r.Target} ({r.Score:F2})");
            //}

            foreach (var targetWord in target)
            {
                string source = string.Empty;
                double score = 0f;
                // find the best matching source word for this target word
                foreach (var edge in results.Edges.Where(e => e.Target.Contains(targetWord)))// == targetWord))
                {
                    if (edge.Score > score)
                    {
                        source = edge.Source;
                        score = edge.Score;
                    }
                }
                Console.WriteLine($"{targetWord} -> {source} ({score:F2})");
            }
        }
    }
}
