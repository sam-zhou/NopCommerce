using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using NopImport.Model;
using NopImport.Model.Common;
using NopImport.Model.Data;
using NopImport.Model.SearchModel;

namespace NopImport.UrlSearcher
{
    public abstract class ProductListReader
    {
        public event EventHandler<ProgressChangedEventArgs> ProgressChanged; 
        
        protected CategorySearchModel CategorySearch;

        protected ProductListReader()
        {
            
        }

        protected ProductListReader(CategorySearchModel categorySearch)
        {
            CategorySearch = categorySearch;
        }

        public List<Product> Process()
        {
            var output = new List<Product>();
            if (CategorySearch.PageSize > 0)
            {
                for (int i = 1; i <= CategorySearch.PageSize; i++)
                {
                    var catePageUrl = GetUrl(i);
                    var htmlString = ReadHtml(catePageUrl);

                    output.AddRange(GetUrlsFromHtml(htmlString));

                    if (ProgressChanged != null)
                    {
                        ProgressChanged(this, new ProgressChangedEventArgs(i * 100 /CategorySearch.PageSize, null));
                    }
                }
            }
            return output;
        }

        private string GetUrl(int page)
        {
            return string.Format(CategorySearch.UrlTemplate, page);
        }

        protected IEnumerable<Product> GetUrlsFromHtml(string html)
        {
            var output = new List<Product>();
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(html);
            var nodes = GetNodesFromIdentifier(htmlDocument.DocumentNode, CategorySearch.ProductItemIdentifier);//("//*[@class='" + classValue + "']");

            foreach (var node in nodes)
            {
                var name = GetValueFromNode(node, CategorySearch.ProductNameIdentifier);
                var url = GetValueFromNode(node, CategorySearch.ProductUrlIdentifier);

                output.Add(new Product
                {
                    Name = name,
                    Url = CategorySearch.BaseUrl + url,
                });
            }

            return output;
        }

        protected string GetValueFromNode(HtmlNode node, Identifier identifier)
        {
            if (identifier.Type == IdentifierType.Attribute)
            {
                return node.Attributes[identifier.Value].Value;
            }
            else
            {
                return node.SelectSingleNode(identifier.Value).InnerText;
            }
        }

        protected HtmlNodeCollection GetNodesFromIdentifier(HtmlNode node, Identifier identifier)
        {
            if (identifier.Type == IdentifierType.Attribute)
            {
                throw new Exception("Cannot get HtmlNodeCollection from attribute");
            }

            return node.SelectNodes(identifier.Value);
        }

        private string ReadHtml(string url)
        {

            var request = WebRequest.Create(url);
            using (var response = request.GetResponse())
            {
                using (var responseStream = response.GetResponseStream())
                {
                    if (responseStream != null)
                    {
                        using (var sr = new StreamReader(responseStream))
                        {
                            var content = sr.ReadToEnd();

                            return content;
                        }
                    }
                }
            }
            return null;
        }
    }
}
