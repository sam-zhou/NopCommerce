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


namespace NopImport.UrlSearcher
{
    class Program
    {


        static void Main(string[] args)
        {
                    
            var reader = new CWProductListReader();
            reader.ProgressChanged += ReaderOnProgressChanged;
            var list = reader.Process();

            
            Console.WriteLine("Writing To Db : ");
            using (var db = new DatabaseService("DefaultConnectionString", "NopImport"))
            {
                db.ResetDatabase();

                

                for (int i = 0; i < list.Count; i++)
                {
                    if (!db.Get(new IsProductByExist(list[i].Url)))
                    {
                        var product = list[i];
                        db.Save(product);
                    }

                    
                    Console.Write("Completed: \r{0}%  {1}/{2}", (i + 1) * 100 / list.Count, i + 1, list.Count);
                }

            }
            Console.WriteLine("Done! ");



            
            Console.ReadLine();
        }

        private static void ReaderOnProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Console.Write("Completed: \r{0}% ", e.ProgressPercentage);
        }
    }
}
