using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvancedAligner
{
    public class AlignmentResult
    {
        public List<HebrewAramaicToken> HebrewTokens { get; set; }
        public List<VersionToken> EnglishTokens { get; set; }
        public List<AlignmentLink> Alignments { get; set; }
    }

}
