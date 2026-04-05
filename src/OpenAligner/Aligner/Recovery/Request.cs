using System;
using System.Collections.Generic;
using System.Text;

namespace AdvancedAligner.Recovery
{
    public class Request
    {
        public Contents[] contents { get; set; }
    }
}

public class Contents
{
    public Parts[] parts { get; set; }
    public string role { get; set; }    
}

public class Parts
{
    public string text { get; set; }
}

