using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace NopImport.Console.Common.Excel
{
    public interface IWorkSheetBase
    {
        event EventHandler<ProgressChangedEventArgs> ExportProgressChanged;
        void Create();
    }

    public abstract class WorkSheetBase : IWorkSheetBase
    {

        protected dynamic Worksheet;

        public event EventHandler<ProgressChangedEventArgs> ExportProgressChanged;



        protected WorkSheetBase(dynamic workbook, bool createNew = true)
        {
            if (createNew)
            {
                Worksheet = workbook.Sheets.Add();
            }
            else
            {
                Worksheet = workbook.Sheets[1];
            }
        }

        public void Create()
        {
            CreateHeader();
            CreateContent();

            Marshal.FinalReleaseComObject(Worksheet);
        }

        protected abstract void CreateHeader();


        protected abstract void CreateContent();

        protected void ProgressChange(int progressPercentage, string msg)
        {

            if (ExportProgressChanged != null)
            {
                var args = new ProgressChangedEventArgs(progressPercentage, msg);
                ExportProgressChanged(this, args);
            }
        }
    }
}
