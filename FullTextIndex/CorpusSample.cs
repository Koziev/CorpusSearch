using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FullTextIndex
{
    public class CorpusSample
    {
        public int LineNumber { get; set; }
        public string LineText { get; set; }
        public float Score { get; set; }
    }
}
