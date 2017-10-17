using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FluentNHibernate.Mapping;


namespace DbObjectMappings
{
    /// <summary>
    /// Описание корпуса.
    /// </summary>
    public class CorpusInfo
    {
        public virtual int Id { get; set; }

        /// <summary>
        /// Наименование корпуса для отображения в интерфейсе
        /// </summary>
        public virtual string Caption { get; set; }

        /// <summary>
        /// Путь к файлу корпуса или каталогу с документами
        /// </summary>
        public virtual string TxtFilesPath { get; set; }

        /// <summary>
        /// Время последнего индексирования корпуса
        /// </summary>
        public virtual DateTime IndexDate { get; set; }

        /// <summary>
        /// JSON-строка с описанием формата корпуса - кодировки, файл или папка с файлами и т.д.
        /// </summary>
        public virtual string CorpusFormat { get; set; }
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
            Map(x => x.CorpusFormat).Column("CorpusFormat");
        }
    }

}
