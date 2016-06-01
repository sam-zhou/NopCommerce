using HtmlAgilityPack;
using NopImport.Common.Services;
using NopImport.Model.Data;
using NopImport.Model.SearchModel;

namespace NopImport.Console.Common
{
    public abstract class ProductReader: AbstractReader
    {        
        protected ProductSearchModel ProductSearchModel;

        protected ProductReader()
        {
            
        }

        protected ProductReader(ProductSearchModel productSearchModel)
        {
            ProductSearchModel = productSearchModel;
        }

        public override void Process()
        {
            using (var db = new DatabaseService("DefaultConnectionString", "NopImport"))
            {
                var products = db.Session.QueryOver<Product>().Where(q => !q.IsSynced && !q.IsUpdated).List();
                for (int i = 0; i < products.Count; i++)
                {
                    db.BeginTransaction();
                    var product = products[i];
                    var htmlDocument = new HtmlDocument();
                    htmlDocument.LoadHtml(ReadHtml(ProductSearchModel.BaseUrl + product.Url));
                    var node = htmlDocument.DocumentNode;

                    product = node.GetEntity(ProductSearchModel, product);




                    product = ProcessItem(product);
                    












                    db.Session.Update(product);

                    db.CommitTransaction();
                    ChangeProgress((i + 1) * 100 / products.Count);
                }

            }
        }

        public virtual Product ProcessItem(Product product)
        {
            product.IsUpdated = true;
            return product;
        }
    }
}
