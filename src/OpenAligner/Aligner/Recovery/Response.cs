using System;
using System.Collections.Generic;
using System.Text;

namespace AdvancedAligner.Recovery
{
    public class Response
    {
        public Candidates[] candidates { get; set; }
    }

    public class Candidates
    {
        public Contents content { get; set; }
    }



}
