using NopImport.Model.Common;

namespace NopImport.Model.SearchModel
{
    public class CategorySearchModel : BaseSeachModel
    {
        public string Name { get; set; }

        public string UrlTemplate { get; set; }

        public int PageSize { get; set; }

        public string BaseUrl { get; set; }

        public Identifier ProductItemIdentifier { get; set; }

    }
}
