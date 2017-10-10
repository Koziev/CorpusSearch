using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CorpusSearch
{
    class Corpora_ViewModel
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private ObservableCollection<DbObjectMappings.CorpusInfo> corpus_infos = new ObservableCollection<DbObjectMappings.CorpusInfo>();


        private string indexes_folder;
        public void SetIndexesFolder( string path )
        {
            Contract.Ensures( !string.IsNullOrEmpty(path) );
            indexes_folder = path;
        }

        private NHibernate.ISessionFactory session_factory;
        public void ConnectDB(NHibernate.ISessionFactory session_factory)
        {
            this.session_factory = session_factory;

            using (var db_session = session_factory.OpenSession())
            {
                using (var tx = db_session.BeginTransaction())
                {
                    var crit = db_session.CreateCriteria<DbObjectMappings.CorpusInfo>();
                    //.Add(NHibernate.Criterion.Expression.Eq("Status", 1));
                    var corpora = crit.List<DbObjectMappings.CorpusInfo>();

                    foreach (var corpus_info in corpora)
                    {
                        corpus_infos.Add(corpus_info);
                    }
                }
            }
        }


        public void AddCorpus(string corpus_caption, string txt_files, bool do_index)
        {
            if (string.IsNullOrEmpty(corpus_caption))
            {
                throw new ArgumentException("corpus_caption");
            }

            if (string.IsNullOrEmpty(txt_files))
            {
                throw new ArgumentException("txt_files");
            }

            log4net.ILog log = log4net.LogManager.GetLogger(typeof(Corpora_ViewModel));
            log.InfoFormat("{0} corpus_caption={1} txt_files={1} do_index={2}", nameof(AddCorpus), corpus_caption, txt_files, do_index);

            //db_session.Clear();

            try
            {
                using (var db_session = session_factory.OpenSession())
                {
                    using (var tx = db_session.BeginTransaction())
                    {
                        DbObjectMappings.CorpusInfo new_corpus = new DbObjectMappings.CorpusInfo();
                        new_corpus.Caption = corpus_caption;
                        new_corpus.TxtFilesPath = txt_files;

                        db_session.Save(new_corpus);

                        tx.Commit();

                        log.Info("tx.Commit OK");

                        corpus_infos.Add(new_corpus);
                        SelectedCorpus = new_corpus;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }

            return;
        }


        public ObservableCollection<DbObjectMappings.CorpusInfo> CorpusInfos
        {
            get
            {
                return corpus_infos;
            }

            set
            {
                corpus_infos = value;
                NotifyPropertyChanged();
            }
        }

        private DbObjectMappings.CorpusInfo selected_corpus;
        public DbObjectMappings.CorpusInfo SelectedCorpus
        {
            get { return selected_corpus; }
            set
            {
                selected_corpus = value;
            }
        }

        private DbObjectMappings.CorpusInfo selected_corpus_for_searching;
        public DbObjectMappings.CorpusInfo SelectedCorpusForSearching
        {
            get { return selected_corpus_for_searching; }
            set
            {
                selected_corpus_for_searching = value;
            }
        }

        private string GetIndexFolder( DbObjectMappings.CorpusInfo corpus )
        {
            Contract.Requires(!string.IsNullOrEmpty(indexes_folder));
            return System.IO.Path.Combine(indexes_folder, corpus.Id.ToString());
        }

        public void ReindexSelectedCorpus()
        {
            log4net.ILog log = log4net.LogManager.GetLogger(typeof(Corpora_ViewModel));
            if (selected_corpus != null)
            {
                try
                {
                    log.InfoFormat("Start indexing the corpus id={0} caption={1}", selected_corpus.Id, selected_corpus.Caption);

                    FullTextIndex.CorpusIndexer indexer = new FullTextIndex.CorpusIndexer();
                    // todo: запускать в фоне
                    indexer.BuildIndex(GetIndexFolder(selected_corpus), selected_corpus.TxtFilesPath);


                    using (var db_session = session_factory.OpenSession())
                    {
                        using (var tx = db_session.BeginTransaction())
                        {
                            selected_corpus.IndexDate = DateTime.Now;
                            db_session.Update(selected_corpus);

                            tx.Commit();

                            log.Info("tx.Commit OK");

                            //PropertyChanged(this, new PropertyChangedEventArgs("CorpusInfos"));
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.Error(ex);
                }
            }

        }

        public void DeleteSelectedCorpus()
        {
            log4net.ILog log = log4net.LogManager.GetLogger(typeof(Corpora_ViewModel));
            if (selected_corpus != null)
            {
                try
                {
                    using (var db_session = session_factory.OpenSession())
                    {
                        using (var tx = db_session.BeginTransaction())
                        {
                            log.InfoFormat("Deleting corpus id={0} caption={1}", selected_corpus.Id, selected_corpus.Caption);
                            db_session.Delete(selected_corpus);

                            tx.Commit();

                            log.Info("tx.Commit OK");

                            corpus_infos.Remove(selected_corpus);
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.Error(ex);
                }
            }
        }


        private string query_str;
        public string QueryStr
        {
            get
            {
                return query_str;
            }

            set
            {
                query_str = value;
            }

        }


        private ObservableCollection<FullTextIndex.SampleHit> search_hits = new ObservableCollection<FullTextIndex.SampleHit>();
        public ObservableCollection<FullTextIndex.SampleHit> SearchHits
        {
            get
            {
                return search_hits;
            }
        }

        public void Search()
        {
            if( SelectedCorpusForSearching!=null && !string.IsNullOrEmpty(QueryStr) )
            {
                search_hits.Clear();
                FullTextIndex.Searcher searcher = new FullTextIndex.Searcher(GetIndexFolder(SelectedCorpusForSearching));
                var hits = searcher.Search(QueryStr);
                foreach( var hit in hits )
                {
                    search_hits.Add(hit);
                }
            }
        }
    }
}
