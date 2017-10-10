using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FullTextIndex
{
    public class CorpusFileReader
    {
        public IEnumerable<CorpusSample> ReadAllLines( string corpus_file_path)
        {
            using (System.IO.StreamReader rdr = new StreamReader(corpus_file_path))
            {
                int line_count = 0;
                while( !rdr.EndOfStream)
                {
                    line_count++;
                    string line = rdr.ReadLine();
                    if(line==null)
                    {
                        break;
                    }
                    else
                    {
                        yield return new CorpusSample{ LineNumber=line_count, LineText=line };
                    }
                }
            }
        }
    }
}
