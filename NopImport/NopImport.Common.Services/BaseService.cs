using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NopImport.Common.Services
{
    public abstract class BaseService
    {
        protected BaseService(IDatabaseService dbService)
        {
            DatabaseService = dbService;
        }
        protected IDatabaseService DatabaseService { get; set; }

        public void BeginTransaction()
        {
            DatabaseService.BeginTransaction();
        }

        public void CommitTransaction()
        {
            DatabaseService.CommitTransaction();
        }

        public void RollBackTransaction()
        {
            DatabaseService.RollBackTransaction();
        }

        protected void SingleTransactionAction<TArg1>(Action<TArg1> method, TArg1 arg1)
        {
            try
            {
                BeginTransaction();
                method(arg1);
                CommitTransaction();
            }
            catch (Exception)
            {
                RollBackTransaction();
                throw;
            }
        }

        protected void SingleTransactionAction<TArg1, TArg2>(Action<TArg1, TArg2> method, TArg1 arg1, TArg2 arg2)
        {
            try
            {
                BeginTransaction();
                method(arg1, arg2);
                CommitTransaction();
            }
            catch (Exception)
            {
                RollBackTransaction();
                throw;
            }
        }

        protected void SingleTransactionAction<TArg1, TArg2, TArg3>(Action<TArg1, TArg2, TArg3> method, TArg1 arg1, TArg2 arg2, TArg3 arg3)
        {
            try
            {
                BeginTransaction();
                method(arg1, arg2, arg3);
                CommitTransaction();
            }
            catch (Exception)
            {
                RollBackTransaction();
                throw;
            }
        }

        protected TResult SingleTransactionOperation<TArg1, TResult>(Func<TArg1, TResult> method, TArg1 arg1)
        {
            try
            {
                BeginTransaction();
                var result = method(arg1);
                CommitTransaction();
                return result;
            }
            catch (Exception)
            {
                RollBackTransaction();
                throw;
            }
        }

        protected TResult SingleTransactionOperation<TArg1, TArg2, TResult>(Func<TArg1, TArg2, TResult> method, TArg1 arg1, TArg2 arg2)
        {
            try
            {
                BeginTransaction();
                var result = method(arg1, arg2);
                CommitTransaction();
                return result;
            }
            catch (Exception)
            {
                RollBackTransaction();
                throw;
            }
        }

        protected TResult SingleTransactionOperation<TArg1, TArg2, TArg3, TResult>(Func<TArg1, TArg2, TArg3, TResult> method, TArg1 arg1, TArg2 arg2, TArg3 arg3)
        {
            try
            {
                BeginTransaction();
                var result = method(arg1, arg2, arg3);
                CommitTransaction();
                return result;
            }
            catch (Exception)
            {
                RollBackTransaction();
                throw;
            }
        }

    }
}
