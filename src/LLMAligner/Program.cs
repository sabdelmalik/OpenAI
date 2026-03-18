// COMPLETE LLM-BASED HEBREW ALIGNMENT PIPELINE
// Includes:
// 1. API call (Responses API)
// 2. JSON schema enforcement
// 3. Retry + validation
// 4. Batch processing
// 5. Ready for interlinear export

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml;

namespace LLMHebrewAlignment
{
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

        public async Task<AlignmentResult> AlignVerseAsync(object verseJson)
        {
            string input = JsonSerializer.Serialize(verseJson);

            if (_cache.ContainsKey(input))
                return ParseResult(_cache[input]);

            string prompt = BuildPrompt(input);

            string response = await CallLLM(prompt);

            if (!IsValidJson(response))
            {
                // retry twice
                for (int i = 0; i < 2; i++)
                {
                    response = await CallLLM(prompt);

                    if (IsValidJson(response) && ValidateSchema(response))
                        break;
                }
            }

            _cache[input] = response;

            return ParseResult(response);
        }

/*
        private string BuildPrompt(string json)
        {
            return $@"
You are aligning a Biblical Hebrew verse to its translation.

Rules:
- Use token indices
- Allow one-to-many and many-to-many alignments
- Align by meaning
- Include ALL words

Return ONLY JSON:
{{
  \"alignments\": [
    {
                { \"hebrew\": [0], \"target\": [0] }}
  ]
}
            }

        Input:
            { json}
            ";
        }
*/
        private async Task<string> CallLLM(string prompt)
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

            string output = "{}"; // return empty JSON on error

            try
            {
                output = parsed.RootElement
                    .GetProperty("output")[0]
                    .GetProperty("content")[0]
                    .GetProperty("text")
                    .GetString();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error parsing LLM response: " + ex.Message);
                var values = JsonSerializer.Deserialize<Dictionary<string, object>>(parsed);

                if (values != null)
                {
                    Console.WriteLine("Root element properties (from Dictionary):");
                    // Get the keys from the dictionary
                    foreach (var property in values)
                    {
                        Console.WriteLine($"- {property.Key} = {property.Value}");
                    }
                }
            }

            return output;

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

    // ================= EXAMPLE =================
    class Program
    {
        static async Task Main()
        {
            string apiKey = Environment.GetEnvironmentVariable("OPENAI_ALIGNER_KEY", EnvironmentVariableTarget.User);
            var service = new AlignmentService(apiKey);

            Console.OutputEncoding = Encoding.UTF8;

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
