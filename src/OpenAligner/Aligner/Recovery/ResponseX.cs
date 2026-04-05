using System;
using System.Collections.Generic;
using System.Text;

namespace AdvancedAligner.Recovery
{
    public class ResponseX
    {
        public CandidatesX[] candidates { get; set; }
        public UsageMetadata usageMetadata { get; set; }
    }

    public class CandidatesX
    {
        public Contents content { get; set; }
        public string finishReason { get; set; }
    }

    public class UsageMetadata
    {
        /// <summary>
        /// Charged at the model's input token price.
        /// </summary>
        public int promptTokenCount { get; set; }
        /// <summary>
        /// charged at the model's output token price.
        /// </summary>
        public int candidatesTokenCount { get; set; }
        public int totalTokenCount { get; set; }
        public PromptTokensDetails[] promptTokensDetails { get; set; }
        /// <summary>
        /// charged at the model's output token price.
        /// </summary>
        public int thoughtsTokenCount { get; set; }
    }

    public class PromptTokensDetails
    {
        public string modality { get; set; }
        public int tokenCount { get; set; }
    }

}
