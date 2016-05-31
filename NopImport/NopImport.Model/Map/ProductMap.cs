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
            Map(q => q.IsSynced).Not.Nullable().Default("0");
        }
        
    }
}
