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

        public virtual string Url { get; set; }

        public virtual bool IsSynced { get; set; }
    }
}
