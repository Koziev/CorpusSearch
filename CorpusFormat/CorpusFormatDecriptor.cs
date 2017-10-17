using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorpusFormat
{
    public static class CorpusFormat_Files
    {
        public static readonly string SINGLE_FILE = "single_file";
    }

    public static class CorpusFormat_Content
    {
        public static readonly string LINES = "lines";
    }

    public static class CorpusFormat_Encoding
    {
        public static readonly string UTF8 = "utf-8";
    }

    public class CorpusFormatDecriptor
    {
        public string files;
        public string content;
        public string encoding;

        public CorpusFormatDecriptor()
        {
            files = CorpusFormat_Files.SINGLE_FILE;
            content = CorpusFormat_Content.LINES;
            encoding = CorpusFormat_Encoding.UTF8;
        }

        public override string ToString()
        {
            return $"format={files}, {content}, {encoding}";
        }

        public string ToJSON()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }

        public static CorpusFormatDecriptor ParseJSON( string json )
        {
            if (string.IsNullOrEmpty(json))
            {
                return new CorpusFormatDecriptor();
            }
            else
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject<CorpusFormatDecriptor>(json);
            }
        }

    }
}
