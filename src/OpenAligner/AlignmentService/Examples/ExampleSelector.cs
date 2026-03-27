using System;
using System.Collections.Generic;
using System.Text;

namespace AdvancedAligner.Examples
{
    public class ExampleSelector
    {
        private readonly List<ExampleAlignment> _examples;
        private HebrewBibleParser hebrewBibleParser;
        public ExampleSelector(ExamplesDatabase exampleDatabase, HebrewBibleParser hebrewBibleParser)
        {
            _examples = exampleDatabase.Examples;
            this.hebrewBibleParser = hebrewBibleParser;
        }

        // Advanced similarity: lemma + POS + morphology aware
        public List<ExampleAlignment> GetBestExamples(List<HebrewFeature> features, int topN = 2)
        {
            List<ExampleAlignment> result = _examples
                .Select(e => new
                {
                    Example = e,
                    Score = ComputeScore(e, features)
                })
                .OrderByDescending(x => x.Score)
                .Take(topN)
                .Select(x => x.Example)
                .ToList();

            // ensure verses are in ascending order
            SortedDictionary<int, ExampleAlignment> sorted = new();
            foreach(var example in result)
            {
                int index = hebrewBibleParser.referenceIndices[example.Reference];
                sorted[index] = example;
            }

            return sorted.Values.ToList();
        }

        private double ComputeScore(ExampleAlignment ex, List<HebrewFeature> features)
        {
            double score = 0;

            foreach (var f in features)
            {
                if (ex.HebrewLemmas.Contains(f.Lemma))
                    score += 2.0; // lemma match (strongest)

                if (ex.POS != null && ex.POS.Contains(f.POS))
                    score += 1.0; // POS match

                if (ex.MorphPatterns != null && ex.MorphPatterns.Any(m => f.Morph.Contains(m)))
                    score += 0.5; // morphology partial match
            }

            return score;
        }
    }

}
