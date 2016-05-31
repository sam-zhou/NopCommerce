using NopImport.Model.Common;
using NopImport.Model.SearchModel;
using NopImport.UrlSearcher.Common;

namespace NopImport.UrlSearcher.ChemistWarehouse
{
    public class CWProductListReader : ProductListReader
    {
        
        public CWProductListReader(bool resetDb = false) : base(resetDb) 
        {
            //CategorySearch = new CategorySearchModel
            //{
            //    Name = "Chemist Warehouse",
            //    BaseUrl = "http://www.chemistwarehouse.com.au",
            //    UrlTemplate = "https://www.chemistwarehouse.com.au/Shop-Online/81/Vitamins?page={0}",
            //    PageSize = 61,
            //    ProductItemIdentifier = new Identifier
            //    {
            //        Type = IdentifierType.Text,
            //        Value = "//*[@class='product-container']"
            //    },
            //};

            CategorySearch = new CategorySearchModel
            {
                Name = "Chemist Warehouse",
                BaseUrl = "http://www.chemistwarehouse.com.au",
                UrlTemplate = "http://www.chemistwarehouse.com.au/Shop-Online/587/Swisse?page={0}",
                PageSize = 5,
                ProductItemIdentifier = new Identifier
                {
                    Type = IdentifierType.Text,
                    Value = "//*[@class='product-container']"
                },
            };

        
            CategorySearch.AddIdentifier("Name", IdentifierType.Text, ".//*[@class='product-name']");
            CategorySearch.AddIdentifier("Url", IdentifierType.Attribute, "href");
            CategorySearch.AddIdentifier("ExternalId", IdentifierType.Attribute, "value", ".//input", true);
            CategorySearch.AddIdentifier("Price", IdentifierType.Text, ".//*[@class='Price']", null, false, 1);
        }

        
    }
}
