using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BibleAlignmentEngine
{
    public class AlignmentEngine
    {
        private readonly string _apiKey;
        private readonly HttpClient _httpClient;

        // Cache embeddings to save cost
        private readonly Dictionary<string, float[]> _embeddingCache = new();

        public AlignmentEngine(string apiKey)
        {
            _apiKey = apiKey;
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _apiKey);
        }

        // ===== PUBLIC ENTRY POINT =====
        public async Task<List<AlignmentResult>> AlignAsync(
            List<string> sourceWords,
            List<string> targetWords,
            ILexiconProvider lexicon = null,
            double similarityThreshold = 0.65)
        {
            var sourceEmbeddings = await GetEmbeddingsBatch(sourceWords);
            var targetEmbeddings = await GetEmbeddingsBatch(targetWords);

            var results = new List<AlignmentResult>();

            for (int i = 0; i < sourceWords.Count; i++)
            {
                string source = sourceWords[i];
                var sourceVec = sourceEmbeddings[i];

                double bestScore = -1;
                int bestIndex = -1;

                for (int j = 0; j < targetWords.Count; j++)
                {
                    string target = targetWords[j];
                    var targetVec = targetEmbeddings[j];

                    double score = CosineSimilarity(sourceVec, targetVec);

                    // Boost score using lexicon hint
                    if (lexicon != null && lexicon.IsMatch(source, target))
                    {
                        score += 0.15; // adjustable weight
                    }

                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestIndex = j;
                    }
                }

                if (bestScore >= similarityThreshold && bestIndex >= 0)
                {
                    results.Add(new AlignmentResult
                    {
                        Source = source,
                        Target = targetWords[bestIndex],
                        Score = bestScore
                    });
                }
            }

            return results;
        }

        // ===== EMBEDDINGS =====
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
            {
                results.Add(_embeddingCache[text]);
            }

            return results;
        }

        // ===== COSINE SIMILARITY =====
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

    // ===== RESULT MODEL =====
    public class AlignmentResult
    {
        public string Source { get; set; }
        public string Target { get; set; }
        public double Score { get; set; }
    }

    // ===== LEXICON INTERFACE =====
    public interface ILexiconProvider
    {
        bool IsMatch(string sourceWord, string targetWord);
    }

    // ===== SAMPLE LEXICON IMPLEMENTATION =====
    public class SimpleLexicon : ILexiconProvider
    {
        private readonly Dictionary<string, List<string>> _map;

        public SimpleLexicon(Dictionary<string, List<string>> map)
        {
            _map = map;
        }

        public bool IsMatch(string sourceWord, string targetWord)
        {
            if (!_map.ContainsKey(sourceWord)) return false;

            return _map[sourceWord].Any(t =>
                string.Equals(t, targetWord, StringComparison.OrdinalIgnoreCase));
        }
    }

    // ===== EXAMPLE USAGE =====
    class Program
    {
        static async Task Main()
        {
            var apiKey = "YOUR_API_KEY";

            var engine = new AlignmentEngine(apiKey);

            var greek = new List<string> { "ἐν", "ἀρχῇ", "ἦν", "λόγος" };
            var english = new List<string> { "In", "the beginning", "was", "the Word" };

            var lexiconData = new Dictionary<string, List<string>>
            {
                { "λόγος", new List<string> { "Word" } },
                { "ἀρχῇ", new List<string> { "beginning" } }
            };

            var lexicon = new SimpleLexicon(lexiconData);

            var results = await engine.AlignAsync(greek, english, lexicon);

            foreach (var r in results)
            {
                Console.WriteLine($"{r.Source} -> {r.Target} ({r.Score:F2})");
            }
        }
    }
}
