using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FullTextIndex
{
    public class CorpusIndexer
    {
        public CorpusIndexer()
        {
        }

        public void BuildIndex(string index_folder,
            string corpus_file_path,
            CorpusFormat.CorpusFormatDecriptor corpus_format,
            ICancelIndexation cancellation, IShowIndexationProgress progress )
        {
            if (string.IsNullOrEmpty(index_folder))
            {
                throw new ArgumentException("index_folder");
            }

            if (string.IsNullOrEmpty(corpus_file_path))
            {
                throw new ArgumentException("corpus_file_path");
            }

            // todo: добавить учет параметров corpus_format, вероятно через фабрику.


            if (!System.IO.File.Exists(corpus_file_path))
            {
                throw new ApplicationException($"File {corpus_file_path} does not exists");
            }

            // Очистим папку с индексной информацией от предыдущего индексирования.
            if (System.IO.Directory.Exists(index_folder))
            {
                System.IO.Directory.Delete(index_folder, true);
            }

            // Для оценки прогресса индексирования большого файла нам нужно заранее получить число строк в нем,
            // чем мы сейчас и займемся в лоб.
            // TODO: для оптимизации можно читать байты блоками и искать \n
            int total_lines = 0;
            using (System.IO.StreamReader rdr0 = new System.IO.StreamReader(corpus_file_path))
            {
                while(!rdr0.EndOfStream)
                {
                    rdr0.ReadLine();
                    total_lines += 1;
                }
            }


            using (Lucene.Net.Store.Directory luceneIndexDirectory = Lucene.Net.Store.FSDirectory.Open(index_folder))
            {
                Lucene.Net.Analysis.Analyzer analyzer = new Lucene.Net.Analysis.Ru.RussianAnalyzer(Lucene.Net.Util.Version.LUCENE_CURRENT);
                //Lucene.Net.Analysis.Analyzer analyzer = new Lucene.Net.Analysis.Standard.StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_CURRENT);

                using (Lucene.Net.Index.IndexWriter writer = new Lucene.Net.Index.IndexWriter(luceneIndexDirectory, analyzer, Lucene.Net.Index.IndexWriter.MaxFieldLength.UNLIMITED))
                {
                    CorpusFileReader rdr = new CorpusFileReader();

                    int line_counter = 0;
                    foreach (var sampleDataFileRow in rdr.ReadAllLines(corpus_file_path))
                    {
                        line_counter++;

                        if (cancellation.GetCancellationPending())
                        {
                            cancellation.Cancelled = true;
                            break;
                        }

                        if( (line_counter%100000)==0 )
                        {
                            int percentage = (int)Math.Round( (100.0 * line_counter) / total_lines );
                            progress.ShowProgress(percentage);
                        }

                        Lucene.Net.Documents.Document doc = new Lucene.Net.Documents.Document();

                        doc.Add(new Lucene.Net.Documents.Field(IndexModel.LineNumber,
                        sampleDataFileRow.LineNumber.ToString(),
                        Lucene.Net.Documents.Field.Store.YES,
                        Lucene.Net.Documents.Field.Index.NO));

                        doc.Add(new Lucene.Net.Documents.Field(IndexModel.LineText,
                        sampleDataFileRow.LineText,
                        Lucene.Net.Documents.Field.Store.YES,
                        Lucene.Net.Documents.Field.Index.ANALYZED));

                        writer.AddDocument(doc);
                    }

                    writer.Optimize();
                    writer.Flush(true, true, true);
                }
            }
        }
    }
}
