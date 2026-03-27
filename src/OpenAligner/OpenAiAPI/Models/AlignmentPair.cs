using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAiAPI.Models
{
    public class AlignmentPair
    {
        public List<int> t { get; set; }
        public List<int> h { get; set; }
        public string notes { get; set; }

    }

}
