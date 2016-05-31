using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;
using NopImport.Common.Services.DefaultDataFactory;
using NopImport.Model;
using NopImport.Model.Map;

namespace NopImport.Common.Services.FluentNHibernate
{
    internal static class NHibernateHelper
    {
        public static ISessionFactory CreateSessionFactory(string connectionStringKey, string assemblyNameSpace, bool createNew)
        {
            var modelAssembly = Assembly.Load(assemblyNameSpace + ".Model");
            Assembly databaseAssembly = null;
            try
            {
                databaseAssembly = Assembly.Load(assemblyNameSpace + ".Database");
            }
            catch (Exception)
            {
                //ignore
            }

            var configuration = BuildConfiguration(connectionStringKey, modelAssembly);
            if (createNew)
            {
                configuration.ExposeConfiguration(CreateSchemaExport);
            }

            var sessionFactory = configuration.BuildSessionFactory();

            if (createNew && databaseAssembly != null)
            {
                PopulateDefaultData(sessionFactory, modelAssembly, databaseAssembly);
            }

            return sessionFactory;
        }

        private static FluentConfiguration BuildConfiguration(string connectionStringKey, Assembly assembly)
        {
            var configuration = Fluently.Configure()
                .Database(
                    MsSqlConfiguration.MsSql2012
                        .ConnectionString(q => q.FromConnectionStringWithKey(connectionStringKey)))
                .Mappings(m =>
                    m.FluentMappings
                    .AddFromAssembly(assembly)
                    .AddFromAssembly(CreateGenericClassMappingAssembly(assembly)));
#if DEBUG || TRACE
            configuration.ExposeConfiguration(SetInterceptors);
#endif

            return configuration;
        }

        private static Assembly CreateGenericClassMappingAssembly(Assembly modelAssembly)
        {
            var types = new List<DynamicTypeInfo>();
            var enumTypes = modelAssembly.GetMapableEnumTypes();
            foreach (var enumType in enumTypes)
            {
                var parentBase = typeof (EnumTableMap<>);
                var parent = parentBase.MakeGenericType(enumType);
                types.Add(new DynamicTypeInfo(enumType.Name + "Map", parent));
            }

            return DynamicClassHelper.CreateDynamicAssembly(modelAssembly.GetName().Name + ".Dynamic", types);
        }

        private static void SetInterceptors(Configuration cfg)
        {
            cfg.SetInterceptor(new SqlStatementInterceptor());
        }

        private static void CreateSchemaExport(Configuration cfg)
        {
            var schemaExport = new SchemaExport(cfg);
            schemaExport.Create(true, true);
        }

        private static void PopulateDefaultData(ISessionFactory sessionFactory, Assembly modelAssembly, Assembly databaseAssembly = null)
        {
            using (var session = sessionFactory.OpenSession())
            {
                session.Transaction.Begin();
                var types =
                    Assembly.GetExecutingAssembly().GetTypes().Where(type => typeof(IDefaultDataFactory).IsAssignableFrom(type) && !type.IsAbstract).ToList();

                if (databaseAssembly != null)
                {
                    types.AddRange(databaseAssembly.GetTypes().Where(type => typeof(IDefaultDataFactory).IsAssignableFrom(type) && !type.IsAbstract).ToList());
                }
                

                foreach (var type in types)
                {
                    var instance = Activator.CreateInstance(type, session, modelAssembly) as IDefaultDataFactory;
                    if (instance != null)
                    {
                        instance.Populate();
                    }
                }
                session.Transaction.Commit();
            }
        }
    }
}
