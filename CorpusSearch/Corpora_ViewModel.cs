﻿using System;
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
    class Corpora_ViewModel : INotifyPropertyChanged
    {
        private UserConfigManager user_config_manager;

        public Corpora_ViewModel(UserConfigManager user_config_manager)
        {
            Contract.Ensures(this.user_config_manager != null);
            this.user_config_manager = user_config_manager;
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


        private int selected_tab_index = 0;
        public int SelectedTabIndex
        {
            get
            {
                return selected_tab_index;
            }

            set
            {
                selected_tab_index = value;
                NotifyPropertyChanged();
                StoreConfig();
            }
        }



        private ObservableCollection<DbObjectMappings.CorpusInfo> corpus_infos = new ObservableCollection<DbObjectMappings.CorpusInfo>();


        private string indexes_folder;
        public void SetIndexesFolder(string path)
        {
            Contract.Ensures(!string.IsNullOrEmpty(path));
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


        public void AddCorpus(string corpus_caption, string txt_files, CorpusFormat.CorpusFormatDecriptor corpus_format, bool do_index)
        {
            if (string.IsNullOrEmpty(corpus_caption))
            {
                throw new ArgumentException("corpus_caption");
            }

            if (string.IsNullOrEmpty(txt_files))
            {
                throw new ArgumentException("txt_files");
            }

            if( corpus_format==null )
            {
                throw new ArgumentNullException("corpus_format");
            }

            log4net.ILog log = log4net.LogManager.GetLogger(typeof(Corpora_ViewModel));
            log.InfoFormat("{0} corpus_caption={1} txt_files={1} format={2} do_index={3}", nameof(AddCorpus), corpus_caption, txt_files, corpus_format, do_index);

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
                        new_corpus.CorpusFormat = corpus_format.ToJSON();

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
                StoreConfig();
            }
        }

        private string GetIndexFolder(DbObjectMappings.CorpusInfo corpus)
        {
            Contract.Requires(!string.IsNullOrEmpty(indexes_folder));
            return System.IO.Path.Combine(indexes_folder, corpus.Id.ToString());
        }

        public void ReindexSelectedCorpus(BackgroundWorker worker, Indexation_ViewModel indexation_vm, out bool cancelled)
        {
            cancelled = false;
            log4net.ILog log = log4net.LogManager.GetLogger(typeof(Corpora_ViewModel));
            if (selected_corpus != null)
            {
                try
                {
                    log.InfoFormat("Start indexing the corpus id={0} caption={1}", selected_corpus.Id, selected_corpus.Caption);

                    indexation_vm.IndexingProgress = "Indexation of " + selected_corpus.Caption;

                    FullTextIndex.CorpusIndexer indexer = new FullTextIndex.CorpusIndexer();

                    CancelIndexationFromWorker cancellation = new CancelIndexationFromWorker(worker);
                    IndexationProgressWithWorker progress = new IndexationProgressWithWorker(worker);

                    indexer.BuildIndex(GetIndexFolder(selected_corpus),
                        selected_corpus.TxtFilesPath,
                        CorpusFormat.CorpusFormatDecriptor.ParseJSON( selected_corpus.CorpusFormat ),
                        cancellation,
                        progress);
                    cancelled = cancellation.Cancelled;

                    using (var db_session = session_factory.OpenSession())
                    {
                        using (var tx = db_session.BeginTransaction())
                        {
                            selected_corpus.IndexDate = DateTime.Now;
                            db_session.Update(selected_corpus);

                            tx.Commit();

                            log.Info("tx.Commit OK");

                            PropertyChanged(this, new PropertyChangedEventArgs("CorpusInfos"));
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

        private string search_hits_html = "<html></html>";
        public string SearchHitsHtml
        {
            get
            {
                return search_hits_html;
            }

            set
            {
                search_hits_html = value;
                NotifyPropertyChanged();
            }
        }


        public void Search()
        {
            if (SelectedCorpusForSearching != null && !string.IsNullOrEmpty(QueryStr))
            {
                search_hits.Clear();
                FullTextIndex.Searcher searcher = new FullTextIndex.Searcher(GetIndexFolder(SelectedCorpusForSearching));
                var hits = searcher.Search(QueryStr);
                System.Text.StringBuilder html = new System.Text.StringBuilder();

                html.Append("<html>");
                html.Append("<head>");
                html.Append("<meta charset='utf-8'>");
                html.Append("</head>");
                html.Append("<body>");

                foreach (var hit in hits)
                {
                    search_hits.Add(hit);

                    html.Append("<p>");
                    html.AppendFormat("{0}", hit.html_highlighting);
                    html.Append("</p>");
                }

                html.Append("</body>");
                html.Append("</html>");

                SearchHitsHtml = html.ToString();
            }
        }



        private void StoreConfig()
        {
            Contract.Requires(user_config_manager != null);

            user_config_manager["selected_corpus_id_for_searching"] = SelectedCorpusForSearching.Id.ToString();
            user_config_manager["selected_tab_index"] = SelectedTabIndex;
            
            user_config_manager.Store();

            return;
        }


        public void LoadConfig()
        {
            Contract.Requires(user_config_manager != null);

            string selected_corpus_id_for_searching = (string)user_config_manager["selected_corpus_id_for_searching"];

            if (!string.IsNullOrEmpty(selected_corpus_id_for_searching))
            {
                var selected_corpus = CorpusInfos.Where(z => z.Id.ToString() == selected_corpus_id_for_searching).FirstOrDefault();
                if (selected_corpus != null)
                {
                    SelectedCorpusForSearching = selected_corpus;
                }

                SelectedTabIndex = (int)(long)user_config_manager["selected_tab_index"];
            }

            return;
        }

    }
}
