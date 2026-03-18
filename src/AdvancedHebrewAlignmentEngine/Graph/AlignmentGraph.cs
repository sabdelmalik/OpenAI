using HebrewAlignmentEngineAdvanced;
using System;
using System.Collections.Generic;
using System.Text;

namespace AdvancedHebrewAlignmentEngine.Graph
{
    public class AlignmentGraph
    {
        public List<AlignmentEdge> Edges { get; set; } = new();
        public double Confidence { get; set; }

        public void ComputeConfidence()
        {
            if (Edges.Count == 0) { Confidence = 0; return; }
            Confidence = Edges.Average(e => e.Score);
        }
    }
}
