using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorpusSearch
{
    class CancelIndexationFromWorker : FullTextIndex.ICancelIndexation
    {
        private BackgroundWorker worker;
        public CancelIndexationFromWorker(BackgroundWorker worker )
        {
            Contract.Ensures(this.worker != null);
            this.worker = worker;
        }

        public bool GetCancellationPending()
        {
            Contract.Invariant(worker != null);
            return worker.CancellationPending;
        }

        public bool Cancelled { get; set; }
    }
}
