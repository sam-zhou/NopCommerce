using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;
using NopImport.Common.Services;
using NopImport.Common.Services.Queries;
using NopImport.Model.Data;
using NopImport.Model.SearchModel;

namespace NopImport.Console.Common
{
    public abstract class ProductListHtmlReader : AbstractHtmlReader
    {
        private readonly bool _resetDb;

        protected CategorySearchModel CategorySearch;

        protected ProductListHtmlReader(bool resetDb = false)
        {
            _resetDb = resetDb;
        }

        protected ProductListHtmlReader(CategorySearchModel categorySearchModel, bool resetDb = false) : this(resetDb)
        {
            CategorySearch = categorySearchModel;
            
        }


        public override void Process()
        {
            if (CategorySearch.PageSize > 0)
            {
                using (var db = new DatabaseService("DefaultConnectionString", "NopImport"))
                {
                    if (_resetDb)
                    {
                        db.ResetDatabase();
                    }
                    

                    for (int i = 1; i <= CategorySearch.PageSize; i++)
                    {
                        var catePageUrl = GetUrl(i);
                        var htmlString = ReadHtml(catePageUrl);
                        var listResult = GetProductFromHtml(htmlString).ToList();
                        

                        for (int j = 0; j < listResult.Count; j++)
                        {
                            if (!db.Get(new IsProductUrlExist(listResult[j].Url)))
                            {
                                var product = listResult[j];
                                db.Save(product);
                            }
                            
                            

                        }
                        ChangeProgress(i * 100 / CategorySearch.PageSize);
                    }


                    

                    //Console.WriteLine(db.Get(new IsProductByExist("/buy/67491/Swisse-Ultiboost-Calcium-Vitamin-D-150-Tablets")));

                    

                }

            }
        }

        private string GetUrl(int page)
        {
            return string.Format(CategorySearch.UrlTemplate, page);
        }

        protected IEnumerable<Product> GetProductFromHtml(string html)
        {
            var output = new List<Product>();
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(html);
            var nodes = htmlDocument.DocumentNode.GetNodesFromIdentifier(CategorySearch.ProductItemIdentifier);//("//*[@class='" + classValue + "']");

            foreach (var node in nodes)
            {
                output.Add(node.GetEntity(CategorySearch));
            }

            return output;
        }
    }
}
