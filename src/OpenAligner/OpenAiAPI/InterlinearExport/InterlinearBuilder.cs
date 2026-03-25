using OpenAiAPI.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAiAPI.InterlinearExport
{
    public class InterlinearBuilder
    {
        public static string Print(AlignmentResult result, dynamic verse)
        {
            StringBuilder sb = new StringBuilder();

            foreach (var align in result.alignments)
            {
                sb.Append("Hebrew: ");
                foreach (var i in align.h)
                    sb.Append(verse.hebrew.tokens[i].surface + " ");

                sb.Append(" -> English: ");
                foreach (var i in align.t)
                    sb.Append(verse.target.tokens[i].word + " ");

                sb.AppendLine();
            }
            return sb.ToString();
        }
    }
}
