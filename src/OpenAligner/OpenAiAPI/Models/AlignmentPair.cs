using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace OpenAiAPI.Models
{
    public class AlignmentPair
    {
        public List<int> t { get; set; }
        public List<int> h { get; set; }
        [JsonIgnore]
        public string notes { get; set; }

    }

}
