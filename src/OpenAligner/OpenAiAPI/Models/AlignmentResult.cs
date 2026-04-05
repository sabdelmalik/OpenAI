using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace OpenAiAPI.Models
{
    public class AlignmentResult
    {
        public string reference { get; set; }
        public List<AlignmentPair> alignments { get; set; }


        public void ReplaceHebrew(int target, List<int> hebrew)
        {
            alignments[target].h = hebrew;
        }

        public void Split(int index)
        {
            // if the selected index has only a single target
            // just return
            if (alignments[index].t.Count < 2)
                return;

            // we need to rebuild the alignments
            var newAlignments = new List<AlignmentPair>();
            if (index > 0)
            {
                // copy alignments till we reach the index to split
                for (int i = 0; i < index; i++)
                {
                    newAlignments.Add(alignments[i]);
                }
            }
            //==========SPLIT======
            var a = alignments[index];
            bool hDone = false;
            foreach (var t in a.t)
            {
                var a2 = new AlignmentPair();
                a2.t = new List<int> { t };
                if (!hDone)
                {
                    hDone = true;
                    a2.h = a.h;
                }
                else
                    a2.h = new List<int>();
                newAlignments.Add(a2);
            }
            //========Copy the rest
            for (int i = index + 1; i < alignments.Count; i++)
            {
                newAlignments.Add(alignments[i]);
            }

            alignments = newAlignments;
        }


        public void Merge(int index, int count)
        {
            if(count < 2) return;

            // we need to rebuild the alignments
            var newAlignments = new List<AlignmentPair>();
            if (index > 0)
            {
                // copy alignments till we reach the index to split
                for (int i = 0; i < index; i++)
                {
                    newAlignments.Add(alignments[i]);
                }
            }
            //==========Merge======
            var newPair = new AlignmentPair();
            newPair.t = alignments[index].t;
            newPair.h = alignments[index].h;
            for(int i= index + 1; i < index + count; i++ )
            { 
                newPair.t.AddRange(alignments[i].t);
                newPair.h.AddRange(alignments[i].h);
            }
            newAlignments.Add(newPair);
            //========Copy the rest
            for (int i = index + count; i < alignments.Count; i++)
            {
                newAlignments.Add(alignments[i]);
            }

            alignments = newAlignments;
        } 
    }
}
