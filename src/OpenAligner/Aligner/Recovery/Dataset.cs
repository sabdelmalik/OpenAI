using System;
using System.Collections.Generic;
using System.Text;

namespace AdvancedAligner.Recovery
{
    public class Dataset
    {
        public Request request { get; set; }
        public Response[] response { get; set; }
        public string turnId { get; set; }
        public string[] datasetIds { get; set; }
        public string createTime { get; set; }
        public object responseStatus { get; set; }
        public string apiSource { get; set; }
    }
}
