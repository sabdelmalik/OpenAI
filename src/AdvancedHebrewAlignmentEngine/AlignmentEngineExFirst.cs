// ADVANCED HEBREW ALIGNMENT ENGINE (UPGRADED WITH CONTEXT + PHRASES + RANKING)

using AdvancedHebrewAlignmentEngine.Graph;
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
    public class AlignmentEngineExFirst
    {
        private readonly string _apiKey;
        private readonly HttpClient _httpClient;
        private readonly Dictionary<string, float[]> _cache = new();
        private Dictionary<string, string> _llmCache = new();

        public AlignmentEngineExFirst(string apiKey)
        {
            _apiKey = apiKey;
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _apiKey);
        }

        public async Task<AlignmentGraph> AlignAsync(
            List<HebrewToken> tokens,
            List<string> targets,
            ILexiconProvider lexicon,
            string verseReference,
            string hebrewVerse,
            string englishVerse)
        {
            var expanded = Expand(tokens);
            var phrases = BuildPhrases(expanded);

            var sourceTexts = tokens //phrases
                .Select(p => BuildHebrewContext(p, verseReference, hebrewVerse))
                .ToList();

            var targetTexts = targets
                .Select(t => BuildEnglishContext(t, verseReference, englishVerse))
                .ToList();

            var sourceVecs = await Embed(sourceTexts);
            var targetVecs = await Embed(targetTexts);

            var graph = new AlignmentGraph();

            for (int i = 0; i < /*phrases*/tokens.Count; i++)
            {
                var candidates = new List<AlignmentEdge>();

                for (int j = 0; j < targets.Count; j++)
                {
                    double score = Cosine(sourceVecs[i], targetVecs[j]);

                    score += MorphBoost(/*phrases*/tokens[i], targets[j]);

                    //if (lexicon != null && lexicon.IsMatch(phrases[i].Lemma, targets[j]))
                    if (/*tokens[i].Gloss.ToLower().Contains(targets[j].ToLower()) || */targets[j].ToLower() == tokens[i].Gloss.ToLower())
                        score += 0.35;

                    if (tokens[i].Surface != "אֵ֥ת")
                    {
                        candidates.Add(new AlignmentEdge
                        {
                            Source = tokens[i].Surface, //phrases[i].Text,
                            Target = targets[j],
                            Score = score
                        });

                    }
                }

                // Keep top-N matches instead of thresholding only
                var top = candidates
                    .OrderByDescending(c => c.Score)
                    .Take(3) // top 3 candidates
                    .Where(c => c.Score > 0.35)
                    .ToList();

                // graph.Edges.AddRange(top);
                // LLM refinement for ambiguous cases
                var candidateWords = top.Select(c => c.Target).ToList();

                // If multiple strong candidates, ask LLM to choose the best one. Add  "&& top[0].Score < 0.75" to control cost
                if (candidateWords.Count > 1 && top[0].Score < 0.75)//75) 
                {
                    var best = await RefineWithLLM(
                        tokens[i].Surface, //phrases[i].Text,
                        candidateWords,
                        verseReference,
                        hebrewVerse,
                        englishVerse);

                    var chosen = top.FirstOrDefault(c =>
                        c.Target.Equals(best, StringComparison.OrdinalIgnoreCase));

                    if (chosen != null)
                        graph.Edges.Add(chosen);
                }
                else if (candidateWords.Count > 0)
                {
                    graph.Edges.Add(top[0]);
                }
                else
                {
                    int x = 0;
                }
            }

            graph.ComputeConfidence();
            return graph;
        }

        // ================= LLM =================
        private async Task<string> RefineWithLLM(
    string hebrew,
    List<string> candidates,
    string reference,
    string hebrewVerse,
    string englishVerse)
        {
            string cacheKey = hebrew + "|" + string.Join(",", candidates);
            if (_llmCache.ContainsKey(cacheKey))
                return _llmCache[cacheKey]; 

            var prompt = $@"
You are aligning a Hebrew Bible word to its English translation.

Verse: {reference}

Hebrew verse:
{hebrewVerse}

English verse:
{englishVerse}

Hebrew word:
{hebrew}

Candidate translations:
{string.Join(", ", candidates)}

Select the BEST translation for this word in this verse.
Return ONLY the chosen word.
";

            var requestBody = new
            {
                // gpt-4.1-mini is a cheaper variant of gpt-4.1, good for simple disambiguation tasks. Adjust as needed based on cost/performance tradeoff.
                model = "gpt-4.1-mini", // cheap + good enough $.80/MTokens
                messages = new[]
                {
            new { role = "user", content = prompt }
        },
                max_tokens = 10,
                temperature = 0
            };

            var json = JsonSerializer.Serialize(requestBody);

            var response = await _httpClient.PostAsync(
                "https://api.openai.com/v1/chat/completions",
                new StringContent(json, Encoding.UTF8, "application/json"));

            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();
            var parsed = JsonDocument.Parse(responseJson);

            var result = parsed.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString()
                .Trim();

            _llmCache[cacheKey] = result; // Cache the LLM decision
            return result;
        }

        // ================= CONTEXT BUILDERS =================
        private string BuildHebrewContext(/*Phrase*/ HebrewToken p, string reference, string verse)
        {
            //return $"In {reference}, Hebrew phrase '{p.Text}' (lemma: {p.Lemma}, morphology: {p.Morph}) in the verse '{verse}'";
            return $"In {reference}, Hebrew phrase '{p.Surface}' (lemma: {p.Lemma}, pos: {p.POS}, morphology: {p.Morphology}) in the verse '{verse}'";
        }

        private string BuildEnglishContext(string word, string reference, string verse)
        {
            return $"In {reference}, English word '{word}' in the sentence '{verse}' meaning {word}";
        }

        // ================= EXPANSION =================
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

                if (t.Morphology.Contains("verb"))
                {
                    list.Add(new ExpandedToken
                    {
                        Text = "(implicit subject)",
                        Lemma = "subject",
                        POS = "pronoun",
                        Context = "Hebrew implicit subject"
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
                phrases.Add(new Phrase(tokens[i]));

                if (i < tokens.Count - 1)
                    phrases.Add(new Phrase(tokens[i], tokens[i + 1]));

                if (i < tokens.Count - 2)
                    phrases.Add(new Phrase(tokens[i], tokens[i + 1], tokens[i + 2]));
            }

            return phrases;
        }

        // ================= EMBEDDINGS =================
        private async Task<List<float[]>> Embed(List<string> texts)
        {
            var result = new List<float[]>();
            var uncached = texts.Where(t => !_cache.ContainsKey(t)).ToList();

            if (uncached.Any())
            {
                var body = new { input = uncached, model = "text-embedding-3-small" };
                var json = JsonSerializer.Serialize(body);

                var res = await _httpClient.PostAsync(
                    "https://api.openai.com/v1/embeddings",
                    new StringContent(json, Encoding.UTF8, "application/json"));

                var parsed = JsonDocument.Parse(await res.Content.ReadAsStringAsync());
                var data = parsed.RootElement.GetProperty("data");

                for (int i = 0; i < uncached.Count; i++)
                {
                    _cache[uncached[i]] = data[i]
                        .GetProperty("embedding")
                        .EnumerateArray()
                        .Select(x => x.GetSingle())
                        .ToArray();
                }
            }

            foreach (var t in texts)
                result.Add(_cache[t]);

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

        private double MorphBoost(/*Phrase*/HebrewToken p, string target)
        {
            double score = 0;

            //if (p.Morph.Contains("prefix") &&
            if (p.POS.Contains("prefix") &&
                (target.Equals("and", StringComparison.OrdinalIgnoreCase) ||
                 target.Equals("in", StringComparison.OrdinalIgnoreCase)))
                score += 0.1;

            //if (p.Morph.Contains("verb") && target.EndsWith("ed"))
            if (p.POS.Contains("verb") && target.EndsWith("ed"))
                score += 0.05;

            return score;
        }
    }

}
