using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CorpusSearch
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly BackgroundWorker indexer_worker = new BackgroundWorker();


        public MainWindow()
        {
            InitializeComponent();

            log4net.ILog log = log4net.LogManager.GetLogger(typeof(MainWindow));
            log.InfoFormat("MainWindow ctor");

            try
            {
                var vm = new Corpora_ViewModel();
                vm.SetIndexesFolder(System.Configuration.ConfigurationManager.AppSettings["indexes"]);
                vm.ConnectDB(NHibernateHelper.CreateSessionFactory());
                DataContext = vm;
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }

            indexer_worker.RunWorkerCompleted += indexer_worker_RunWorkerCompleted;
            indexer_worker.DoWork += indexer_worker_DoWork;
            indexer_worker.ProgressChanged += indexer_worker_ProgressChanged;

            indexer_worker.WorkerSupportsCancellation = true;
            indexer_worker.WorkerReportsProgress = true;
        }

        private void btAddCorpora_Click(object sender, RoutedEventArgs e)
        {
            var w = new AddCorpusWindow();
            bool? res = w.ShowDialog();
            if( res.HasValue && res.Value )
            {
                string corpus_caption = w.tbCaption.Text.Trim();
                string txt_files = w.tbtxtFile.Text.Trim();
                bool do_index = w.chbIndex.IsChecked.Value;

                ((Corpora_ViewModel)DataContext).AddCorpus(corpus_caption, txt_files, do_index);

                if( do_index)
                {
                    btReindexCorpus_Click(sender, e);
                }
            }
        }

        private void btDeleteCorpus_Click(object sender, RoutedEventArgs e)
        {
            ((Corpora_ViewModel)DataContext).DeleteSelectedCorpus();
        }

        private BusyWithIndexing busy_with_indexing;
        private void btReindexCorpus_Click(object sender, RoutedEventArgs e)
        {
            Indexation_ViewModel indexation_vm = new Indexation_ViewModel(indexer_worker);

            indexer_worker.RunWorkerAsync( new Tuple<Corpora_ViewModel, Indexation_ViewModel>((Corpora_ViewModel)DataContext, indexation_vm) );

            busy_with_indexing = new BusyWithIndexing(indexation_vm);
            busy_with_indexing.Owner = this;
            busy_with_indexing.ShowDialog();
        }




        private void btSearch_Click(object sender, RoutedEventArgs e)
        {
            ((Corpora_ViewModel)DataContext).Search();
        }


        private void indexer_worker_DoWork(object sender, DoWorkEventArgs e)
        {
            Tuple<Corpora_ViewModel, Indexation_ViewModel> view_models = (Tuple<Corpora_ViewModel, Indexation_ViewModel>)e.Argument;

            bool cancelled;
            view_models.Item1.ReindexSelectedCorpus((BackgroundWorker)sender, view_models.Item2, out cancelled);
            if (cancelled)
            {
                e.Cancel = true;
            }
        }

        private void indexer_worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs args)
        {
            busy_with_indexing.Close();
            busy_with_indexing = null;
        }

        private void indexer_worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if(busy_with_indexing!=null)
            {
                ((Indexation_ViewModel)busy_with_indexing.DataContext).IndexingProgress = string.Format("Indexing, {0}% completed", e.ProgressPercentage );
            }
        }

    }
}
