using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace AdvancedHebrewAlignmentEngine
{

    /// <summary>
    /// gpt-5.1 is a powerful model that can be used for complex tasks like morphological analysis, 
    /// but it can be expensive to use for large volumes of text.
    /// input           $2.50 per 1M tokens
    /// cached input    $0.25 per 1M tokens
    /// output          $15.50 per 1M tokens
    ///
    /// gpt-5.1-mini is a smaller, more cost-effective model that can be used for tasks that don't require the full capabilities of gpt-5.1.
    /// input           $0.75 per 1M tokens
    /// cached input    $0.075 per 1M tokens
    /// output          $4.50 per 1M tokens
    /// 
    /// gpt-4.1 is a powerful model that can be used for complex tasks like morphological analysis,
    /// input           $3.00 per 1M tokens
    /// cached input    $0.75 per 1M tokens
    /// output          $12.00 per 1M tokens
    /// training        $25.00 per 1M tokens
    /// 
    /// gpt-4.1-mini is a cheaper variant of gpt-4.1, 
    /// good for simple disambiguation tasks. Adjust as needed based on cost/performance tradeoff.
    /// input           $0.80 per 1M tokens
    /// cached input    $0.20 per 1M tokens
    /// output          $3.20 per 1M tokens
    /// training        $5.00 per 1M tokens
    /// </summary>
    internal class TestLLM
    {
        private readonly string _apiKey;
        private readonly HttpClient _httpClient;

        public TestLLM(string apiKey)
        {
            _apiKey = apiKey;
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);
        }

        private async Task<string> AlignAsync()
        {
            var prompt = GetPrompt(@"בְּ רֵאשִׁ֖ית בָּרָ֣א אֱלֹהִ֑ים אֵ֥ת הַ שָּׁמַ֖יִם וְ אֵ֥ת הָ אָֽרֶץ");

            var requestBody = new
            {
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

        }

        public string GetPrompt(string verses)
        {
            return $@"Given the following Hebrew verses, provide a detailed morphological analysis for each word, including its lemma, part of speech, and any relevant morphological features. The analysis should be structured in a clear and organized manner, allowing for easy comparison between the original Hebrew text and the morphological breakdown. Here are the verses:\n\n{verses}";
        }
    }
}
