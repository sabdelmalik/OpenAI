using OpenAiAPI.Models;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace OpenAiAPI
{
    /// <summary>
    /// Make OpenAiService a singelton
    /// </summary>
    public class OpenAiService
    {
        private readonly HttpClient _http;
        private readonly Dictionary<string, string> _cache = new();

        public OpenAiService()
        {
            string apiKey = Environment.GetEnvironmentVariable("OPENAI_ALIGNER_KEY", EnvironmentVariableTarget.User);
            _http = new HttpClient();
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", apiKey);
        }

        //public async Task<List<AlignmentResult>> AlignVersesAsync(object verses, GptModel model)
        public async Task<PromptResult> AlignVersesAsync(object verses, GptModel model)
        {
            Stopwatch sw = Stopwatch.StartNew();

            string? input = (verses is string) ?
                verses.ToString() : JsonSerializer.Serialize(verses);

            if (input == null) 
            {
                return new PromptResult(false, string.Empty, string.Empty, 0, 0, model, "AlignVersesAsync: input is null"); 
            }

//            if (_cache.ContainsKey(input))
//                return ParseResult(_cache[input]);

            string prompt = BuildPrompt(input);

            PromptResult response = await CallLLM(prompt, model);

            if (!IsValidJson(response.result))
            {
                // retry twice
                for (int i = 0; i < 2; i++)
                {
                    response = await CallLLM(prompt, model);

                    if (IsValidJson(response.result) && ValidateSchema(response.result))
                        break;
                }
            }

            _cache[input] = response.result;
            sw.Stop();
            long elapsedMilliseconds = sw.ElapsedMilliseconds;
            // convert elapsedMilliseconds to hh:mm:ss.msc
            TimeSpan t = TimeSpan.FromMilliseconds(elapsedMilliseconds);

            response.time = t.ToString();
            response.ParsedResult = ParseResult(response.result);

            return response;
        }

        public async Task<PromptResult> AlignVersesAsync(string prompt, GptModel model)
        {
            Stopwatch sw = Stopwatch.StartNew();

            if (string.IsNullOrEmpty(prompt))
            {
                return new PromptResult(false, prompt, string.Empty, 0, 0, model, "AlignVersesAsync: input is null");
            }

            //            if (_cache.ContainsKey(input))
            //                return ParseResult(_cache[input]);

            //string prompt = BuildPrompt(input);

            PromptResult response = await CallLLM(prompt, model);

            if (!IsValidJson(response.result))
            {
                // retry twice
                for (int i = 0; i < 2; i++)
                {
                    response = await CallLLM(prompt, model);

                    if (IsValidJson(response.result) && ValidateSchema(response.result))
                        break;
                }
            }

            //_cache[input] = response.result;
            sw.Stop();
            long elapsedMilliseconds = sw.ElapsedMilliseconds;
            // convert elapsedMilliseconds to hh:mm:ss.msc
            TimeSpan t = TimeSpan.FromMilliseconds(elapsedMilliseconds);

            response.time = t.ToString();
            response.ParsedResult = ParseResult(response.result);

            return response;
        }
        private async Task<PromptResult> CallLLM(string prompt, GptModel gptModel)
        {
            bool success = true;
            StringBuilder errorString = new StringBuilder();

            string output = "{}"; // return empty JSON on error
            int inputTokens = 0;
            int outputTokens = 0;

            string modelToUse = gptModel switch
            {
                GptModel.gpt_4_1_mini => "gpt-4.1-mini",
                GptModel.gpt_4_1 => "gpt-4.1",
                _ => ""
            };

            if (modelToUse == "")
            {
                errorString.Append("Invalid Model");
                success = false;
            }
            else
            {
                var body = new
                {
                    model = "gpt-4.1",
                    input = new[]
                    {
                    new { role = "user", content = prompt }
                },
                    temperature = 0,
                    max_output_tokens = 20000
                };

                var json = JsonSerializer.Serialize(body);

                var response = await _http.PostAsync(
                    "https://api.openai.com/v1/responses",
                    new StringContent(json, Encoding.UTF8, "application/json"));

                var str = await response.Content.ReadAsStringAsync();

                var parsed = JsonDocument.Parse(str);

                try
                {
                    output = parsed.RootElement
                        .GetProperty("output")[0]
                        .GetProperty("content")[0]
                        .GetProperty("text")
                        .GetString();
                    var usage = parsed.RootElement.GetProperty("usage");
                    inputTokens = usage.GetProperty("input_tokens").GetInt32();
                    outputTokens = usage.GetProperty("output_tokens").GetInt32();
                }
                catch (Exception ex)
                {
                    success = false;

                    errorString.AppendLine("Error parsing LLM response: " + ex.Message);
                    var values = JsonSerializer.Deserialize<Dictionary<string, object>>(parsed);

                    if (values != null)
                    {
                        errorString.AppendLine("Root element properties (from Dictionary):");
                        // Get the keys from the dictionary
                        foreach (var property in values)
                        {
                            errorString.AppendLine($"- {property.Key} = {property.Value}");
                        }
                    }
                }
            }

            return new PromptResult(success, prompt, output, inputTokens, outputTokens, gptModel, errorString.ToString());

        }

        private bool IsValidJson(string text)
        {
            try
            {
                JsonDocument.Parse(text);
                return true;
            }
            catch(Exception ex)
            {
                return false;
            }
        }

        private List<AlignmentResult> ParseResult(string json)
        {
            var result = JsonSerializer.Deserialize<List<AlignmentResult>>(json);
            return result;
        }

        // ================= JSON SCHEMA ENFORCEMENT =================
        private readonly object _jsonSchema = new
        {
            type = "object",
            properties = new
            {
                reference = new { type = "string" },
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
You are aligning multiple Biblical Hebrew verses to their English target translations.
The input Hebrew verse is given as a complete text, followed by the same text tokenised. Each token has an index indicating its position in the verse. Each token may represent word, an affix, a conjunction, … Each token includes morphological information to help in the alignment
The input target verse is given as a complete text, followed by the same text tokenised. Each token has an index indicating its position in the verse.

The Input JSON is structured as in the following example:
Example Start ====
[
{{
""reference"": ""Gen.1.1"",
""hebrew"": {{
""text"": ""בְּרֵאשִׁ֖ית בָּרָ֣א אֱלֹהִ֑ים אֵ֥ת הַשָּׁמַ֖יִם וְאֵ֥ת הָאָֽרֶץ"",
""tokens"": [
{{""i"": 0, ""surface"": ""בְּ"", ""lemma"": ""ב"", ""pos"": ""preposition"", ""morph"": ""inseparable_prep"",  gloss"": ""in""}},
{{“i"": 1, “surface"": ""רֵאשִׁ֖ית"", “lemma"": ""רֵאשִׁית"", “pos"": ""noun"", “morph"": ""common-fs-abs"", “gloss"": ""beginning"" }},
{{“i"": 2, “surface"": ""בָּרָ֣א"", “lemma"": ""בָּרָא"", “pos"": ""verb"", “morph"": ""qal-perfect(qatal)-3ms"", “gloss"": ""he created""}},
…
]
}},
""target"": {{
""text"": ""In the beginning God created the heavens and the earth."",
""tokens"": [
{{“i"": 0, “word"": ""In"" }},
{{“i"": 1, “word"": ""the""}},
{{“i"": 2, “word"": ""beginning""}},
…
]
}}
}}
]
Example end ====

Task:
For EACH verse:
Align Hebrew tokens to target tokens. Pay attention to the Hebrew token details: lemma, pos, morphology and gloss.

Rules:
- Align by meaning
- Use token indices
- Allow one-to-many and many-to-many alignments
- Do not combine articles and conjunctions with other words if they have a Hebrew token to align to
- Combine target tokens when it makes morphological sense
- if a target pronoun DOES NOT HAVE a corresponding Hebrew token, but is implied in the Hebrew verb, combine it with the target verb
- Include ALL target words
- Treat Hebrew words connected with a maqqef as seperate words (ignore the maqqef)
- The Hebrew אֵת should not be mapped
- IMPORTANT: Any token can be used only once in the mapping.
- Try to always have Hebrew prepositions morph=inseparable_prep, appear in the output mapping
  For example if לָ֭מָּה is represented by tokens 0=לָ֭ and 1=מָּה a corresponding Engilish token ""why"" would have its hebrew indices as [0,1] 

In the output JSON 
- tokens are identified by their indices
- All target indices must appear in the map
- Target indices should appear in the same order as in the input
- A target token's index can appear only once in
- If a target token does not have a Hebrew equivalent, its Hebrew indices will be empty []
- A Hebrew token index can appear only once
- Ignore a Hebrew token if it does not correspond to any of the target tokens.
- Add brief notes explaining each alignment decision

The output returned should be JSON in this format:
[
{{
""reference"": ""Genesis 1:1"",
""alignments"": [
{{""t"":[...],""h"":[..], ""notes"": notes, ""Brief notes explaining each alignment decision""}}, 
…
]
}},
{{
""reference"": ""Genesis 1:2"",
""alignments"": [
{{""t"":[...],""h"":[..], ""notes"": notes, ""Brief notes explaining each alignment decision""}}, 
…
]
}},
…
]
where """"t"""" = list of target token indices, """"h"""" = list of Hebrew token indices   

Return a raw JSON array with NO markdown formatting, NO code blocks, NO backticks, NO explanation.
Do NOT wrap in ```json or ``` markers.
Your response must be valid, parseable JSON that can be processed by JsonSerializer.Deserialize()
Start directly with [ and end directly with ]


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
        //public async Task<List<AlignmentResult>> AlignBatchAsync(List<object> verses)
        //{
        //    var results = new List<AlignmentResult>();

        //    foreach (var v in verses)
        //    {
        //        var result = await AlignVerseAsync(v);
        //        results.Add(result);
        //    }

        //    return results;
        //}
    }

    public class PromptResult
    {
        // gpt-4.1-mini pricing
        private double InputCostPer1K = 0.0004;
        private double OutputCostPer1K = 0.00016;

        public PromptResult(bool success,string prompt, string result, int inputTokens, int outputTokens, GptModel model, string errorString)    
        {
            this.success = success;
            this.prompt = prompt;
            this.result = result;
            this.inputTokens = inputTokens;
            this.outputTokens = outputTokens;

            // pricing
            if(model == GptModel.gpt_4_1)
            {
                InputCostPer1K = 0.002;
                OutputCostPer1K = 0.008;
            }

            this.model = model.ToString();

            cost = (inputTokens / 1000.0 * InputCostPer1K) +
              (outputTokens / 1000.0 * OutputCostPer1K);

            this.errorString = errorString;
        }

        public string model { get; private set; }
        public bool success { get; }
        public string result {  get; }

        public List<AlignmentResult> ParsedResult { get; set; }
        public string usage { get; }

        public int inputTokens { get; }
        public int outputTokens { get; }
        public double cost { get; }
        public string errorString {  get; }
        public string time { get; set; }
        public string prompt { get; set; }
    }

    public enum GptModel
    {
        gpt_4_1,
        gpt_4_1_mini,
    }
}
