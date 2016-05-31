using NopImport.Model.Common;
using NopImport.Model.SearchModel;

namespace NopImport.UrlSearcher.ChemistWarehouse
{
    public class CWProductListReader : ProductListReader
    {
        
        public CWProductListReader() 
        {
            CategorySearch = new CategorySearchModel
            {
                Name = "Chemist Warehouse",
                BaseUrl = "http://www.chemistwarehouse.com.au",
                UrlTemplate = "https://www.chemistwarehouse.com.au/Shop-Online/81/Vitamins?page={0}",
                PageSize = 61,
                ProductItemIdentifier = new Identifier
                {
                    Type = IdentifierType.Text,
                    Value = "//*[@class='product-container']"
                },
            };

            CategorySearch.AddIdentifier("Name", IdentifierType.Text, ".//*[@class='product-name']");
            CategorySearch.AddIdentifier("Url", IdentifierType.Attribute, "href");
        }

        
    }
}
