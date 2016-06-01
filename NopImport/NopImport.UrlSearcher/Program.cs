using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using NopImport.Common.Services;
using NopImport.Common.Services.Queries;
using NopImport.Model;
using NopImport.Model.Common;
using NopImport.Model.SearchModel;
using NopImport.UrlSearcher.ChemistWarehouse;
using NopImport.UrlSearcher.Common;


namespace NopImport.UrlSearcher
{
    internal class Program
    {


        private static void Main(string[] args)
        {



            //GetList(true);
            GetDetails();
            Console.WriteLine("Done!");
            Console.ReadLine();
        }

        private static void GetDetails()
        {
            Console.WriteLine("GetDetails ");
            var reader = new CWProductReader();
            reader.ProgressChanged += ReaderOnProgressChanged;
            
            reader.Process();
        }


        private static void GetList(bool resetDb = false)
        {
            Console.WriteLine("GetList");
            var reader = new CWProductListReader(resetDb);
            reader.ProgressChanged += ReaderOnProgressChanged;
            
            reader.Process();

            
            
        }

        private static void ReaderOnProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Console.Write("Completed: \r{0}% ", e.ProgressPercentage);
        }
    }
}
