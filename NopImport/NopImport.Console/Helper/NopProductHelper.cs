using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Media;
using Nop.Services.Seo;
using NopProduct = Nop.Core.Domain.Catalog.Product;
using Product = NopImport.Model.Data.Product;

namespace NopImport.Console.Helper
{
    public static class NopProductHelper
    {
        public static void UpdateFrom(this NopProduct nopProduct, Product product)
        {
            nopProduct.Name = product.Name;
            nopProduct.MetaDescription = product.MetaDescription;
            nopProduct.MetaKeywords = product.MetaKeywords;
            nopProduct.ManageInventoryMethod = ManageInventoryMethod.ManageStock;
            nopProduct.StockQuantity = 10;
            nopProduct.MetaTitle = product.Name;
            nopProduct.Price = decimal.Parse(product.Price);
            nopProduct.OldPrice = decimal.Parse(product.OriginalPrice);
            nopProduct.ShortDescription = product.MetaDescription;
            nopProduct.FullDescription = product.GeneralInfo;
            nopProduct.ProductType = ProductType.SimpleProduct;
            nopProduct.Published = true;
            nopProduct.VisibleIndividually = true;
            nopProduct.AllowCustomerReviews = true;
            nopProduct.Sku = string.Format("{0}-{1}", product.ExternalStoreCode, product.ExternalId);
            

            if (nopProduct.CreatedOnUtc == DateTime.MinValue)
            {
                nopProduct.CreatedOnUtc = DateTime.UtcNow;
            }

            nopProduct.UpdatedOnUtc = DateTime.UtcNow;
        }
    }
}
