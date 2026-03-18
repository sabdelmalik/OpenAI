
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace AdvancedAligner
{
    public class UserPrompt
    {
        private string StartRef { get; set; } = string.Empty;
        private string EndRef { get; set; } = string.Empty;
        private HebrewBibleParser HebrewBibleParser { get; set; }
        private VersionParser VersionParser { get; set; }
        
        public UserPrompt( HebrewBibleParser hebrewBibleParser, VersionParser versionParser)
        {
            HebrewBibleParser = hebrewBibleParser;
            VersionParser = versionParser;
        }

        /// <summary>
        /// Generates a JSON string containing verse tokens for all verses identified in the provided list of references.
        /// Each reference in the list corresponds to a verse, and the method retrieves the verse tokens from both 
        /// the Hebrew Bible and a version Bible based on these references. 
        /// The resulting JSON string is structured to include the verse reference, Hebrew tokens, and version tokens for each verse specified in the input list.
        /// </summary>
        /// <param name="refrences">The list of references to include in the token generation</param>
        /// <returns></returns>
        public string BuildPrompt(List<string> refrences, bool compact = true)
        {
            if(compact)
            {
                return BuildPromptCompact(refrences);
            }
            List<VerseToken> verseTokens = new List<VerseToken>();
            foreach(string reference in refrences)
            {
                int index = HebrewBibleParser.referenceIndices[reference];
                VersionVerse versionVerse = VersionParser.VersionBible[index];
                OtVerse hebrewVerse = HebrewBibleParser.HebrewBible[index];
                VerseToken verseToken = new VerseToken
                {
                    reference = reference,
                    hebrew_tokens = hebrewVerse.Tokens,
                    version_tokens = versionVerse.Tokens
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
            File.WriteAllText("promptTokens.json", promptTokensJson);
            return promptTokensJson;

        }

        private string BuildPromptCompact(List<string> refrences)
        {
            List<VerseTokenCompact> verseTokens = new List<VerseTokenCompact>();
            foreach(string reference in refrences)
            {
                int index = HebrewBibleParser.referenceIndices[reference];
                VersionVerse versionVerse = VersionParser.VersionBible[index];
                OtVerse hebrewVerse = HebrewBibleParser.HebrewBible[index];
                VerseTokenCompact verseToken = new VerseTokenCompact
                {
                    reference = reference,
                    hebrew_tokens = hebrewVerse.CompactTokens,
                    version_tokens = versionVerse.Tokens
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
        public string BuildPrompt(string startRef, string endRef, bool compact = true)
        {
            if (compact)
            {
                return BuildPromptCompact(startRef, endRef);
            }

            // 1. get start/end indeces from the HebrewBibleParser referenceIndices dictionary
            int startIndex = HebrewBibleParser.referenceIndices[startRef];
            int endIndex = HebrewBibleParser.referenceIndices[endRef];

            int currentIndex = startIndex;

            List<VerseToken> verseTokens = new List<VerseToken>();

            while(currentIndex <= endIndex)
            {
                VersionVerse versionVerse = VersionParser.VersionBible[currentIndex];
                OtVerse hebrewVerse = HebrewBibleParser.HebrewBible[currentIndex];
                string reference = hebrewVerse.Reference;

                    VerseToken verseToken = new VerseToken
                    {
                        reference = reference,
                        hebrew_tokens = hebrewVerse.Tokens,
                        version_tokens = versionVerse.Tokens
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

        private string BuildPromptCompact(string startRef, string endRef)
        {
            // 1. get start/end indeces from the HebrewBibleParser referenceIndices dictionary
            int startIndex = HebrewBibleParser.referenceIndices[startRef];
            int endIndex = HebrewBibleParser.referenceIndices[endRef];
            int currentIndex = startIndex;
            List<VerseTokenCompact> verseTokens = new List<VerseTokenCompact>();
            while(currentIndex <= endIndex)
            {
                VersionVerse versionVerse = VersionParser.VersionBible[currentIndex];
                OtVerse hebrewVerse = HebrewBibleParser.HebrewBible[currentIndex];
                string reference = hebrewVerse.Reference;
                    VerseTokenCompact verseToken = new VerseTokenCompact
                    {
                        reference = reference,
                        hebrew_tokens = hebrewVerse.CompactTokens,
                        version_tokens = versionVerse.Tokens
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

        public List<HebrewAramaicToken> BuildVersionTokens(OtVerse verse)
        {
            List<HebrewAramaicToken> result = new List<HebrewAramaicToken>();


            return result;
        }

        public List<VersionToken> BuildVersionTokens(VersionVerse verse)
        {
            List<VersionToken> result = new List<VersionToken>();


            return result;
        }

        private string userPrompt = $@"";

    }
}
