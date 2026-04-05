using Google.GenAI;
using Google.GenAI.Types;
using Microsoft.Extensions.Logging;
using OpenAiAPI.Models;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;


namespace OpenAiAPI
{
    /// <summary>
    /// Make OpenAiService a singelton
    /// </summary>
    public class OpenAiService
    {
        private readonly HttpClient _http;
        private readonly Dictionary<string, string> _cache = new();
        private Client geminiClient;

        private readonly ILogger<OpenAiService> logger;
        public OpenAiService(ILogger<OpenAiService> logger)
        {
            this.logger = logger;
            string apiKey = System.Environment.GetEnvironmentVariable("OPENAI_ALIGNER_KEY", EnvironmentVariableTarget.User);
            _http = new HttpClient();
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", apiKey);

            string geminiKey = System.Environment.GetEnvironmentVariable("GEMINI_API_KEY", EnvironmentVariableTarget.User);
            geminiClient = new Client(apiKey: geminiKey);
        }

        //public async Task<List<AlignmentResult>> AlignVersesAsync(object verses, AiModel model)

        public async Task<PromptResult> AlignVersesAsync(string prompt, AiModel model)
        {
            logger.LogInformation("AlignVersesAsync called with model {model}", model);

            string aiModel = model.ToString().Replace("__", ".").Replace("_", "-");

            AiProvider provider = AiProvider.Unknown;
            if (aiModel.StartsWith("gpt"))
                provider = AiProvider.OpenAI;
            else if (aiModel.StartsWith("gemini"))
                provider = AiProvider.Gemini;

            if (provider == AiProvider.Unknown)
            {
                return new PromptResult(false, prompt, string.Empty, 0, 0, aiModel, "AlignVersesAsync: unsupported Model");
            }
            
            if (string.IsNullOrEmpty(prompt))
            {
                return new PromptResult(false, prompt, string.Empty, 0, 0, aiModel, "AlignVersesAsync: input is null");
            }

            Stopwatch sw = Stopwatch.StartNew();

            //            if (_cache.ContainsKey(input))
            //                return ParseResult(_cache[input]);

            //string prompt = BuildPrompt(input);

            PromptResult response = provider switch
            {
                AiProvider.OpenAI => await CallLLM(prompt, aiModel),
                AiProvider.Gemini => await CallGemini(prompt, aiModel),
            };

            if (!IsValidJson(response.result))
            {
                // retry twice
                for (int i = 0; i < 2; i++)
                {
                    response = provider switch
                    {
                        AiProvider.OpenAI => await CallLLM(prompt, aiModel),
                        AiProvider.Gemini => await CallGemini(prompt, aiModel),
                    };

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

        private async Task<PromptResult> CallGemini(string prompt, string model)
        {
            logger.LogInformation("Calling Gemini API with model {model}", model);

            bool success = true;
            StringBuilder errorString = new StringBuilder();

            string output = "{}"; // return empty JSON on error
            int inputTokens = 0;
            int outputTokens = 0;

            PromptResult promptResult;
            int thoughtsBudgetPerVerse = 2000;
            var thinkingConfig = new ThinkingConfig()
            {
                IncludeThoughts = true,
                ThinkingBudget = 20000,
                ThinkingLevel = ThinkingLevel.High
            };
    
            var config = new GenerateContentConfig()
            {
                Temperature = 0,
                MaxOutputTokens = 20000
            };
            GenerateContentResponse response = null;
            Candidate candidate = null;
            while (true) 
            {
                try
                {
                    response = await geminiClient.Models.GenerateContentAsync(
                        model: model,
                        contents: prompt
                    );

                    logger.LogInformation("Gemini API call completed. Processing response...");
                    // 1. Check if there are any candidates at all
                    if (response.Candidates == null || response.Candidates.Count == 0)
                    {
                        success = false;
                        string error = "No response was generated by the model.";
                        errorString.AppendLine(error);
                        logger.LogError(error);
                    }
                    candidate = response.Candidates[0];
                    // 2. Check the FinishReason
                    // FinishReason.Stop is the only "Success" state where the model finished naturally.
                    if (candidate.FinishReason == FinishReason.Stop)
                    {
                        //Token generation reached a natural stopping point or a configured stop sequence.
                        // extract all information from the candidate
                        var metadata = response.UsageMetadata;
                        int? promptTokenCount = metadata.PromptTokenCount; // The total number of tokens in the prompt 
                        int? candidatesTokenCount = metadata.CandidatesTokenCount; // the total number of tokens in the generated candidates
                        int? thoughtsTokenCount = metadata.ThoughtsTokenCount; // output only. The number of tokens that were part of the model's generated "thoughts" output if applicable.  
                        int? x = metadata.TotalTokenCount; // promptTokenCount + candidatesTokenCount + thoughtsTokenCount
                        int promptCount = metadata.PromptTokensDetails.Count;
                        inputTokens = promptTokenCount ?? 0;
                        outputTokens = thoughtsTokenCount ?? 0;
                        outputTokens += candidatesTokenCount ?? 0;
                        output = response.Text;
                        promptResult = new PromptResult(success, prompt, output, inputTokens, outputTokens, model, errorString.ToString());
                        logger.LogInformation("Gemini API call successful. Prompt tokens: {inputTokens}, Output tokens: {outputTokens}", inputTokens, outputTokens);
                        break;
                    }
                    else if (candidate.FinishReason == FinishReason.MaxTokens)
                    {
                        success = false;
                        string error = "Token generation reached the configured maximum output tokens.";
                        errorString.AppendLine(error);
                        logger.LogError(error);
                        promptResult = new PromptResult(success, prompt, output, inputTokens, outputTokens, model, errorString.ToString());
                        break;
                    }
                    else
                    {
                        success = false;
                        string error = $"Model response finished with reason: {candidate.FinishReason}";
                        errorString.AppendLine(error);
                        logger.LogError(error);
                        promptResult = new PromptResult(success, prompt, output, inputTokens, outputTokens, model, errorString.ToString());
                        break;
                    }

                }
                catch (HttpRequestException ex)
                {
                    success = false;
                    string error = "HTTP error calling Gemini API:" + ex.Message;
                    errorString.AppendLine(error);
                    logger.LogCritical(error, ex.ToString());
                    promptResult = new PromptResult(success, prompt, output, inputTokens, outputTokens, model, errorString.ToString());
                    break;
                }
                catch (Exception ex)
                {
                    success = false;
                    if (ex.Message.Contains("429") || ex.Message.Contains("Quota"))
                    {
                        string error = "Rate limit exceeded or quota reached when calling Gemini API: " + ex.Message;
                        errorString.AppendLine(error);
                        logger.LogWarning(error);
                        // we need to keep trying until we get a valid response, so we will not break here. Instead, we will wait for a few seconds and try again.
                    }
                    else if (ex.Message.Contains("500") || ex.Message.Contains("503"))
                    {
                        string error = "Server error when calling Gemini API: " + ex.Message;
                        errorString.AppendLine(error);
                        logger.LogError(error);
                    }
                    else if (ex.Message.Contains("404)"))
                    {
                        string error = "Model not found when calling Gemini API: " + ex.Message;
                        errorString.AppendLine(error);
                        logger.LogError(error);
                    }
                    else
                    {
                        string error = "Exception calling Gemini API: " + ex.Message;
                        errorString.AppendLine(error);
                        logger.LogCritical(error, ex.ToString());
                    }
                    promptResult = new PromptResult(success, prompt, output, inputTokens, outputTokens, model, errorString.ToString());
                    break;
                }
            }

            return promptResult;
        }

        private async Task<PromptResult> CallLLM(string prompt, string model)
        {
            bool success = true;
            StringBuilder errorString = new StringBuilder();

            string output = "{}"; // return empty JSON on error
            int inputTokens = 0;
            int outputTokens = 0;

            if (model == "")
            {
                errorString.Append("Invalid Model");
                success = false;
            }
            else
            {
                var body = new
                {
                    model = model,
                    input = new[]
                    {
                    new { role = "user", content = prompt }
                },
                    temperature = 0,
                    max_output_tokens = 20000
                };

                var json = JsonSerializer.Serialize(body);
                int retries = 5;
                int delay = 1000;
                System.Net.Http.HttpResponseMessage response = null;
                for (int i = 0; i < retries; i++)
                {
                    response = await _http.PostAsync(
                        "https://api.openai.com/v1/responses",
                        new StringContent(json, Encoding.UTF8, "application/json"));

                    if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                    {
                        await Task.Delay(delay);
                        delay *= 2; // exponential backoff
                    }
                    else
                        break;
                }

                if (response != null)
                {
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
            }
            return new PromptResult(success, prompt, output, inputTokens, outputTokens, model, errorString.ToString());

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
        private double InputCostPer1K =  0.0004;
        private double OutputCostPer1K = 0.0016;

        public PromptResult(bool success,string prompt, string result, int inputTokens, int outputTokens, string model, string errorString)    
        {
            this.success = success;
            this.prompt = prompt;
            this.result = result;
            this.inputTokens = inputTokens;
            this.outputTokens = outputTokens;

            InputCostPer1K = model switch
            {
                "gpt-4.1-mini" => 0.0004,
                "gpt-4.1" => 0.002,
                "gpt-5.1-mini" => 0.00075,
                "gpt-5.1" => 0.0025,
                "gemini-2.5-pro" => 0.00125,
                "gemini-2.5-flash" => 0.0003,
                _ => 0.0
            };
            OutputCostPer1K = model switch
            {
                "gpt-4.1-mini" => 0.0016,
                "gpt-4.1" => 0.008,
                "gpt-5.1-mini" => 0.0045,
                "gpt-5.1" => 0.015,
                "gemini-2.5-pro" => 0.010,
                "gemini-2.5-flash" => 0.0025,
                _ => 0.0
            };


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

    public enum AiModel
    {
        gpt_4__1,
        gpt_4__1_mini,
        gpt_5__1,
        gpt_5__1_mini,
        gemini_2__5_pro,
        gemini_2__5_flash,
        gemini_3_flash_preview,
        gemini_3__1_pro_preview
    }

    public enum AiProvider
    {
        Unknown,
        OpenAI,
        Gemini
    }
}
