using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NopImport.Model.Data;

namespace NopImport.Model.Map
{
    public class ProductMap : BaseMap<Product>
    {
        public ProductMap()
        {
            Map(q => q.Name).Length(255).Not.Nullable();
            Map(q => q.Url).Length(500).Not.Nullable();
            Map(q => q.MetaDescription).Length(1000).Nullable();
            Map(q => q.MetaKeywords).Length(500).Nullable();
            Map(q => q.ExternalStoreCode).Length(2).Nullable();
            Map(q => q.ExternalId).Length(10).Nullable();
            Map(q => q.Picture).Length(200).Nullable();
            Map(q => q.LocalPicture).Length(200).Nullable();
            Map(q => q.Manufacturer).Length(10).Nullable();
            Map(q => q.Category).Length(10).Nullable();
            Map(q => q.Price).Length(10).Default("0.00");
            Map(q => q.OriginalPrice).Length(10).Default("0.00");
            Map(q => q.Description).Length(10000).Nullable();
            Map(q => q.GeneralInfo).Length(10000).Nullable();
            Map(q => q.Miscellaneous).Length(10000).Nullable();
            Map(q => q.DrugInteractions).Length(10000).Nullable();
            Map(q => q.Warnings).Length(10000).Nullable();
            Map(q => q.CommonUses).Length(10000).Nullable();
            Map(q => q.Ingredients).Length(10000).Nullable();
            Map(q => q.Directions).Length(10000).Nullable();
            Map(q => q.Indications).Length(10000).Nullable();
            Map(q => q.IsUpdated).Not.Nullable().Default("0");
            Map(q => q.IsSynced).Not.Nullable().Default("0");
            Map(q => q.NopId).Length(10).Nullable();
        }
        
    }
}
