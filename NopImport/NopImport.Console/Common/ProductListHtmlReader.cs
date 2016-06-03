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

        private List<CategorySearchModel> _searchModels;


        protected List<CategorySearchModel> SearchModels
        {
            get
            {
                if (_searchModels == null)
                {
                    _searchModels = new List<CategorySearchModel>();
                }
                return _searchModels;
            }
        }

        protected ProductListHtmlReader(bool resetDb = false)
        {
            _resetDb = resetDb;
        }

        protected ProductListHtmlReader(List<CategorySearchModel> searchModels, bool resetDb = false)
        {
            _searchModels = searchModels;
            _resetDb = resetDb;
        }

        public void AddCategorySearchModel(CategorySearchModel model)
        {
            SearchModels.Add(model);
        }


        public override void Process()
        {
            var count = 0;
            foreach (var searchModel in SearchModels)
            {
                count ++;
                if (searchModel.PageSize > 0)
                {
                    using (var db = new DatabaseService("DefaultConnectionString", "NopImport"))
                    {
                        if (_resetDb)
                        {
                            db.ResetDatabase();
                        }

                        var percentage = 100 * count / SearchModels.Count;

                        for (int i = 1; i <= searchModel.PageSize; i++)
                        {
                            var catePageUrl = GetUrl(i, searchModel);
                            var htmlString = ReadHtml(catePageUrl);
                            var listResult = GetProductFromHtml(htmlString, searchModel).ToList();


                            for (int j = 0; j < listResult.Count; j++)
                            {
                                if (!db.Get(new IsProductUrlExist(listResult[j].Url)))
                                {
                                    var product = listResult[j];
                                    db.Save(product);
                                }



                            }

                            
                            ChangeProgress(i * percentage / searchModel.PageSize);
                        }




                        //Console.WriteLine(db.Get(new IsProductByExist("/buy/67491/Swisse-Ultiboost-Calcium-Vitamin-D-150-Tablets")));



                    }

                }
            }

            
        }

        private string GetUrl(int page, CategorySearchModel searchModel)
        {
            return string.Format(searchModel.UrlTemplate, page);
        }

        protected IEnumerable<Product> GetProductFromHtml(string html, CategorySearchModel searchModel)
        {
            var output = new List<Product>();
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(html);
            var nodes = htmlDocument.DocumentNode.GetNodesFromIdentifier(searchModel.ProductItemIdentifier);//("//*[@class='" + classValue + "']");

            foreach (var node in nodes)
            {
                output.Add(node.GetEntity(searchModel));
            }

            return output;
        }
    }
}
