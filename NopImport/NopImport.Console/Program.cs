using System.ComponentModel;
using Nop.Core.Infrastructure;
using Nop.Services.Catalog;
using NopImport.Console.ChemistWarehouse;
using NopImport.Console.Export;
using NopImport.Console.Import;
using NopImport.GoogleTranslate;
using SevenSpikes.Nop.Plugins.NopQuickTabs.Domain;
using SevenSpikes.Nop.Plugins.NopQuickTabs.Services;

namespace NopImport.Console
{
    internal class Program
    {


        private static void Main(string[] args)
        {

            //GetList();
            //GetDetails();
            //RunExport();
            ImportToNop();
            System.Console.WriteLine("All Done!");
            System.Console.ReadLine();
        }

        private static void GetDetails()
        {
            System.Console.WriteLine("GetDetails        ");
            var reader = new CWProductHtmlReader();
            reader.ProgressChanged += OnProgressChanged;
            reader.Process();
        }


        private static void GetList(bool resetDb = false)
        {
            System.Console.WriteLine("GetList           ");
            var reader = new CWProductListHtmlReader(resetDb);
            reader.ProgressChanged += OnProgressChanged;
            reader.Process();

            
            
        }

        static void RunExport()
        {
            System.Console.WriteLine("RunExport            ");
            var exporter = new ExcelExporter();
            exporter.ProgressChanged += OnProgressChanged;
            exporter.Completed += ExporterOnCompleted;
            exporter.RunAsync("d:\\products.xlsx");
            System.Console.ReadLine();
        }

        private static void ExporterOnCompleted(object sender, AsyncCompletedEventArgs asyncCompletedEventArgs)
        {
            System.Console.WriteLine("Export Completed !         ");
        }

        private static void OnProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            System.Console.Write("\r{0}% Completed", e.ProgressPercentage);
        }


        static void ImportToNop()
        {
            System.Console.WriteLine("ImportToNop              ");
            var linker = new NopLinker();
            linker.ProgressChanged += OnProgressChanged;
            //var output = linker.IsProductExists("CW", "40649");
            //System.Console.WriteLine(output);
            linker.Process();
        }
    }
}
