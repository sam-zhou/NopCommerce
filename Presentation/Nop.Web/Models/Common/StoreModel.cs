using Nop.Web.Framework.Mvc;

namespace Nop.Web.Models.Common
{
    public partial class StoreModel : BaseNopEntityModel
    {
        public string Name { get; set; }

        public string Url { get; set; }

        public string Country { get; set; }

    }
}