using System;
using System.ComponentModel;
using NopImport.Console.Export;

namespace NopImport.Console.Common.Excel
{
    public class WorkSheetFactory
    {
        public event EventHandler<ProgressChangedEventArgs> ExportProgressChanged;

        private readonly dynamic _workbook;

        public WorkSheetFactory(dynamic workbook)
        {
            _workbook = workbook;
        }

        public void CreateWorkSheets()
        {
            var specialityWorkSheet = new ProductWorkSheet(_workbook);
            specialityWorkSheet.ExportProgressChanged += OnExportProgressChanged;
            specialityWorkSheet.Create();

        }

        private void OnExportProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (ExportProgressChanged != null)
            {
                ExportProgressChanged(this, e);
            }
        }
    }
}
