using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FluentNHibernate.Mapping;


namespace DbObjectMappings
{
    public class CorpusInfo
    {
        public virtual int Id { get; set; }
        public virtual string Caption { get; set; }

        public virtual string TxtFilesPath { get; set; }

        public virtual DateTime IndexDate { get; set; }
    }

    public class CorpusInfo_Map : ClassMap<CorpusInfo>
    {
        public CorpusInfo_Map()
        {
            Table("CorpusInfo");
            Id(x => x.Id).GeneratedBy.Native("GENID_CorpusInfo");
            Map(x => x.Caption).Column("Caption");
            Map(x => x.TxtFilesPath).Column("TxtFilesPath");
            Map(x => x.IndexDate).Column("IndexDate");
        }
    }

}
