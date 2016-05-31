using NopImport.Model.Common;
using NopImport.Model.Data;

namespace NopImport.Model.SearchModel
{
    public class ProductSearchModel: BaseSeachModel<Product> 
    {
        public string BaseUrl { get; set; }
    }
}
