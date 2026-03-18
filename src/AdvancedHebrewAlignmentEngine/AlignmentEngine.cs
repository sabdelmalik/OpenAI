using AdvancedHebrewAlignmentEngine.Graph;
using AdvancedHebrewAlignmentEngine.Models;
using HebrewAlignmentEngineAdvanced;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace AdvancedHebrewAlignmentEngine
{
    public class AlignmentEngine
    {
        private readonly string _apiKey;
        private readonly HttpClient _httpClient;
        private readonly Dictionary<string, float[]> _embeddingCache = new();

        public AlignmentEngine(string apiKey)
        {
            _apiKey = apiKey;
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _apiKey);
        }

        /// <summary>
        /// Aligns Hebrew tokens to English target phrases using embeddings and morphological analysis.
        /// </summary>
        /// <param name="tokens"></param>
        /// <param name="targets"></param>
        /// <param name="lexicon"></param>
        /// <returns></returns>
        public async Task<AlignmentGraph> AlignAsync(List<HebrewToken> tokens, List<string> targets, ILexiconProvider lexicon)
        {
            var expanded = Expand(tokens);
            var phrases = BuildPhrases(expanded);

            var sourceTexts = phrases.Select(p => p.Context).ToList();
            var targetTexts = targets.Select(t => "English: " + t).ToList();

            var sourceVecs = await Embed(sourceTexts);
            var targetVecs = await Embed(targetTexts);

            var graph = new AlignmentGraph();

            for (int i = 0; i < phrases.Count; i++)
            {
                for (int j = 0; j < targets.Count; j++)
                {
                    double score = Cosine(sourceVecs[i], targetVecs[j]);

                    score += MorphBoost(phrases[i], targets[j]);

                    if (lexicon != null && lexicon.IsMatch(phrases[i].Lemma, targets[j]))
                        score += 0.2;

                    if (score > 0.5)
                    {
                        graph.Edges.Add(new AlignmentEdge
                        {
                            Source = phrases[i].Text,
                            Target = targets[j],
                            Score = score
                        });
                    }
                }
            }

            graph.ComputeConfidence();
            return graph;
        }

        // ================= EXPANSION =================
        /// <summary>
        /// expands Hebrew tokens into multiple alignment candidates by:
        /// 
        /// </summary>
        /// <param name="tokens"></param>
        /// <returns></returns>
        private List<ExpandedToken> Expand(List<HebrewToken> tokens)
        {
            var list = new List<ExpandedToken>();

            foreach (var t in tokens)
            {
                if (t.Prefixes != null)
                {
                    foreach (var p in t.Prefixes)
                    {
                        list.Add(new ExpandedToken
                        {
                            Text = p,
                            Lemma = p,
                            POS = "prefix",
                            Context = $"Hebrew prefix {p}"
                        });
                    }
                }

                // Verb expansion
                //if (t.Morphology.Contains("verb"))
                if (t.POS.Contains("verb"))
                {
                    list.Add(new ExpandedToken
                    {
                        Text = "(implicit subject)",
                        Lemma = "subject",
                        POS = "pronoun",
                        Context = "Hebrew implicit subject pronoun"
                    });
                }

                list.Add(new ExpandedToken
                {
                    Text = t.Surface,
                    Lemma = t.Lemma,
                    POS = t.POS,
                    Morph = t.Morphology,
                    Context = $"Hebrew {t.Lemma} {t.Morphology}"
                });
            }

            return list;
        }

        // ================= PHRASES =================
        private List<Phrase> BuildPhrases(List<ExpandedToken> tokens)
        {
            var phrases = new List<Phrase>();

            for (int i = 0; i < tokens.Count; i++)
            {
                // Single
                phrases.Add(new Phrase(tokens[i]));

                // Pair (construct chains approximation)
                if (i < tokens.Count - 1)
                {
                    phrases.Add(new Phrase(tokens[i], tokens[i + 1]));
                }
            }

            return phrases;
        }

        // ================= EMBEDDINGS =================
        private async Task<List<float[]>> Embed(List<string> texts)
        {
            var result = new List<float[]>();

            var uncached = texts.Where(t => !_embeddingCache.ContainsKey(t)).ToList();

            if (uncached.Any())
            {
                var body = new { input = uncached, model = "text-embedding-3-small" };
                var json = JsonSerializer.Serialize(body);
                var res = await _httpClient.PostAsync("https://api.openai.com/v1/embeddings",
                    new StringContent(json, Encoding.UTF8, "application/json"));

                var parsed = JsonDocument.Parse(await res.Content.ReadAsStringAsync());
                var data = parsed.RootElement.GetProperty("data");

                for (int i = 0; i < uncached.Count; i++)
                {
                    _embeddingCache[uncached[i]] = data[i].GetProperty("embedding")
                        .EnumerateArray().Select(x => x.GetSingle()).ToArray();
                }
            }

            foreach (var t in texts)
                result.Add(_embeddingCache[t]);

            return result;
        }

        private double Cosine(float[] a, float[] b)
        {
            double dot = 0, ma = 0, mb = 0;
            for (int i = 0; i < a.Length; i++)
            {
                dot += a[i] * b[i];
                ma += a[i] * a[i];
                mb += b[i] * b[i];
            }
            return dot / (Math.Sqrt(ma) * Math.Sqrt(mb));
        }

        private double MorphBoost(Phrase p, string target)
        {
            double score = 0;

            //if (p.Morph.Contains("prefix") && (target == "and" || target == "in"))
            //    score += 0.1;

            //if (p.Morph.Contains("verb") && target.EndsWith("ed"))
            //    score += 0.05;

            if (p.POS.Contains("prefix") && (target == "and" || target == "in"))
                score += 0.1;

            if (p.POS.Contains("verb") && target.EndsWith("ed"))
                score += 0.05;

            return score;
        }
    }

}
