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
            }
        }

        private void btDeleteCorpus_Click(object sender, RoutedEventArgs e)
        {
            ((Corpora_ViewModel)DataContext).DeleteSelectedCorpus();
        }

        private BusyWithIndexing busy_with_indexing;
        private void btReindexCorpus_Click(object sender, RoutedEventArgs e)
        {
            indexer_worker.RunWorkerAsync(DataContext);
            busy_with_indexing = new BusyWithIndexing();
            busy_with_indexing.Owner = this;
            busy_with_indexing.ShowDialog();
        }

        private void btSearch_Click(object sender, RoutedEventArgs e)
        {
            ((Corpora_ViewModel)DataContext).Search();
        }


        private void indexer_worker_DoWork(object sender, DoWorkEventArgs e)
        {
            Corpora_ViewModel vm = (Corpora_ViewModel)e.Argument;
            vm.ReindexSelectedCorpus();
        }

        private void indexer_worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs args)
        {
            busy_with_indexing.Close();
            busy_with_indexing = null;
        }

    }
}
