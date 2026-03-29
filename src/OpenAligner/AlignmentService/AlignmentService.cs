
using AdvancedAligner.Examples;
using OpenAiAPI;
using OpenAiAPI.Models;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using TAParser;
using static System.ComponentModel.Design.ObjectSelectorEditor;

namespace AdvancedAligner
{
    public class AlignmentService
    {
        private string StartRef { get; set; } = string.Empty;
        private string EndRef { get; set; } = string.Empty;
        private HebrewBibleParser hebrewBibleParser { get; set; }
        private TahotParser tahotParser { get; set; }
        private TargetParser targetParser { get; set; }
        private ExampleSelector exampleSelector { get; set; }
        private ExamplesDatabase exampleDatabase { get; set; }

        OpenAiService openAiService;

        public AlignmentService(OpenAiService openAiService, 
                                HebrewBibleParser hebrewBibleParser, 
                                TahotParser taParser, 
                                TargetParser targetParser,
                                ExampleSelector exampleSelector,
                                ExamplesDatabase exampleDatabase)
        {
            this.openAiService = openAiService;
            this.hebrewBibleParser = hebrewBibleParser;
            this.tahotParser = taParser;
            this.targetParser = targetParser;
            this.openAiService = openAiService;
            this.exampleSelector = exampleSelector;
            this.exampleDatabase = exampleDatabase;
        }


        public async Task<List<PromptResult>> Align(List<string> refrences, int maxPromptTokens, GptModel model)
        {
            List<PromptResult> result = new List<PromptResult>();
            
            // 1. ensure good parameters
            if (maxPromptTokens < 1 ||
                refrences is null  ||
               refrences.Count == 0)
            {
                return result;
            }

            int remaining = refrences.Count;
            int currentIndex = 0;
            List<int> verses = new List<int>();
            while (remaining > 0)
            {
                string reference = refrences[currentIndex++];
                int verseIndex = hebrewBibleParser.referenceIndices[reference];
                verses.Add(verseIndex);
                remaining--;
                if (verses.Count == maxPromptTokens || remaining == 0)
                {
                    List<CombinedVerse> combinedVerses = GetCombinedVerses(verses);
                    string userPrompt = GetUserPrompt(combinedVerses);
                    var gptResult = await openAiService.AlignVersesAsync(userPrompt, model);
                    result.Add(gptResult);
                    verses.Clear();
                    if (gptResult is null || !gptResult.success)
                    {
                        // 3.3b if result is not success, add result to the list and and terminate the loop
                        break;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Returns a list of aligned results from AI
        /// Each result containes a maximum of "maxPromptTokens" verses
        /// </summary>
        /// <param name="startRef">starting verse reference</param>
        /// <param name="endRef">last verse reference</param>
        /// <param name="maxPromptTokens">maximum verses per GPT prompt</param>
        /// <returns></returns>
        public async Task<List<PromptResult>> Align(string startRef, string endRef, int maxPromptTokens, GptModel model)
        {
            List<PromptResult> result = new List<PromptResult>();
            // 1. ensure good parameters
            if (maxPromptTokens < 1 ||
                string.IsNullOrEmpty(startRef) ||
                string.IsNullOrEmpty(endRef))
            {
                return result;
            }

            // 2. get the indices of the first an last verse
            int startIndex = hebrewBibleParser.referenceIndices[startRef];
            int endIndex = hebrewBibleParser.referenceIndices[endRef];

            if(startIndex < 0 || endIndex < 0 || startIndex > endIndex)
                { return result; }

            // 3. Loop: For each "maxPromptTokens" count, get an array Array of Combined Verses
            
            int remaining = endIndex - startIndex + 1;
            int currentStart = startIndex;
            while (remaining > 0)
            {
                // 3.1 Create a GPT prompt containing the Array of Combined Verses
                int count = 0;
                if (remaining <= maxPromptTokens)
                {
                    count = remaining;
                    remaining = 0;
                }
                else
                {
                    count = maxPromptTokens;
                    remaining = remaining - maxPromptTokens;
                }
                List<int> verses = new List<int>();
                for (int i = currentStart; i < currentStart + count; i++)
                {
                    verses.Add(i);
                }
                currentStart += count;

                List<CombinedVerse> combinedVerses = GetCombinedVerses(verses);
                string userPrompt = GetUserPrompt(combinedVerses);

                // 3.2 use OpenAiService to get the Alignments from GPT as a Prompt result
                var gptResult = await openAiService.AlignVersesAsync(userPrompt, model);

                // 3.3a if result is success add result to the list and repeat 3 until all verses are done
                result.Add(gptResult);
                if (gptResult is null || !gptResult.success)
                {
                    // 3.3b if result is not success, add result to the list and and terminate the loop
                    break;
                }
            }

            return result;
        }

        private string GetUserPrompt(List<CombinedVerse> verses)
        {
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
                WriteIndented = false
            };

            string json = System.Text.Json.JsonSerializer.Serialize(verses,options);

            List<ExampleAlignment> exampleAlignments = null;

            if (exampleSelector != null)
            {
                List<HebrewFeature> features = new();
                foreach (var verse in verses)
                {
                    foreach (var token in verse.hebrew.tokens)
                    {
                        features.Add(new() { Lemma = token.lemma, POS = token.pos, Morph = token.morph });
                    }
                }
                exampleAlignments = exampleSelector.GetBestExamples(features);
            }
            var exampleText = "";

            if (exampleAlignments != null && exampleAlignments.Count > 0)
            {
                List<CombinedVerse> exampleVerses = new List<CombinedVerse>();
                List<AlignmentResult> bestExampleAlignment = new();
                foreach (var ex in exampleAlignments)
                {
                    int index = hebrewBibleParser.referenceIndices[ex.Reference];
                    exampleVerses.Add(GetCompinedVerse(index)) ;
                    bestExampleAlignment.Add(ex.Alignment);
                }

                exampleText = $@"
Complete Examples Start ======
Example Input 
{JsonSerializer.Serialize(exampleVerses, options)}
Example Alignment Response
{JsonSerializer.Serialize(bestExampleAlignment, options)}
Complete Examples End ======
                ";
            }

/*                    string prompt = $@"
You are aligning multiple Biblical Hebrew verses to their English target translations given as Input.
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
*/
                    string prompt = $@"
You are aligning multiple Biblical Hebrew verses to their English target translations given as Input.
The input Hebrew verse is given as a complete text, followed by the same text tokenised. Each token has an index indicating its position in the verse. Each token may represent word, an affix, a conjunction, … Each token includes morphological information to help in the alignment
The input target verse is given as a complete text, followed by the same text tokenised. Each token has an index indicating its position in the verse.

Use the Complete Examples below for Guidance.

Task:
For EACH verse:
Align Hebrew tokens to target tokens. Pay attention to the Hebrew token details: lemma, pos, morphology and gloss.

Rules:
- Align by meaning
- Use token indices
- A Hebrew token index MUST NOT be used more than once in the alignment
- A Target token index MUST NOT be used more than once in the alignment
- Do not combine articles and conjunctions with other words if they have a Hebrew token to align to
- Combine target tokens when it makes morphological sense
- if a target pronoun DOES NOT HAVE a corresponding Hebrew token, but is implied in the Hebrew verb, combine it with the target verb
- Include ALL target words
- Treat Hebrew words connected with a maqqef as separate words (ignore the maqqef)
- The Hebrew אֵת should not be mapped
- Try to always have Hebrew prepositions morph=inseparable_prep, appear in the output mapping
  For example if לָ֭מָּה is represented by tokens 0=לָ֭ and 1=מָּה a corresponding English token ""why"" would have its hebrew indices as [0,1] 

In the output JSON 
- tokens are identified by their indices
- All target indices must appear in the map
- Target indices should appear in the same order as in the input
- If a target token does not have a Hebrew equivalent, its Hebrew indices will be empty []
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

{exampleText}

Input:
{json}
";

            return prompt;
        }


        private List<CombinedVerse> GetCombinedVerses(List<int> verseIndices)
        {
            List<CombinedVerse> verses = new List<CombinedVerse>();

            foreach (int currentIndex in verseIndices)
            {
                verses.Add(GetCompinedVerse(currentIndex));
            }

            return verses;
        }
        
        private CombinedVerse GetCompinedVerse(int index)
        {
            ParserTargetVerse targetVerse = targetParser.TargetBible[index];
            ParserHebrewVerse hebrewVerse = hebrewBibleParser.HebrewBible[index];
            TahotVerse tahotVerse = tahotParser.HebrewBible[index];

            string reference = hebrewVerse.Reference;

            string hebrewText = tahotVerse.VerseText;
            List<HebrewToken> hebrewTokens = new List<HebrewToken>();
            int idx = 0;

            foreach (var hebrewWord in hebrewVerse.Tokens)
            {
                hebrewTokens.Add(new HebrewToken()
                {
                    i = idx++,
                    surface = hebrewWord.surface,
                    lemma = hebrewWord.lemma,
                    pos = hebrewWord.pos,
                    morph = hebrewWord.morph,
                    gloss = hebrewWord.gloss
                }
                );
            }

            string targetText = targetVerse.TranslationText;
            List<OpenAiAPI.Models.TargetToken> targetTokens = new List<OpenAiAPI.Models.TargetToken>();
            idx = 0;
            foreach (var targetwWord in targetVerse.Tokens)
            {
                targetTokens.Add(new OpenAiAPI.Models.TargetToken()
                {
                    i = idx++,
                    word = targetwWord.surface,
                }
                );
            }

            CombinedVerse verse = new CombinedVerse()
            {
                reference = reference,
                hebrew = new HebrewVerse()
                {
                    text = hebrewText,
                    tokens = hebrewTokens
                },
                target = new OpenAiAPI.Models.TargetVerse()
                {
                    text = targetText,
                    tokens = targetTokens
                }
            };

            return verse;
        }

        /// <summary>
        /// Generates a JSON string containing verse tokens for all verses between the specified start and end
        /// references, inclusive.
        /// </summary>
        /// <remarks>The method retrieves verse tokens from both the Hebrew Bible and a version Bible
        /// based on the provided references. The resulting JSON string is also written to a file named
        /// 'promptTokens.json'.</remarks>
        /// <param name="startRef">The reference string that identifies the first verse to include in the token generation.</param>
        /// <param name="endRef">The reference string that identifies the last verse to include in the token generation.</param>
        /// <returns>A JSON string representing a list of verse tokens, each containing the verse reference, Hebrew tokens, and
        /// version tokens.</returns>
        [Obsolete]
        public string BuildPromptOpenAI(string startRef, string endRef)
        {
            List<CombinedVerse> verses = new List<CombinedVerse>();
            
            // 1. get start/end indeces from the hebrewBibleParser referenceIndices dictionary
            int startIndex = hebrewBibleParser.referenceIndices[startRef];
            int endIndex = hebrewBibleParser.referenceIndices[endRef];

            int currentIndex = startIndex;

            while (currentIndex <= endIndex)
            {
                ParserTargetVerse targetVerse = targetParser.TargetBible[currentIndex];
                ParserHebrewVerse hebrewVerse = hebrewBibleParser.HebrewBible[currentIndex];
                TahotVerse tahotVerse = tahotParser.HebrewBible[currentIndex];
                
                string reference = hebrewVerse.Reference;

                string hebrewText = tahotVerse.VerseText;
                List<HebrewToken> hebrewTokens = new List<HebrewToken>();
                int idx = 0;

                foreach (var hebrewWord in hebrewVerse.Tokens)
                {
                    hebrewTokens.Add(new HebrewToken()
                    {
                        i = idx++,
                        surface = hebrewWord.surface,
                        lemma = hebrewWord.lemma,
                        pos = hebrewWord.pos,
                        morph = hebrewWord.morph,
                        gloss = hebrewWord.gloss
                    }
                    );
                }

                string targetText = targetVerse.TranslationText;
                List<OpenAiAPI.Models.TargetToken> targetTokens = new List<OpenAiAPI.Models.TargetToken>();
                idx = 0;
                foreach (var targetwWord in targetVerse.Tokens)
                {
                    targetTokens.Add(new OpenAiAPI.Models.TargetToken()
                    {
                        i = idx++,
                        word = targetwWord.surface,
                    }
                    );
                }

                CombinedVerse verse = new CombinedVerse()
                {
                    reference = reference,
                    hebrew = new HebrewVerse()
                    {
                        text = hebrewText,
                        tokens = hebrewTokens
                    },
                    target = new OpenAiAPI.Models.TargetVerse()
                    {
                        text = targetText,
                        tokens = targetTokens
                    }
                };
                verses.Add(verse);

                currentIndex++;
            }

            // convert the list of HebrewAramaicTokens to a JSON string
            string promptTokensJson = System.Text.Json.JsonSerializer.Serialize(verses, new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
                WriteIndented = false
            });

            File.WriteAllText("promptTokens.json", promptTokensJson);

            return promptTokensJson;
        }

        /// <summary>
        /// Generates a JSON string containing verse tokens for all verses identified in the provided list of references.
        /// Each reference in the list corresponds to a verse, and the method retrieves the verse tokens from both 
        /// the Hebrew Bible and a version Bible based on these references. 
        /// The resulting JSON string is structured to include the verse reference, Hebrew tokens, and version tokens for each verse specified in the input list.
        /// </summary>
        /// <param name="refrences">The list of references to include in the token generation</param>
        /// <returns></returns>
        [Obsolete]
        private string BuildPromptCompact(List<string> refrences)
        {
            List<VerseTokenCompact> verseTokens = new List<VerseTokenCompact>();
            foreach(string reference in refrences)
            {
                int index = hebrewBibleParser.referenceIndices[reference];
                ParserTargetVerse targetVerse = targetParser.TargetBible[index];
                ParserHebrewVerse hebrewVerse = hebrewBibleParser.HebrewBible[index];
                VerseTokenCompact verseToken = new VerseTokenCompact
                {
                    reference = reference,
                    hebrew_tokens = hebrewVerse.CompactTokens,
                    target_tokens = targetVerse.Tokens
                };
                verseTokens.Add(verseToken);
            }
            // convert the list of HebrewAramaicTokens to a JSON string
            string promptTokensJson = System.Text.Json.JsonSerializer.Serialize(verseTokens, new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
                WriteIndented = false
            });
            promptTokensJson = promptTokensJson.Replace("version_tokens", "english_tokens"); // remove zero-width space characters from the JSON string
            File.WriteAllText("promptCompactTokens.json", promptTokensJson);
            return promptTokensJson;

        }

        /// <summary>
        /// Generates a JSON string containing verse tokens for all verses between the specified start and end
        /// references, inclusive.
        /// </summary>
        /// <remarks>The method retrieves verse tokens from both the Hebrew Bible and a version Bible
        /// based on the provided references. The resulting JSON string is also written to a file named
        /// 'promptTokens.json'.</remarks>
        /// <param name="startRef">The reference string that identifies the first verse to include in the token generation.</param>
        /// <param name="endRef">The reference string that identifies the last verse to include in the token generation.</param>
        /// <returns>A JSON string representing a list of verse tokens, each containing the verse reference, Hebrew tokens, and
        /// version tokens.</returns>
        [Obsolete]
        public string BuildPrompt(string startRef, string endRef, bool compact = true)
        {
            if (compact)
            {
                return BuildPromptCompact(startRef, endRef);
            }

            // 1. get start/end indeces from the hebrewBibleParser referenceIndices dictionary
            int startIndex = hebrewBibleParser.referenceIndices[startRef];
            int endIndex = hebrewBibleParser.referenceIndices[endRef];

            int currentIndex = startIndex;

            List<VerseToken> verseTokens = new List<VerseToken>();

            while(currentIndex <= endIndex)
            {
                ParserTargetVerse targetVerse = targetParser.TargetBible[currentIndex];
                ParserHebrewVerse hebrewVerse = hebrewBibleParser.HebrewBible[currentIndex];
                string reference = hebrewVerse.Reference;

                    VerseToken verseToken = new VerseToken
                    {
                        reference = reference,
                        hebrew_tokens = hebrewVerse.Tokens,
                        target_tokens = targetVerse.Tokens
                    };
                verseTokens.Add(verseToken);

                currentIndex++;
            }

            // convert the list of HebrewAramaicTokens to a JSON string
            string promptTokensJson = System.Text.Json.JsonSerializer.Serialize(verseTokens, new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
                WriteIndented = false
            });

            File.WriteAllText("promptTokens.json", promptTokensJson);

            return promptTokensJson;
        }

        [Obsolete]
        private string BuildPromptCompact(string startRef, string endRef)
        {
            // 1. get start/end indeces from the hebrewBibleParser referenceIndices dictionary
            int startIndex = hebrewBibleParser.referenceIndices[startRef];
            int endIndex = hebrewBibleParser.referenceIndices[endRef];
            int currentIndex = startIndex;
            List<VerseTokenCompact> verseTokens = new List<VerseTokenCompact>();
            while(currentIndex <= endIndex)
            {
                ParserTargetVerse targetVerse = targetParser.TargetBible[currentIndex];
                ParserHebrewVerse hebrewVerse = hebrewBibleParser.HebrewBible[currentIndex];
                string reference = hebrewVerse.Reference;
                    VerseTokenCompact verseToken = new VerseTokenCompact
                    {
                        reference = reference,
                        hebrew_tokens = hebrewVerse.CompactTokens,
                        target_tokens = targetVerse.Tokens
                    };
                verseTokens.Add(verseToken);
                currentIndex++;
            }
            // convert the list of HebrewAramaicTokens to a JSON string
            string promptTokensJson = System.Text.Json.JsonSerializer.Serialize(verseTokens, new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
                WriteIndented = false
            });
            File.WriteAllText("promptCompactTokens.json", promptTokensJson);
            return promptTokensJson;
        }

        /// <summary>
        /// Generates a JSON string containing verse tokens for all verses identified in the provided list of references.
        /// Each reference in the list corresponds to a verse, and the method retrieves the verse tokens from both 
        /// the Hebrew Bible and a version Bible based on these references. 
        /// The resulting JSON string is structured to include the verse reference, Hebrew tokens, and version tokens for each verse specified in the input list.
        /// </summary>
        /// <param name="refrences">The list of references to include in the token generation</param>
        /// <returns></returns>
        [Obsolete]
        public string BuildPromptOpenAI(List<string> refrences)
        {
            List<CombinedVerse> verses = new List<CombinedVerse>();
            foreach (string reference in refrences)
            {
                int index = hebrewBibleParser.referenceIndices[reference];
                ParserTargetVerse targetVerse = targetParser.TargetBible[index];
                ParserHebrewVerse hebrewVerse = hebrewBibleParser.HebrewBible[index];
                TahotVerse tahotVerse = tahotParser.HebrewBible[index];

                string hebrewText = tahotVerse.VerseText;
                List<HebrewToken> hebrewTokens = new List<HebrewToken>();
                int idx = 0;
                foreach (var hebrewWord in hebrewVerse.Tokens)
                {
                    hebrewTokens.Add(new HebrewToken()
                    {
                        i = idx++,
                        surface = hebrewWord.surface,
                        lemma = hebrewWord.lemma,
                        pos = hebrewWord.pos,
                        morph = hebrewWord.morph,
                        gloss = hebrewWord.gloss
                    }
                    );
                }

                string targetText = targetVerse.TranslationText;
                List<OpenAiAPI.Models.TargetToken> targetTokens = new List<OpenAiAPI.Models.TargetToken>();
                idx = 0;
                foreach (var targetwWord in targetVerse.Tokens)
                {
                    targetTokens.Add(new OpenAiAPI.Models.TargetToken()
                    {
                        i = idx++,
                        word = targetwWord.surface,
                    }
                    );
                }

                CombinedVerse verse = new CombinedVerse()
                {
                    reference = reference,
                    hebrew = new HebrewVerse()
                    {
                        text = hebrewText,
                        tokens = hebrewTokens
                    },
                    target = new OpenAiAPI.Models.TargetVerse()
                    {
                        text = targetText,
                        tokens = targetTokens
                    }
                };
                verses.Add(verse);
            }

            // convert the list of verses to a JSON string
            string promptTokensJson = System.Text.Json.JsonSerializer.Serialize(verses, new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
                WriteIndented = true
            });

            //promptTokensJson = promptTokensJson.Replace("version_tokens", "english_tokens"); // remove zero-width space characters from the JSON string
            File.WriteAllText("promptTokens.json", promptTokensJson);
            return promptTokensJson;

        }

    }
}
