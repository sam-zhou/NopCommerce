using System.ComponentModel;
using NopImport.Console.ChemistWarehouse;
using NopImport.Console.Export;

namespace NopImport.Console
{
    internal class Program
    {


        private static void Main(string[] args)
        {



            // GetList(true);
            // GetDetails();
            RunExport();
            System.Console.WriteLine("All Done!");
            System.Console.ReadLine();
        }

        private static void GetDetails()
        {
            System.Console.WriteLine("GetDetails        ");
            var reader = new CWProductReader();
            reader.ProgressChanged += ReaderOnProgressChanged;
            
            reader.Process();
        }


        private static void GetList(bool resetDb = false)
        {
            System.Console.WriteLine("GetList           ");
            var reader = new CWProductListReader(resetDb);
            reader.ProgressChanged += ReaderOnProgressChanged;
            
            reader.Process();

            
            
        }

        private static void ReaderOnProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            System.Console.Write("Completed: \r{0}% ", e.ProgressPercentage);
        }


        static void RunExport()
        {
            System.Console.WriteLine("RunExport            ");
            var exporter = new ExcelExporter();
            exporter.ProgressChanged += ExporterOnProgressChanged;
            exporter.Completed += ExporterOnCompleted;
            exporter.RunAsync("d:\\products.xlsx");
            System.Console.ReadLine();
        }

        private static void ExporterOnCompleted(object sender, AsyncCompletedEventArgs asyncCompletedEventArgs)
        {
            System.Console.WriteLine("Export Completed !         ");
        }

        private static void ExporterOnProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            System.Console.Write("\r {0}% Completed", e.ProgressPercentage);
        }
    }
}
