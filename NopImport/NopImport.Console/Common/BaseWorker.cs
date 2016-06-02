using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NopImport.Console.Common
{
    public abstract class BaseWorker
    {
        public event EventHandler<ProgressChangedEventArgs> ProgressChanged;


        protected void ChangeProgress(int percentage)
        {
            if (ProgressChanged != null)
            {
                ProgressChanged(this, new ProgressChangedEventArgs(percentage, null));
            }
        }

        public abstract void Process();
    }
}
