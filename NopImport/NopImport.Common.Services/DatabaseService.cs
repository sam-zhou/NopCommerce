using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NopImport.Common.Services.FluentNHibernate;
using NopImport.Model.Data;

namespace NopImport.Common.Services
{
    public class DatabaseService : IDatabaseService
    {
        private readonly string _connectionStringKey;
        private readonly string _assemblyName;
        private ISessionFactory _sessionFactory;

        private ISession _session;

        private ITransaction _transaction;

        protected ISessionFactory SessionFactory
        {
            get
            {
                if (_sessionFactory == null || _sessionFactory.IsClosed)
                {
                    _sessionFactory = NHibernateHelper.CreateSessionFactory(_connectionStringKey, _assemblyName, false);
                }
                return _sessionFactory;
            }
        }

        public ISession Session
        {
            get
            {
                if (_session == null || !_session.IsOpen)
                {
                    _session = SessionFactory.OpenSession();
                }
                return _session;
            }
        }

        public DatabaseService(string connectionStringKey, string assemblyName)
        {
            _connectionStringKey = connectionStringKey;
            _assemblyName = assemblyName;
        }

        public DatabaseService()
        {
            _connectionStringKey = "DefaultConnectionString";
            _assemblyName = "Lynex.BillMaster";
        }

        public void ResetDatabase()
        {
            _sessionFactory = NHibernateHelper.CreateSessionFactory(_connectionStringKey, _assemblyName, true);
        }

        public void BeginTransaction()
        {
            _transaction = Session.BeginTransaction();
        }

        public void CommitTransaction()
        {
            if (_transaction != null)
            {
                _transaction.Commit();
                _transaction.Dispose();
            }
            _transaction = null;
        }

        public void RollBackTransaction()
        {
            if (_transaction != null)
            {
                _transaction.Rollback();
                _transaction.Dispose();
            }
            _transaction = null;
        }


        public TEntity Get<TEntity>(IGetItemQuery<TEntity> itemQuery) 
        {
            return itemQuery.Execute(Session);
        }

        public IEnumerable<TEntity> Get<TEntity>(IGetItemsQuery<TEntity> query) 
        {
            return query.Execute(Session);
        }

        public void ExecuteQuery(IExecuteQuery query)
        {
            query.Execute(Session);
        }

        public TEntity Get<TEntity>(object entityId) where TEntity : class, IDbModel
        {
            return Session.Get<TEntity>(entityId);
        }

        public TEntity Get<TEntity>() where TEntity : class
        {
            return Session.QueryOver<TEntity>().List().FirstOrDefault();
        }

        public IEnumerable<TEntity> GetAll<TEntity>() where TEntity : class
        {
            return Session.QueryOver<TEntity>().List();
        }

        public void Save<TEntity>(TEntity entity) where TEntity : class, IDbModel
        {
            Session.Save(entity);
        }

        public void Delete<TEntity>(TEntity entity) where TEntity : class, IDbModel
        {
            Session.Delete(entity);
        }

        public void Dispose()
        {
            if (_session != null)
            {

                _session.Dispose();
            }

            if (_sessionFactory != null)
            {
                _sessionFactory.Close();
                _sessionFactory.Dispose();
            }
        }
    }
}
