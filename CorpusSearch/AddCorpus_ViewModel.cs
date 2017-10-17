using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CorpusSearch
{
    public class AddCorpus_ViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public Dictionary<string,string> Encodings
        {
            get
            {
                Dictionary<string, string> x = new Dictionary<string, string>();
                x.Add("utf-8", "utf-8");
                return x;
            }
        }

    }
}
