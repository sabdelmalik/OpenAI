using AdvancedAligner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AdvancedAligner
{
    public class OtVerse
    {
        public OtVerse(string reference)
        {
            Reference = reference;
            RawTokens = new List<AiRawToken>();
            Tokens = new List<HebrewAramaicToken>();
            CompactTokens = new List<HebrewAramaicTokenCompact>();
        }

        public void AddRawToken(AiRawToken rawToken)
        {
            RawTokens.Add(rawToken);
            // Convert AiRawToken to ClaudeHebrewAramaicToken and add to Tokens list
            //// if strongs contains a dash use the part before the dash, otherwise use the whole strongs value
            string strongs = rawToken.strongs.Contains("-") ? rawToken.strongs.Split('-')[0].Trim() : rawToken.strongs.Trim();
            strongs = rawToken.strongs;
            //// if strongs has a suffix starting with an underscore, remove the suffix and trim whitespace
            // strongs = strongs.Contains("_") ? strongs.Split('_')[0].Trim() : strongs.Trim();

            HebrewAramaicToken token = new HebrewAramaicToken
            {
                index = rawToken.index,
                // remove any slashes from the surface form and trim whitespace
                surface = rawToken.surface.Replace("/","").Trim(),
                strong = strongs,
                lemma = rawToken.lemma,
                gloss = rawToken.gloss,
            };
            ClaudeMorphFeatureExtractor.ApplyFromMorphCode(rawToken.morphology, token);
            HebrewAramaicTokenCompact compactToken = new HebrewAramaicTokenCompact
            {
                index = token.index,
                surface = token.surface,
                pos = token.pos,
                morph = token.morph,
            };
            Tokens.Add(token);
            CompactTokens.Add(compactToken);
        }

        public string VerseText
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                foreach (var token in Tokens)
                {
                    sb.Append(token.surface);
                    sb.Append(' ');
                }
                return sb.ToString().Trim();
            }
        }

        public string Reference { get; set; }
        public List<AiRawToken> RawTokens { get; set; }
        public List<HebrewAramaicToken> Tokens { get; set; }
        public List<HebrewAramaicTokenCompact> CompactTokens { get; set; }
        //private string VerseText
        //{
        //    get
        //    {
        //        StringBuilder sb = new StringBuilder();
        //        foreach (var word in Words)
        //        {
        //            sb.Append(word.Hebrew);
        //            sb.Append(' ');
        //        }
        //        return sb.ToString().Trim();
        //    }
        //}

        //private string VerseStrongs
        //{
        //    get
        //    {
        //        StringBuilder sb = new StringBuilder();
        //        foreach (var word in Words)
        //        {
        //            if (!string.IsNullOrEmpty(word.StrongsNumber))
        //            {
        //                sb.Append(word.StrongsNumber);
        //                sb.Append(' ');
        //            }
        //            else
        //            {
        //                sb.Append("----- ");
        //            }
        //        }
        //        return sb.ToString().Trim();
        //    }
        //}

        //public Dictionary<string, string> WordToStrongsList
        //{
        //    get
        //    {
        //        Dictionary<string, string> dict = new Dictionary<string, string>();
        //        foreach (var word in Words)
        //        {
        //            if (!string.IsNullOrEmpty(word.StrongsNumber))
        //            {
        //                if(!dict.ContainsKey(word.Hebrew))
        //                    dict[word.Hebrew] = word.StrongsNumber;
        //            }
        //        }
        //        return dict;
        //    }
        //}

        //override public string ToString()
        //{
        //    return $"{Reference}: {VerseText} [{VerseStrongs}]";
        //}

    }
}
