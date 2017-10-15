using Lucene.Net.Documents;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FullTextIndex
{
    public class Searcher
    {
        private string index_folder;
        private int max_search_hits = 100; // ? вынести в конфиг ?

        public Searcher(string index_folder)
        {
            this.index_folder = index_folder;
        }



        public IEnumerable<SampleHit> Search(string query_str)
        {
            List<SampleHit> result_hits = new List<SampleHit>();

            using (Lucene.Net.Store.Directory luceneIndexDirectory = Lucene.Net.Store.FSDirectory.Open(index_folder))
            {
                Lucene.Net.Analysis.Analyzer analyzer = new Lucene.Net.Analysis.Ru.RussianAnalyzer(Lucene.Net.Util.Version.LUCENE_CURRENT);
                //Lucene.Net.Analysis.Analyzer analyzer = new Lucene.Net.Analysis.Standard.StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_CURRENT);

                using (IndexSearcher searcher = new IndexSearcher(luceneIndexDirectory))
                {
                    QueryParser parser = new QueryParser(Lucene.Net.Util.Version.LUCENE_CURRENT, IndexModel.LineText, analyzer);
                    Query query = parser.Parse(query_str);

                    TopDocs hits = searcher.Search(query, max_search_hits);

                    // code highlighting
                    var formatter = new Lucene.Net.Search.Highlight.SimpleHTMLFormatter("<span style=\"background:yellow;\">", "</span>");
                    var fragmenter = new Lucene.Net.Search.Highlight.SimpleFragmenter(200);
                    Lucene.Net.Search.Highlight.QueryScorer scorer = new Lucene.Net.Search.Highlight.QueryScorer(query);
                    Lucene.Net.Search.Highlight.Highlighter highlighter = new Lucene.Net.Search.Highlight.Highlighter(formatter, scorer);
                    highlighter.TextFragmenter = fragmenter;

                    foreach (ScoreDoc hit in hits.ScoreDocs)
                    {
                        Document doc = searcher.Doc(hit.Doc);
                        float score = hit.Score;

                        Field line_number = doc.GetField(IndexModel.LineNumber);
                        Field line_text = doc.GetField(IndexModel.LineText);

                        Lucene.Net.Analysis.TokenStream stream = analyzer.TokenStream("", new System.IO.StringReader(line_text.StringValue));
                        string highlightedText = highlighter.GetBestFragments(stream, doc.Get(IndexModel.LineText), 1, "...");

                        result_hits.Add(new SampleHit { line_number = line_number.StringValue, sample_text = line_text.StringValue, html_highlighting = highlightedText });
                    }
                }
            }


            return result_hits;
        }
    }
}
