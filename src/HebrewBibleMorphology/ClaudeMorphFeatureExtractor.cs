using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvancedAligner
{
    /// <summary>
    /// STEP: https://docs.google.com/document/d/14yun9nZAmWiArRQ-ELedODAEydaEZA-TL8pDZoX1F8g/edit
    /// OpenScripture: https://hb.openscriptures.org/parsing/HebrewMorphologyCodes.html
    /// </summary>
    public static class ClaudeMorphFeatureExtractor
    {
        public static void ApplyFromMorphCode(string morph, HebrewAramaicToken token)
        {
            if (string.IsNullOrWhiteSpace(morph))
                return;

            // the format of morph is L:morphologycode,
            // where L is a single character indicating the language and morphologycode is a string of characters encoding the morphological features of the word
            BibleLanguage lang = morph[0] switch
            {
                'H' => BibleLanguage.Hebrew,
                'A' => BibleLanguage.Aramaic,
                'G' => BibleLanguage.Greek,
                _ => throw new ArgumentException($"Unknown language code '{morph[0]}' in morphology code '{morph}'")
            };
            if (lang == BibleLanguage.Greek)
                return; // Not handling Greek morphology in this implementation
            
            if (morph.Length < 3)
                return; // Invalid morph code

            morph = morph.Substring(2).Trim(); // Remove language code and the colon

            char partOfSpeech = morph[0];

            morph = morph.Substring(1); // Remove part of speech code for further parsing

            switch (partOfSpeech)
            {
                case 'V':   // verb
                    token.pos = "verb";
                    token.morph = ParseVerb(lang, morph);
                    break;
                case 'N':   // noun
                    token.pos = "noun";
                    token.morph = ParseNoun(lang, morph);
                    break;
                case 'A':   // adjective
                    token.pos = "adjective";
                    token.morph = ParseAdjective(morph);
                    break;
                case 'D':   // adverb
                    token.pos = "adverb";
                    token.morph = "adverb";
                    break;
                case 'P':   // pronoun
                    if (morph.StartsWith("Pp"))
                    {
                        token.pos = "pron_suffix";
                        //token.morph = "pronominal";
                    }
                    else
                    {
                        token.pos = "pronoun";
                        //token.morph = "pronoun";
                    }
                    token.morph = ParsePronoun(lang, morph);
                    break;
                case 'R':   // preposition
                    token.pos = "preposition";
                    token.morph = "inseparable_prep";
                    break;
                case 'C':   // conjunction
                case 'c':   // conjunction
                    token.pos = "conjunction";
                    token.morph = "conjunction";
                    break;
                case 'S':   // suffix
                    token.pos = "suffix";
                    token.morph = ParseSuffix(lang, morph);
                    break;
                case 'T':   // particle
                    token.pos = "particle";
                    token.morph = ParseParticle(lang, morph);
                    break;
            }
        }

        /// <summary>
        /// Example : 
        /// A cfsa : cardinal feminine singular abs
        /// A omsa : ordinal masculine singular abs
        /// A afsa : adjective feminine singular abs
        /// </summary>
        /// <param name="lang"></param>
        /// <param name="morph"></param>
        /// <param name="token"></param>
        private static string ParseAdjective(string morph)
        {
            StringBuilder result = new StringBuilder();
            if (morph.Length < 4)
                return result.ToString();

            // Form
            result.Append(morph[0] switch
            {
                'a' => "common-",
                'c' => "num-",
                'g' => "gentilic-",
                'o' => "num-"
            });

            // Gender
            result.Append(morph.Substring(1, 2));

            // State
            result.Append(morph[3] switch
            {
                'a' => "-abs",
                'c' => "-const",
                'd' => "-determined"
            });

            return result.ToString().Trim('-');
        }

        /// <summary>
        /// 0 form
        /// 1 gender
        /// 2 number - may not be present
        /// 3 state  - may not be present
        /// </summary>
        /// <param name="lang"></param>
        /// <param name="morph"></param>
        /// <returns></returns>
        private static string ParseNoun(BibleLanguage lang, string morph)
        {
            StringBuilder result = new StringBuilder();

            if (morph.Length < 2)
                return result.ToString();

            // form
            result.Append(morph[0] switch
            {
                'c' => "common-",
                'g' => "gentilic-",
                'p' => "proper-",
                't' => "title-",
            });

            if (morph.Length > 2)
                result.Append(morph.Substring(1, 2));

            if (morph.Length > 3)
                result.Append(morph[3] switch
                {
                    'a' => "-abs",
                    'c' => "-const",
                    'd' => "-determined"
                });
            return result.ToString().Trim('-');
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lang"></param>
        /// <param name="morph"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private static string ParsePronoun(BibleLanguage lang, string morph)
        {
            StringBuilder result = new StringBuilder();

            if (string.IsNullOrEmpty(morph))
                return result.ToString();

            result.Append(morph[0] switch
            {
                'd' => "demonstrative-",
                'f' => "indefinite-",
                'i' => "interrogative-",
                'p' => "personal-",
                'r' => "relative-"
            });

            if (morph.Length > 3)
                result.Append(morph.Substring(1).Trim());

            return result.ToString().Trim('-');
        }
        private static string ParseSuffix(BibleLanguage lang, string morph)
        {
            StringBuilder result = new StringBuilder();

            if (string.IsNullOrEmpty(morph))
                return result.ToString();

            // form
            result.Append(morph[0] switch
            {
                'd' => "directional he-",
                'h' => "paragogic he-",
                'n' => "paragogic nun-",
                'p' => "pronominal-",
            });

            if (morph.Length > 3)
                result.Append(morph.Substring(1).Trim());

            return result.ToString().Trim('-');
        }

        /// <summary>
        /// Particle forms
        /// a	affirmation
        /// d definite article
        /// e exhortation
        /// i interrogative
        /// j interjection
        /// m demonstrative
        /// n negative
        /// o direct object marker
        /// r relative
        /// </summary>
        /// <param name="lang"></param>
        /// <param name="morph"></param>
        /// <param name="token"></param>
        private static string ParseParticle(BibleLanguage lang, string morph)
        {
            StringBuilder result = new StringBuilder();

            if (string.IsNullOrEmpty(morph))
                return result.ToString();

            char formCode = morph[0];
            if (lang == BibleLanguage.Aramaic && formCode == 'a')
                formCode = 'd'; // In Aramaic, 'a' is used for the definite article, which is 'd' in Hebrew. Adjusting for consistency.

            // form
            result.Append(morph[0] switch
            {
                'a' => "affirmation",
                'd' => "definite article",
                'e' => "exhortation",
                'i' => "interrogative",
                'j' => "interjection",
                'm' => "demonstrative",
                'n' => "negative",
                'o' => "direct object marker",
                'r' => "relative",
                'c' => "conditional"
            });

            return result.ToString();
        }

        /// <summary>
        /// 0 Hebrew Binyan
        ///     q	qal
        ///     N niphal
        ///     p piel
        ///     P pual
        ///     h hiphil
        ///     H hophal
        ///     t hithpael
        ///     o polel
        ///     O polal
        ///     r hithpolel
        ///     m poel
        ///     M poal
        ///     k palel
        ///     K pulal
        ///     Q qal passive
        ///     l   pilpel
        ///     L   polpal
        ///     f   hithpalpel
        ///     D   nithpael
        ///     j   pealal
        ///     i   pilel
        ///     u   hothpaal
        ///     c   tiphil
        ///     v   hishtaphel
        ///     w   nithpalel
        ///     y   nithpoel
        ///     z   hithpoel
        /// 0 Aramaic Binyan
        ///     q peal
        ///     Q peil
        ///     u hithpeel
        ///     p pael
        ///     P ithpaal
        ///     M hithpaal
        ///     a aphel
        ///     h haphel
        ///     s saphel
        ///     e shaphel
        ///     H hophal
        ///     i ithpeel
        ///     t hishtaphel
        ///     v ishtaphel
        ///     w hithaphel
        ///     o polel
        ///     z ithpoel
        ///     r hithpolel
        ///     f hithpalpel
        ///     b hephal
        ///     c tiphel
        ///     m poel
        ///     l palpel
        ///     L ithpalpel
        ///     O ithpolel
        ///     G ittaphal 
        /// 1 form
        ///     p perfect(qatal)
        ///     q sequential perfect(weqatal)
        ///     i imperfect(yiqtol)
        ///     w sequential imperfect(wayyiqtol)
        ///     h cohortative
        ///     j jussive
        ///     v imperative
        ///     r participle active
        ///     s participle passive
        ///     a infinitive abs
        ///     c infinitive const
        /// 2 person
        /// 3 gender
        /// 4 number
        /// </summary>
        /// <param name="lang"></param>
        /// <param name="morph"></param>
        /// <returns></returns>
        private static string ParseVerb(BibleLanguage lang, string morph)
        {
            StringBuilder result = new StringBuilder();

            if (string.IsNullOrEmpty(morph))
                return result.ToString();

            // Binyan
            if (lang == BibleLanguage.Hebrew)
            {
                result.Append(morph[0] switch
                {
                    'q' => "qal",
                    'N' => "niphal",
                    'p' => "piel",
                    'P' => "pual",
                    'h' => "hiphil",
                    'H' => "hophal",
                    't' => "hithpael",
                    'o' => "polel",
                    'O' => "polal",
                    'r' => "hithpolel",
                    'm' => "poel",
                    'M' => "poal",
                    'k' => "palel",
                    'K' => "pulal",
                    'Q' => "qal passive",
                    'l' => "pilpel",
                    'L' => "polpal",
                    'f' => "hithpalpel",
                    'D' => "nithpael",
                    'j' => "pealal",
                    'i' => "pilel",
                    'u' => "hothpaal",
                    'c' => "tiphil",
                    'v' => "hishtaphel",
                    'w' => "nithpalel",
                    'y' => "nithpoel",
                    'z' => "hithpoel"
                });
            }
            else if (lang == BibleLanguage.Aramaic)
            {
                result.Append(morph[0] switch
                {
                    'q' => "peal",
                    'Q' => "peil",
                    'u' => "hithpeel",
                    'p' => "pael",
                    'P' => "ithpaal",
                    'M' => "hithpaal",
                    'a' => "aphel",
                    'h' => "haphel",
                    's' => "saphel",
                    'e' => "shaphel",
                    'H' => "hophal",
                    'i' => "ithpeel",
                    't' => "hishtaphel",
                    'v' => "ishtaphel",
                    'w' => "hithaphel",
                    'o' => "polel",
                    'z' => "ithpoel",
                    'r' => "hithpolel",
                    'f' => "hithpalpel",
                    'b' => "hephal",
                    'c' => "tiphel",
                    'm' => "poel",
                    'l' => "palpel",
                    'L' => "ithpalpel",
                    'O' => "ithpolel",
                    'G' => "ittaphal"
                });
            }

            result.Append("-");
            // Form
            string form = string.Empty;
            if (morph.Length > 1)
                form = morph[1] switch
                {
                    'p' => "perfect(qatal)",
                    'q' => "sequential perfect(weqatal)",
                    'i' => "imperfect(yiqtol)",
                    'w' => "sequential imperfect(wayyiqtol)",
                    'h' => "cohortative",
                    'j' => "jussive",
                    'v' => "imperative",
                    'r' => "participle active",
                    's' => "participle passive",
                    'a' => "infinitive abs",
                    'c' => "infinitive const",
                    'u' => "conj+imperfect"
                };
            result.Append(form);
            result.Append("-");
            //Generally verbs require no state. Participles, on the other hand, require no person, though they do take a state.
            if (morph.Length > 4 && form.StartsWith("participle"))
            {
                // Participles require no person
                // gender
                result.Append(morph.Substring(2, 2));
                result.Append(morph[4] switch
                {
                    'a' => "-abs",
                    'c' => "-const",
                    'd' => "-determined"
                });
            }
            else if (morph.Length > 4)
            {
                result.Append(morph.Substring(2, 3));
                // Generally verbs require no state, but may be
                if (morph.Length > 5)
                {
                    result.Append(morph[5] switch
                    {
                        'a' => "-abs",
                        'c' => "-const",
                        'd' => "-determined",
                    });
                }
            }
            return result.ToString().Trim('-');
        }
    }
    public enum BibleLanguage
    {
        Hebrew,
        Aramaic,
        Greek
    }
}


