using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NopImport.Common.Services
{
    public class ProductService: BaseService
    {
        public ProductService(IDatabaseService dbService) : base(dbService)
        {
        }
    }
}
