// COMPLETE LLM-BASED HEBREW ALIGNMENT PIPELINE
// NOW WITH RAG (Automatic Example Selector)

// Includes:
// 1. API call (Responses API)
// 2. JSON schema enforcement
// 3. Retry + validation
// 4. Batch processing
// 5. Ready for interlinear export

using LLMHebrewAlignment;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace LLMHebrewAlignment
{
    // ================= RAG EXAMPLE SELECTOR =================
    public class ExampleAlignment
    {
        public string Reference { get; set; }
        public List<string> HebrewLemmas { get; set; }
        public List<string> POS { get; set; }
        public List<string> MorphPatterns { get; set; }
        public string JsonAlignment { get; set; }
    }

    public class HebrewFeature
    {
        public string Lemma { get; set; }
        public string POS { get; set; }
        public string Morph { get; set; }
    }

    public class ExampleSelector
    {
        private readonly List<ExampleAlignment> _examples;

        public ExampleSelector(List<ExampleAlignment> examples)
        {
            _examples = examples;
        }

        // Advanced similarity: lemma + POS + morphology aware
        public List<ExampleAlignment> GetBestExamples(List<HebrewFeature> features, int topN = 2)
        {
            return _examples
                .Select(e => new
                {
                    Example = e,
                    Score = ComputeScore(e, features)
                })
                .OrderByDescending(x => x.Score)
                .Take(topN)
                .Select(x => x.Example)
                .ToList();
        }

        private double ComputeScore(ExampleAlignment ex, List<HebrewFeature> features)
        {
            double score = 0;

            foreach (var f in features)
            {
                if (ex.HebrewLemmas.Contains(f.Lemma))
                    score += 2.0; // lemma match (strongest)

                if (ex.POS != null && ex.POS.Contains(f.POS))
                    score += 1.0; // POS match

                if (ex.MorphPatterns != null && ex.MorphPatterns.Any(m => f.Morph.Contains(m)))
                    score += 0.5; // morphology partial match
            }

            return score;
        }
    }

    public class AlignmentService
    {
        private readonly HttpClient _http;
        private readonly Dictionary<string, string> _cache = new();

        public AlignmentService(string apiKey)
        {
            _http = new HttpClient();
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", apiKey);
        }

        public async Task<AlignmentResult> AlignVerseAsync(object verseJson, ExampleSelector selector = null)
        {
            string input = JsonSerializer.Serialize(verseJson);

            if (_cache.ContainsKey(input))
                return ParseResult(_cache[input]);

            List<ExampleAlignment> examples = null;

            if (selector != null)
            {
                var lemmas = ExtractLemmas(verseJson);
                examples = selector.GetBestExamples(lemmas);
            }

            string prompt = BuildPrompt(input, examples);

            string response = await CallLLM(prompt);

            if (!IsValidJson(response))
            {
                // retry once
                response = await CallLLM(prompt);
            }

            _cache[input] = response;

            return ParseResult(response);
        }

        private List<string> ExtractLemmas(object verseJson)
        {
            var json = JsonSerializer.Serialize(verseJson);
            var doc = JsonDocument.Parse(json);

            var lemmas = new List<string>();

            foreach (var token in doc.RootElement
                .GetProperty("hebrew")
                .GetProperty("tokens")
                .EnumerateArray())
            {
                if (token.TryGetProperty("lemma", out var l))
                    lemmas.Add(l.GetString());
            }

            return lemmas;
        }

        private string BuildPrompt(string json, List<ExampleAlignment> examples = null)
        {
            var exampleText = "";

            if (examples != null && examples.Count > 0)
            {
                exampleText = $@"
Examples:
                ";
                foreach (var ex in examples)
                {
                    exampleText += $@"Reference: {{ex.Reference}}
{ ex.JsonAlignment}

                    ";
                }
            }

            return $@"
You are aligning a Biblical Hebrew verse to its translation.

Rules:
- Use token indices
- Allow one-to-many and many-to-many alignments
- Align by meaning
- Include ALL words

Return ONLY JSON:
{{
    {{
        ""alignments"": [
        {{
             {{ ""hebrew"": [0], ""target"": [0] }}
        }}
        ]
    }}
}}

{ exampleText}
        
Input:
{ json}
";
        }

        private async Task<string> CallLLM(string prompt)
        {
            var body = new
            {
                model = "gpt-5.1-mini",
                input = new[]
                {
                    new { role = "user", content = prompt }
                },
                temperature = 0,
                max_output_tokens = 800
            };

            var json = JsonSerializer.Serialize(body);

            var response = await _http.PostAsync(
                "https://api.openai.com/v1/responses",
                new StringContent(json, Encoding.UTF8, "application/json"));

            var str = await response.Content.ReadAsStringAsync();

            var parsed = JsonDocument.Parse(str);

            return parsed.RootElement
                .GetProperty("output")[0]
                .GetProperty("content")[0]
                .GetProperty("text")
                .GetString();
        }

        private bool IsValidJson(string text)
        {
            try
            {
                JsonDocument.Parse(text);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private AlignmentResult ParseResult(string json)
        {
            return JsonSerializer.Deserialize<AlignmentResult>(json);
        }

        // ================= JSON SCHEMA ENFORCEMENT =================
        private readonly object _jsonSchema = new
        {
            type = "object",
            properties = new
            {
                alignments = new
                {
                    type = "array",
                    items = new
                    {
                        type = "object",
                        properties = new
                        {
                            hebrew = new { type = "array", items = new { type = "integer" } },
                            target = new { type = "array", items = new { type = "integer" } }
                        },
                        required = new[] { "hebrew", "target" }
                    }
                }
            },
            required = new[] { "alignments" }
        };

        private string BuildPrompt(string json)
        {
            return $@"
You are aligning a Biblical Hebrew verse to its translation.

Rules:
- Use token indices
- Allow one-to-many and many-to-many alignments
- Align by meaning
- Include ALL words

Return ONLY JSON that conforms EXACTLY to this schema:
{JsonSerializer.Serialize(_jsonSchema)}

Do not include any explanation.

Input:
{json}
";
        }

        private bool ValidateSchema(string json)
        {
            try
            {
                var doc = JsonDocument.Parse(json);

                if (!doc.RootElement.TryGetProperty("alignments", out var aligns))
                    return false;

                foreach (var item in aligns.EnumerateArray())
                {
                    if (!item.TryGetProperty("hebrew", out var h) || h.ValueKind != JsonValueKind.Array)
                        return false;

                    if (!item.TryGetProperty("target", out var t) || t.ValueKind != JsonValueKind.Array)
                        return false;
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        // ================= BATCH =================
        public async Task<List<AlignmentResult>> AlignBatchAsync(List<object> verses)
        {
            var results = new List<AlignmentResult>();

            foreach (var v in verses)
            {
                var result = await AlignVerseAsync(v);
                results.Add(result);
            }

            return results;
        }
    }

    // ================= MODELS =================
    public class AlignmentResult
{
    public List<AlignmentPair> alignments { get; set; }
}

public class AlignmentPair
{
    public List<int> hebrew { get; set; }
    public List<int> target { get; set; }
}

// ================= INTERLINEAR EXPORT =================
public class InterlinearBuilder
{
    public static void Print(AlignmentResult result, dynamic verse)
    {
        foreach (var align in result.alignments)
        {
            Console.Write("Hebrew: ");
            foreach (var i in align.hebrew)
                Console.Write(verse.hebrew.tokens[i].surface + " ");

            Console.Write(" -> English: ");
            foreach (var i in align.target)
                Console.Write(verse.target.tokens[i].word + " ");

            Console.WriteLine();
        }
    }
}

// ================= COST TRACKER =================
public class CostTracker
{
    private const double InputCostPer1K = 0.0003;
    private const double OutputCostPer1K = 0.0006;

    public double TotalCost { get; private set; } = 0;

    public double AddUsage(int inputTokens, int outputTokens)
    {
        double cost = (inputTokens / 1000.0 * InputCostPer1K) +
                      (outputTokens / 1000.0 * OutputCostPer1K);

        TotalCost += cost;

        return cost;
    }


// ================= UPDATED CALL WITH COST =================
private async Task<(string content, int inputTokens, int outputTokens)> CallLLMWithUsage(string prompt)
{
    var body = new
    {
        model = "gpt-4.1-mini",
        input = new[]
        {
                    new { role = "user", content = prompt }
                },
        temperature = 0,
        max_output_tokens = 800
    };

    var json = JsonSerializer.Serialize(body);

    var response = await _http.PostAsync(
        "https://api.openai.com/v1/responses",
        new StringContent(json, Encoding.UTF8, "application/json"));

    var str = await response.Content.ReadAsStringAsync();
    var parsed = JsonDocument.Parse(str);

    var content = parsed.RootElement
        .GetProperty("output")[0]
        .GetProperty("content")[0]
        .GetProperty("text")
        .GetString();

    var usage = parsed.RootElement.GetProperty("usage");

    int inputTokens = usage.GetProperty("input_tokens").GetInt32();
    int outputTokens = usage.GetProperty("output_tokens").GetInt32();

    return (content, inputTokens, outputTokens);
}

// ================= EXAMPLE =================
class Program
{
    static async Task Main()
    {
        var service = new AlignmentService("YOUR_API_KEY");

        var verse = new
        {
            reference = "Genesis 1:1",
            hebrew = new
            {
                text = "בראשית ברא אלהים",
                tokens = new[]
                {
                        new { i = 0, surface = "בראשית", lemma = "ראשית", pos = "noun", morph = "construct", gloss = "beginning" },
                        new { i = 1, surface = "ברא", lemma = "ברא", pos = "verb", morph = "qal perfect", gloss = "create" },
                        new { i = 2, surface = "אלהים", lemma = "אלהים", pos = "noun", morph = "plural", gloss = "God" }
                    }
            },
            target = new
            {
                text = "In the beginning God created",
                tokens = new[]
                {
                        new { i = 0, word = "In" },
                        new { i = 1, word = "the" },
                        new { i = 2, word = "beginning" },
                        new { i = 3, word = "God" },
                        new { i = 4, word = "created" }
                    }
            }
        };

        var result = await service.AlignVerseAsync(verse);

        InterlinearBuilder.Print(result, verse);
    }
}
}
