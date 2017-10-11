using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FullTextIndex
{
    public interface ICancelIndexation
    {
        bool GetCancellationPending();
        bool Cancelled { get; set; }
    }
}
