using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FullTextIndex
{
    public class SampleHit
    {
        public string line_number { get; set; }
        public string sample_text { get; set; }
        public string html_highlighting { get; set; }
    }
}
