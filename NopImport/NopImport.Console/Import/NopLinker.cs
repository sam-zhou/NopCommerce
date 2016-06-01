using System;
using Nop.Core.Infrastructure;
using Nop.Services.Catalog;
using Nop.Services.Directory;
using NopImport.Common.Services;
using NopImport.Console.Helper;
using NopImport.Model.Data;
using SevenSpikes.Nop.Plugins.NopQuickTabs.Services;
using NopProduct = Nop.Core.Domain.Catalog.Product;

namespace NopImport.Console.Import
{
    public class NopLinker
    {
        public NopLinker()
        {
            EngineContext.Initialize(false);
            

        }


        public void ImportItems()
        {
            using (var db = new DatabaseService("DefaultConnectionString", "NopImport"))
            {
                var productService = EngineContext.Current.Resolve<IProductService>();
                var products = db.Session.QueryOver<Product>().Where(q => q.IsUpdated && !q.IsSynced).List();
                foreach (var product in products)
                {
                    try
                    {
                        var nopProduct =
                            productService.GetProductBySku(string.Format("{0}-{1}", product.ExternalStoreCode,
                                product.ExternalId));
                        if (nopProduct == null)
                        {
                            nopProduct = new NopProduct();
                            nopProduct.UpdateFrom(product);

                            productService.InsertProduct(nopProduct);
                        }
                        else
                        {
                            System.Console.WriteLine("product exists");
                            //nopProduct.UpdateFrom(product);
                            //productService.UpdateProduct(nopProduct);
                        }


                        db.BeginTransaction();
                        product.IsSynced = true;
                        db.Session.Save(product);
                        db.CommitTransaction();
                    }
                    catch (Exception ex)
                    {
                        System.Console.WriteLine("Error when syncing product to NopCommerce, Rolling back");
                        db.RollBackTransaction();
                        throw ex;
                    }
                    
                    
                }
            }
        }
    }
}
