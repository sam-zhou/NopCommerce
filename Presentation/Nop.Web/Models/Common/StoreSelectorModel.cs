using System.Collections.Generic;
using Nop.Web.Framework.Mvc;

namespace Nop.Web.Models.Common
{
    public partial class StoreSelectorModel : BaseNopModel
    {
        public StoreSelectorModel()
        {
            AvailableStores = new List<StoreModel>();
        }

        public IList<StoreModel> AvailableStores { get; set; }

        public int CurrentStoreId { get; set; }
    }
}