using System;
using System.ComponentModel;
using System.Runtime.Remoting.Messaging;

namespace NopImport.Common
{
    public interface IAsyncController<in T> where T : class
    {
        event EventHandler<AsyncCompletedEventArgs> Completed;
        event EventHandler<ProgressChangedEventArgs> ProgressChanged;

        void RunAsync(T param = null);
        bool IsBusy { get; }
    }

    public abstract class AsyncControllerBase<T> : IAsyncController<T> where T : class
    {
        private delegate void AsyncWorkerDelegate(AsyncOperation async, T param);
        private readonly object _sync = new object();
        private bool _isBusy;
        public event EventHandler<AsyncCompletedEventArgs> Completed;
        public event EventHandler<ProgressChangedEventArgs> ProgressChanged;

        public void RunAsync(T param = null)
        {
            var delInstance = new AsyncWorkerDelegate(DoWork);
            var completedCallback = new AsyncCallback(OnJobCompletedCallBack);

            lock (_sync)
            {
                if (_isBusy)
                    throw new InvalidOperationException("The control is currently busy.");

                var async = AsyncOperationManager.CreateOperation(null);
                delInstance.BeginInvoke(async, param, completedCallback, async);
                _isBusy = true;
            }
        }

        protected void ReportProgress(AsyncOperation async, ProgressChangedEventArgs args)
        {
            async.Post(e => OnProgressChanged(this, (ProgressChangedEventArgs)e), args);
        }

        private void OnProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (ProgressChanged != null)
            {
                ProgressChanged(sender, e);
            }
        }

        private void OnJobCompletedCallBack(IAsyncResult ar)
        {
            var worker = (AsyncWorkerDelegate)((AsyncResult)ar).AsyncDelegate;
            var async = (AsyncOperation)ar.AsyncState;

            worker.EndInvoke(ar);

            lock (_sync)
            {
                _isBusy = false;
            }


            var completedArgs = new AsyncCompletedEventArgs(null, false, null);
            async.PostOperationCompleted(e => OnJobCompleted((AsyncCompletedEventArgs)e), completedArgs);
        }

        private void OnJobCompleted(AsyncCompletedEventArgs e)
        {
            if (Completed != null)
            {
                Completed(this, e);
            }
        }

        protected abstract void DoWork(AsyncOperation async, T fullFilename);

        public bool IsBusy { get { return _isBusy; } }
    }
}
