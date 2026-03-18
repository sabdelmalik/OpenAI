using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace HebrewAlignmentEngine
{
    // ===================== CORE ENGINE =====================
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

        public async Task<List<AlignmentResult>> AlignAsync(
            List<HebrewToken> sourceTokens,
            List<string> targetWords,
            ILexiconProvider lexicon,
            double threshold = 0.6)
        {
            var expandedSource = ExpandHebrewTokens(sourceTokens);

            var sourceTexts = expandedSource.Select(t => t.ContextualForm).ToList();
            var targetTexts = targetWords.Select(t => "English word: " + t).ToList();

            var sourceEmbeddings = await GetEmbeddingsBatch(sourceTexts);
            var targetEmbeddings = await GetEmbeddingsBatch(targetTexts);

            var results = new List<AlignmentResult>();

            for (int i = 0; i < expandedSource.Count; i++)
            {
                var source = expandedSource[i];
                var sVec = sourceEmbeddings[i];

                double bestScore = -1;
                int bestIndex = -1;

                for (int j = 0; j < targetWords.Count; j++)
                {
                    var tVec = targetEmbeddings[j];
                    double score = CosineSimilarity(sVec, tVec);

                    // Morphology boost
                    score += MorphologyBoost(source, targetWords[j]);

                    // Lexicon boost
                    if (lexicon != null && lexicon.IsMatch(source.Lemma, targetWords[j]))
                        score += 0.2;

                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestIndex = j;
                    }
                }

                if (bestScore >= threshold && bestIndex >= 0)
                {
                    results.Add(new AlignmentResult
                    {
                        SourceSurface = source.Surface,
                        SourceLemma = source.Lemma,
                        Morphology = source.Morphology,
                        Target = targetWords[bestIndex],
                        Score = bestScore
                    });
                }
            }

            return results;
        }

        // ===================== MORPHOLOGY =====================
        private List<ExpandedToken> ExpandHebrewTokens(List<HebrewToken> tokens)
        {
            var result = new List<ExpandedToken>();

            foreach (var t in tokens)
            {
                // Handle prefixes like ו, ב, ל, כ
                if (t.Prefixes != null)
                {
                    foreach (var p in t.Prefixes)
                    {
                        result.Add(new ExpandedToken
                        {
                            Surface = p,
                            Lemma = p,
                            Morphology = "prefix",
                            ContextualForm = BuildContext(p, "prefix")
                        });
                    }
                }

                result.Add(new ExpandedToken
                {
                    Surface = t.Surface,
                    Lemma = t.Lemma,
                    Morphology = t.Morphology,
                    ContextualForm = BuildContext(t.Lemma, t.Morphology)
                });
            }

            return result;
        }

        private string BuildContext(string lemma, string morphology)
        {
            return $"Hebrew word {lemma} with morphology {morphology}";
        }

        private double MorphologyBoost(ExpandedToken token, string target)
        {
            double boost = 0;

            if (token.Morphology.Contains("verb") && target.EndsWith("ed"))
                boost += 0.05;

            if (token.Morphology.Contains("noun") && char.IsUpper(target[0]))
                boost += 0.05;

            if (token.Morphology.Contains("prefix") &&
                (target.Equals("and", StringComparison.OrdinalIgnoreCase) ||
                 target.Equals("in", StringComparison.OrdinalIgnoreCase)))
                boost += 0.1;

            return boost;
        }

        // ===================== EMBEDDINGS =====================
        private async Task<List<float[]>> GetEmbeddingsBatch(List<string> texts)
        {
            var results = new List<float[]>();

            var uncached = texts.Where(t => !_embeddingCache.ContainsKey(t)).ToList();

            if (uncached.Any())
            {
                var requestBody = new
                {
                    input = uncached,
                    model = "text-embedding-3-small"
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("https://api.openai.com/v1/embeddings", content);
                response.EnsureSuccessStatusCode();

                var responseJson = await response.Content.ReadAsStringAsync();
                var parsed = JsonDocument.Parse(responseJson);

                var data = parsed.RootElement.GetProperty("data");

                for (int i = 0; i < uncached.Count; i++)
                {
                    var embeddingArray = data[i].GetProperty("embedding")
                        .EnumerateArray()
                        .Select(x => x.GetSingle())
                        .ToArray();

                    _embeddingCache[uncached[i]] = embeddingArray;
                }
            }

            foreach (var text in texts)
                results.Add(_embeddingCache[text]);

            return results;
        }

        private double CosineSimilarity(float[] a, float[] b)
        {
            double dot = 0, magA = 0, magB = 0;

            for (int i = 0; i < a.Length; i++)
            {
                dot += a[i] * b[i];
                magA += a[i] * a[i];
                magB += b[i] * b[i];
            }

            return dot / (Math.Sqrt(magA) * Math.Sqrt(magB));
        }
    }

    // ===================== MODELS =====================
    public class HebrewToken
    {
        public string Surface { get; set; }          // original form
        public string Lemma { get; set; }            // dictionary form
        public string Morphology { get; set; }       // e.g. Verb-Qal-Perfect-3ms
        public List<string> Prefixes { get; set; }   // ו, ב, ל, כ etc.
    }

    public class ExpandedToken
    {
        public string Surface { get; set; }
        public string Lemma { get; set; }
        public string Morphology { get; set; }
        public string ContextualForm { get; set; }
    }

    public class AlignmentResult
    {
        public string SourceSurface { get; set; }
        public string SourceLemma { get; set; }
        public string Morphology { get; set; }
        public string Target { get; set; }
        public double Score { get; set; }
    }

    // ===================== LEXICON =====================
    public interface ILexiconProvider
    {
        bool IsMatch(string lemma, string target);
    }

    // ===================== EXAMPLE =====================
    class Program
    {
        static async Task Main()
        {
            string apiKey = Environment.GetEnvironmentVariable("OPENAI_ALIGNER_KEY", EnvironmentVariableTarget.User);
            var engine = new AlignmentEngine(apiKey);

            var hebrew = new List<HebrewToken>
            {
                new HebrewToken { Surface = "בְּרֵאשִׁית", Lemma = "ראשית", Morphology = "noun", Prefixes = new List<string>{"ב"}},
                new HebrewToken { Surface = "בָּרָא", Lemma = "ברא", Morphology = "verb-qal" },
                new HebrewToken { Surface = "אֱלֹהִים", Lemma = "אלהים", Morphology = "noun" },
                new HebrewToken { Surface = "אֵ֥ת", Lemma = "אֵת", Morphology = "particle-direct indicator" },
                new HebrewToken { Surface = "הַשָּׁמַ֖יִם", Lemma = "שָׁמַיִם", Morphology = "noun", Prefixes = new List<string>{ "ה" } },
                new HebrewToken { Surface = "וְאֵ֥ת", Lemma = "אֵת", Morphology = "particle-direct indicator", Prefixes = new List<string>{ "וְ" } },
                new HebrewToken { Surface = "הָאָֽרֶץ", Lemma = "אֶ֫רֶץ", Morphology = "noun", Prefixes = new List<string>{ "ה" } }
           };

            var english = new List<string> { "In", "the", "beginning", "God", "created", "the", "heavens", "and", "the", "earth."};

            var results = await engine.AlignAsync(hebrew, english, null);

            foreach (var r in results)
            {
                Console.WriteLine($"{r.SourceSurface} ({r.Morphology}) -> {r.Target} ({r.Score:F2})");
            }
        }
    }
}
