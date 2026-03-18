using System;
using System.Collections.Generic;
using System.Text;

namespace AdvancedHebrewAlignmentEngine.Graph
{
    public class AlignmentEdge
    {
        public string Source { get; set; }
        public string Target { get; set; }
        public double Score { get; set; }
    }
}
