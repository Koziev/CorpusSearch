using FluentNHibernate.Cfg;
using NHibernate;
using NHibernate.Cfg;
//using NHibernate.Mapping.Attributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace CorpusSearch
{
    public static class NHibernateHelper
    {
        public static ISessionFactory CreateSessionFactory()
        {
            return Fluently.Configure()
              .Database(
                FluentNHibernate.Cfg.Db.SQLiteConfiguration.Standard
                  .UsingFile("e:/tmp/CorpusSearch.sqlite.db")
                  .ShowSql()
              )
              .Mappings(m => m.FluentMappings.AddFromAssemblyOf<DbObjectMappings.CorpusInfo>())
              .BuildSessionFactory();
        }
    }
}