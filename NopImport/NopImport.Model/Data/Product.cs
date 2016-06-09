using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NopImport.Model.Data
{
    public class Product : BaseEntity
    {
        public virtual string Name { get; set; }

        public virtual string MetaDescription { get; set; }

        public virtual string MetaKeywords { get; set; }

        public virtual string Url { get; set; }

        public virtual string ExternalStoreCode { get; set; }

        public virtual string ExternalId { get; set; }

        public virtual string Picture { get; set; }

        public virtual string LocalPicture { get; set; }

        public virtual string Manufacturer { get; set; }

        public virtual string Category { get; set; }

        public virtual string Price { get; set; }

        public virtual string OriginalPrice { get; set; }

        public virtual string Description { get; set; }

        public virtual string GeneralInfo { get; set; }

        public virtual string Miscellaneous { get; set; }

        public virtual string DrugInteractions { get; set; }

        public virtual string Warnings { get; set; }

        public virtual string CommonUses { get; set; }

        public virtual string Ingredients { get; set; }

        public virtual string Directions { get; set; }

        public virtual string Indications { get; set; }

        public virtual bool IsUpdated { get; set; }

        public virtual string NopId { get; set; }
    }
}
