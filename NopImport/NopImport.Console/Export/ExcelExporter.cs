using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using NopImport.Common;
using NopImport.Console.Common.Excel;

namespace NopImport.Console.Export
{
    public interface IExcelExporter : IAsyncController<string>
    {
        bool IsExecelAvailable { get; }
    }

    public class ExcelExporter : AsyncControllerBase<string>, IExcelExporter
    {
        private static readonly string ExcelAssemblyPath = AssemblyLoader.GetAssemblyPath("Microsoft.Office.Interop.Excel");
        private static Assembly _excelAssembly;

        public bool IsExecelAvailable
        {
            get
            {
                return ExcelAssemblyPath != null;
            }
        }

        public static Assembly ExcelAssembly
        {
            get
            {
                if (_excelAssembly == null)
                {
                    _excelAssembly = Assembly.LoadFrom(ExcelAssemblyPath);
                }
                return _excelAssembly;
            }
        }

        protected override void DoWork(AsyncOperation async, string fullFilename)
        {
            var type = ExcelAssembly.GetType("Microsoft.Office.Interop.Excel.ApplicationClass");
            var xlFileFormatType = ExcelAssembly.GetType("Microsoft.Office.Interop.Excel.XlFileFormat");
            var xlSaveAsAccessMode = ExcelAssembly.GetType("Microsoft.Office.Interop.Excel.XlSaveAsAccessMode");

            dynamic excelApp = Activator.CreateInstance(type);
            excelApp.DisplayAlerts = false;
            var workbooks = excelApp.Workbooks;
            var workbook = workbooks.Open(fullFilename);

            var workSheetFactory = new WorkSheetFactory(workbook);
            workSheetFactory.ExportProgressChanged += (o, args) => ReportProgress(async, args);
            workSheetFactory.CreateWorkSheets();

            //var count = workbook.Worksheets.Count;
            //for (int i = count; i > count - 3; i--)
            //{
            //    workbook.Worksheets[i].Delete();
            //}

            var saved = false;
            while (!saved)
            {
                try
                {
                    var basePath = Path.GetDirectoryName(fullFilename);



                    var extension = Path.GetExtension(fullFilename);

                    var newFilename = Guid.NewGuid().ToString("N") + extension;

                    workbook.SaveAs(Path.Combine(basePath, newFilename), (dynamic)Enum.Parse(xlFileFormatType, "xlOpenXMLWorkbook"), Missing.Value,
                    Missing.Value, Missing.Value, Missing.Value,
                     (dynamic)Enum.Parse(xlSaveAsAccessMode, "xlExclusive"), Missing.Value, Missing.Value,
                    Missing.Value, Missing.Value, Missing.Value);
                    saved = true;
                }
                catch (Exception ex)
                {
                    saved = true;
                    System.Console.WriteLine(ex.Message);
                }
            }

            // Cleanup:
            GC.Collect();
            GC.WaitForPendingFinalizers();


            workbook.Close(false);

            excelApp.Quit();
        }

        
    }
}
