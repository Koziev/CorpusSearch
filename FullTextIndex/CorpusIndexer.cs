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

        public void BuildIndex(string index_folder, string corpus_file_path)
        {
            if (System.IO.Directory.Exists(index_folder))
            {
                System.IO.Directory.Delete(index_folder, true);
            }

            using (Lucene.Net.Store.Directory luceneIndexDirectory = Lucene.Net.Store.FSDirectory.Open(index_folder))
            {
                Lucene.Net.Analysis.Analyzer analyzer = new Lucene.Net.Analysis.Ru.RussianAnalyzer(Lucene.Net.Util.Version.LUCENE_CURRENT);
                //Lucene.Net.Analysis.Analyzer analyzer = new Lucene.Net.Analysis.Standard.StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_CURRENT);

                using (Lucene.Net.Index.IndexWriter writer = new Lucene.Net.Index.IndexWriter(luceneIndexDirectory, analyzer, Lucene.Net.Index.IndexWriter.MaxFieldLength.UNLIMITED))
                {
                    CorpusFileReader rdr = new CorpusFileReader();

                    foreach (var sampleDataFileRow in rdr.ReadAllLines(corpus_file_path))
                    {
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
