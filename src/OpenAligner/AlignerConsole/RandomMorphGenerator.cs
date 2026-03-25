using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvancedAligner
{
    public static class RandomMorphGenerator
    {
        static Random rng = new Random();

        public static string RandomMorph()
        {
            int choice = rng.Next(0, 6);

            return choice switch
            {
                0 => RandomVerb(),
                1 => RandomNoun(),
                2 => RandomAdjective(),
                3 => RandomPronoun(),
                4 => RandomParticle(),
                5 => RandomPrefix(),
                _ => "X"
            };
        }

        private static string RandomVerb()
        {
            string[] stems = { "q", "n", "p", "P", "h", "H", "t" };
            string[] conj = { "q", "i", "p", "w" }; // perfect, imperfect, participle, waw-consec
            string[] persons = { "1", "2", "3" };
            string[] genders = { "m", "f", "c" };
            string[] numbers = { "s", "p" };

            return "V" +
                stems[rng.Next(stems.Length)] +
                conj[rng.Next(conj.Length)] +
                persons[rng.Next(persons.Length)] +
                genders[rng.Next(genders.Length)] +
                numbers[rng.Next(numbers.Length)];
        }

        private static string RandomNoun()
        {
            string[] classes = { "c", "p" }; // common, proper
            string[] genders = { "m", "f", "c" };
            string[] numbers = { "s", "p", "d" };
            string[] states = { "a", "c", "e" }; // absolute, construct, emphatic

            return "N" +
                classes[rng.Next(classes.Length)] +
                genders[rng.Next(genders.Length)] +
                numbers[rng.Next(numbers.Length)] +
                states[rng.Next(states.Length)];
        }

        private static string RandomAdjective()
        {
            string[] genders = { "m", "f", "c" };
            string[] numbers = { "s", "p" };
            string[] states = { "a", "c", "e" };

            return "A" +
                genders[rng.Next(genders.Length)] +
                numbers[rng.Next(numbers.Length)] +
                states[rng.Next(states.Length)];
        }

        private static string RandomPronoun()
        {
            string[] types = { "Pp", "Pr", "Pd" }; // suffix, independent, demonstrative
            string[] persons = { "1", "2", "3" };
            string[] genders = { "m", "f", "c" };
            string[] numbers = { "s", "p" };

            return types[rng.Next(types.Length)] +
                persons[rng.Next(persons.Length)] +
                genders[rng.Next(genders.Length)] +
                numbers[rng.Next(numbers.Length)];
        }

        private static string RandomParticle()
        {
            return "T";
        }

        private static string RandomPrefix()
        {
            string[] prefixes = { "C", "R" }; // conjunction, preposition
            return prefixes[rng.Next(prefixes.Length)];
        }
    }
}
