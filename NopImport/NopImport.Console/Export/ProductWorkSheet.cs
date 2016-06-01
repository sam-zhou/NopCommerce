using NopImport.Common;
using NopImport.Common.Services;
using NopImport.Console.Common.Excel;
using NopImport.Model.Data;

namespace NopImport.Console.Export
{
    public class ProductWorkSheet : WorkSheetBase
    {
        public ProductWorkSheet(dynamic workbook)
            : base((object)workbook, false)
        {
            ProgressChange(0, "Gathering test data");
        }

        protected override void CreateHeader()
        {
            
        }

        protected override void CreateContent()
        {
            using (var db = new DatabaseService("DefaultConnectionString", "NopImport"))
            {
                var products = db.Session.QueryOver<Product>().Where(q => q.IsUpdated && !q.IsSynced).List();
                for (int i = 0; i < products.Count; i++)
                {
                    var product = products[i];
                    Worksheet.Cells[i + 2, 1] = "5";
                    Worksheet.Cells[i + 2, 2] = "0";
                    Worksheet.Cells[i + 2, 3] = "TRUE";
                    Worksheet.Cells[i + 2, 4] = product.Name;
                    Worksheet.Cells[i + 2, 5] = product.MetaDescription;
                    Worksheet.Cells[i + 2, 6] = product.GeneralInfo;
                    Worksheet.Cells[i + 2, 7] = "0";
                    Worksheet.Cells[i + 2, 8] = "1";
                    Worksheet.Cells[i + 2, 9] = "FALSE";
                    Worksheet.Cells[i + 2, 10] = product.MetaKeywords;
                    Worksheet.Cells[i + 2, 11] = product.MetaDescription;
                    Worksheet.Cells[i + 2, 12] = product.Name;
                    Worksheet.Cells[i + 2, 13] = StringExtension.GenerateSlug(product.Name);
                    Worksheet.Cells[i + 2, 14] = "TRUE";
                    Worksheet.Cells[i + 2, 15] = "TRUE";
                    Worksheet.Cells[i + 2, 16] = "CW-" + product.ExternalId;
                    Worksheet.Cells[i + 2, 17] = "";
                    Worksheet.Cells[i + 2, 18] = "";
                    Worksheet.Cells[i + 2, 19] = "FALSE";
                    Worksheet.Cells[i + 2, 20] = "0";
                    Worksheet.Cells[i + 2, 21] = "";
                    Worksheet.Cells[i + 2, 22] = "FALSE";
                    Worksheet.Cells[i + 2, 23] = "";
                    Worksheet.Cells[i + 2, 24] = "FALSE";
                    Worksheet.Cells[i + 2, 25] = "FALSE";
                    Worksheet.Cells[i + 2, 26] = "0";
                    Worksheet.Cells[i + 2, 27] = "TRUE";
                    Worksheet.Cells[i + 2, 28] = "10";
                    Worksheet.Cells[i + 2, 29] = "1";
                    Worksheet.Cells[i + 2, 30] = "FALSE";


                    Worksheet.Cells[i + 2, 31] = "0";
                    Worksheet.Cells[i + 2, 32] = "FALSE";
                    Worksheet.Cells[i + 2, 33] = "";
                    Worksheet.Cells[i + 2, 34] = "FALSE";
                    Worksheet.Cells[i + 2, 35] = "100";
                    Worksheet.Cells[i + 2, 36] = "0";
                    Worksheet.Cells[i + 2, 37] = "10";
                    Worksheet.Cells[i + 2, 38] = "FALSE";
                    Worksheet.Cells[i + 2, 39] = "1";
                    Worksheet.Cells[i + 2, 40] = "0";
                    Worksheet.Cells[i + 2, 41] = "TRUE";
                    Worksheet.Cells[i + 2, 42] = "FALSE";
                    Worksheet.Cells[i + 2, 43] = "FALSE";
                    Worksheet.Cells[i + 2, 44] = "0";
                    Worksheet.Cells[i + 2, 45] = "0";
                    Worksheet.Cells[i + 2, 46] = "FALSE";
                    Worksheet.Cells[i + 2, 47] = "0";
                    Worksheet.Cells[i + 2, 48] = "FALSE";
                    Worksheet.Cells[i + 2, 49] = "1"; //Manage Inventory Method Id

                    Worksheet.Cells[i + 2, 50] = "FALSE";
                    Worksheet.Cells[i + 2, 51] = "0";
                    Worksheet.Cells[i + 2, 52] = "10";  //Stock Qty
                    Worksheet.Cells[i + 2, 53] = "TRUE";
                    Worksheet.Cells[i + 2, 54] = "FALSE";
                    Worksheet.Cells[i + 2, 55] = "0";
                    Worksheet.Cells[i + 2, 56] = "0";
                    Worksheet.Cells[i + 2, 57] = "1";
                    Worksheet.Cells[i + 2, 58] = "0";
                    Worksheet.Cells[i + 2, 59] = "FALSE";

                    Worksheet.Cells[i + 2, 60] = "1";
                    Worksheet.Cells[i + 2, 61] = "10000";
                    Worksheet.Cells[i + 2, 62] = "";
                    Worksheet.Cells[i + 2, 63] = "FALSE";
                    Worksheet.Cells[i + 2, 64] = "FALSE";
                    Worksheet.Cells[i + 2, 65] = "FALSE";
                    Worksheet.Cells[i + 2, 66] = "FALSE";
                    Worksheet.Cells[i + 2, 67] = "";
                    Worksheet.Cells[i + 2, 68] = "FALSE";
                    Worksheet.Cells[i + 2, 69] = product.Price;

                    Worksheet.Cells[i + 2, 70] = product.OriginalPrice;
                    Worksheet.Cells[i + 2, 71] = "0";
                    Worksheet.Cells[i + 2, 72] = "";
                    Worksheet.Cells[i + 2, 73] = "";
                    Worksheet.Cells[i + 2, 74] = "";
                    Worksheet.Cells[i + 2, 75] = "FALSE";
                    Worksheet.Cells[i + 2, 76] = "0";
                    Worksheet.Cells[i + 2, 77] = "1000";
                    Worksheet.Cells[i + 2, 78] = "FALSE";
                    Worksheet.Cells[i + 2, 79] = "0";

                    Worksheet.Cells[i + 2, 80] = "1";
                    Worksheet.Cells[i + 2, 81] = "0";
                    Worksheet.Cells[i + 2, 82] = "1";
                    Worksheet.Cells[i + 2, 83] = "FALSE";
                    Worksheet.Cells[i + 2, 84] = "";
                    Worksheet.Cells[i + 2, 85] = "";
                    Worksheet.Cells[i + 2, 86] = "0.5";
                    Worksheet.Cells[i + 2, 87] = "100";
                    Worksheet.Cells[i + 2, 88] = "100";
                    Worksheet.Cells[i + 2, 89] = "100";

                    Worksheet.Cells[i + 2, 90] = "42518.6622331366";
                    Worksheet.Cells[i + 2, 91] = "35;";
                    Worksheet.Cells[i + 2, 92] = "5;";   //Manufactory Code
                    Worksheet.Cells[i + 2, 93] = @"D:\home\site\wwwroot\content\images\thumbs\" + product.LocalPicture;
                    Worksheet.Cells[i + 2, 94] = "";
                    Worksheet.Cells[i + 2, 95] = "";
                    //Worksheet.Cells[i + 2, 96] = "";
                    //Worksheet.Cells[i + 2, 97] = "TRUE";
                    //Worksheet.Cells[i + 2, 98] = "TRUE";
                    //Worksheet.Cells[i + 2, 99] = "TRUE";
                    db.BeginTransaction();
                    product.IsSynced = true;
                    db.Save(product);
                    db.CommitTransaction();
                    ProgressChange((i + 1)*100/products.Count, "");
                }

            }
        }
    }
}
