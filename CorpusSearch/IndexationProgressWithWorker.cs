using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorpusSearch
{
    public class IndexationProgressWithWorker : FullTextIndex.IShowIndexationProgress
    {
        private BackgroundWorker worker;
        public IndexationProgressWithWorker(BackgroundWorker worker)
        {
            Contract.Ensures(this.worker != null);
            this.worker = worker;
        }


        public void ShowProgress(int percentage)
        {
            Contract.Invariant(worker != null);
            worker.ReportProgress(percentage);
        }
    }
}
