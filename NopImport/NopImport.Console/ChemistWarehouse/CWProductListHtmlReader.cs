using NopImport.Console.Common;
using NopImport.Model.Common;
using NopImport.Model.SearchModel;

namespace NopImport.Console.ChemistWarehouse
{
    public class CWProductListHtmlReader : ProductListHtmlReader
    {
        public CWProductListHtmlReader(bool resetDb = false)
            : base(resetDb)
        {
            //CategorySearch = new CategorySearchModel
            //{
            //    Name = "Chemist Warehouse",
            //    BaseUrl = "http://www.chemistwarehouse.com.au",
            //    UrlTemplate = "https://www.chemistwarehouse.com.au/Shop-Online/81/Vitamins?page={0}",
            //    PageSize = 61,
            //    ProductItemIdentifier = new Identifier
            //    {
            //        Type = IdentifierType.ElementContent,
            //        Value = "//*[@class='product-container']"
            //    },
            //};

            


            AddSearchModel("http://www.chemistwarehouse.com.au/Shop-Online/513/Blackmores?page={0}", 8, "5");
            AddSearchModel("http://www.chemistwarehouse.com.au/Shop-Online/587/Swisse?page={0}", 5, "4");
            AddSearchModel("http://www.chemistwarehouse.com.au/Shop-Online/722/Healthy-Care?page={0}", 5, "17");
            AddSearchModel("http://www.chemistwarehouse.com.au/Shop-Online/505/Nature-39-s-Own?page={0}", 4, "6");
            AddSearchModel("http://www.chemistwarehouse.com.au/Shop-Online/660/Nature-39-s-Way?page={0}", 5, "11");
            AddSearchModel("http://www.chemistwarehouse.com.au/Shop-Online/511/Cenovis?page={0}", 3, "8");
            AddSearchModel("http://www.chemistwarehouse.com.au/Shop-Online/519/Ethical-Nutrients?page={0}", 4, "9");

            AddSearchModel("http://www.chemistwarehouse.com.au/Shop-Online/547/Bioglan?page={0}", 4, "10");
            AddSearchModel("http://www.chemistwarehouse.com.au/Shop-Online/506/Bio-Organics?page={0}", 1, "7");
            AddSearchModel("http://www.chemistwarehouse.com.au/Shop-Online/889/Wagner?page={0}", 2, "12");


            AddSearchModel("http://www.chemistwarehouse.com.au/Shop-Online/958/Aptamil?page={0}", 1, "15", "37");
            AddSearchModel("http://www.chemistwarehouse.com.au/Shop-Online/1267/Bellamy-39-s-Organic-Formula?page={0}", 1, "16", "37");
            


        }


        private void AddSearchModel(string urlTemplate, int pageSize, string manufacturer , string category = "35")
        {
            var ethicalNutrients = new CategorySearchModel
            {
                Name = "Chemist Warehouse",
                BaseUrl = "http://www.chemistwarehouse.com.au",
                UrlTemplate = urlTemplate,
                PageSize = pageSize,
                ProductItemIdentifier = new Identifier
                {
                    Type = IdentifierType.ElementContent,
                    Value = "//*[@class='product-container']"
                },
            };
            ethicalNutrients.AddIdentifier("Category", IdentifierType.Text, category);
            ethicalNutrients.AddIdentifier("Manufacturer", IdentifierType.Text, manufacturer);
            ethicalNutrients.AddIdentifier("ExternalStoreCode", IdentifierType.Text, "CW");
            ethicalNutrients.AddIdentifier("Name", IdentifierType.ElementContent, ".//*[@class='product-name']");
            ethicalNutrients.AddIdentifier("Url", IdentifierType.Attribute, "href");
            ethicalNutrients.AddIdentifier("ExternalId", IdentifierType.Attribute, "value", ".//input", true);
            ethicalNutrients.AddIdentifier("Price", IdentifierType.ElementContent, ".//*[@class='Price']", null, false, 1);
            SearchModels.Add(ethicalNutrients);
        }

        //public CWProductListHtmlReader(bool resetDb = false)
        //    : base(resetDb)
        //{


        //}
    }
}
