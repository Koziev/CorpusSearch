using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CorpusSearch
{
    public class Indexation_ViewModel : INotifyPropertyChanged
    {
        private BackgroundWorker index_worker;

        public Indexation_ViewModel(BackgroundWorker index_worker)
        {
            this.index_worker = index_worker;
        }

        public void AbortIndexationWorker()
        {
            if (index_worker.WorkerSupportsCancellation == true)
            {
                index_worker.CancelAsync();
            }
        }


        private string indexing_progress;
        public string IndexingProgress
        {
            get
            {
                return indexing_progress;
            }

            set
            {
                indexing_progress = value;
                NotifyPropertyChanged();
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

    }
}
